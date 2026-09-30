using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Entity;
using mini_3_workspace_access_control.Repo.Enum;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using mini_3_workspace_access_control.Service.Utils;

namespace mini_3_workspace_access_control.Service.Invitation;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly MailService.IService _mailService;
    
    public Service(AppDbContext dbContext, MailService.IService mailService)   
    {
        _dbContext = dbContext;
        _mailService = mailService;
    }
    
    public async Task<Response.CreateInvitationResponse> CreateInvitation(Request.CreateInvitationRequest request, CancellationToken ct)
    {
        
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("INVITATION_EMAIL_REQUIRED", "Invitation email is required.");

        var email = request.Email.Trim().ToLowerInvariant();

        if (!MailAddress.TryCreate(email, out var parsedEmail) ||
            !string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("INVALID_INVITATION_EMAIL", "Invitation email is invalid.");
        }
        
        if(!Enum.IsDefined(request.Role))
            throw new BadRequestException("INVALID_WORKSPACE_ROLE", "The requested workspace role is invalid.");
        
        var inviter = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => 
                x.PersonId == request.CurrentPersonId  &&
                x.WorkspaceId == request.WorkspaceId, ct);

        if (inviter == null)
        {
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");
        }

        if (!inviter.Role.Covers(WorkspaceRole.Manager))
        {
            throw new ForbiddenException("INVITATION_CREATE_FORBIDDEN", "Only the Owner or a Manager can create invitations.");
        }

        if (!WorkspaceRole.Manager.Covers(request.Role))
        {
            throw new BadRequestException("OWNER_INVITATION_NOT_ALLOWED", "An invitation cannot assign the Owner role.");
        }
        
        var alreadyMember = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(x => x.WorkspaceId == request.WorkspaceId &&
                           x.Person.Email == email,ct);

        if (alreadyMember)
        {
            throw new ConflictException("WORKSPACE_MEMBER_ALREADY_EXISTS", $"{email} is already a member of this workspace.");
        }        
        
        var pendingInvitationExist = await _dbContext.WorkspaceInvitations
            .AsNoTracking()
            .AnyAsync( x=>
                x.WorkspaceId == request.WorkspaceId &&
                x.Email == email &&
                x.Status == InvitationStatus.Pending,
                ct);

        if (pendingInvitationExist)
        {
            throw new ConflictException("PENDING_INVITATION_ALREADY_EXISTS", $"A pending invitation already exists for {email}.");
        }

        var (rawToken, tokenHash) = CreateInvitationToken();

        var invitation = new WorkspaceInvitation
        {
            Id = Guid.NewGuid(),
            WorkspaceId = request.WorkspaceId,
            Email = email,
            Role = request.Role,
            Status = InvitationStatus.Pending,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            CreatedByPersonId = request.CurrentPersonId,
        };
        
        _dbContext.WorkspaceInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(ct);
        
        var acceptUrl =
            $"http://localhost:5173/invitations/accept" +
            $"?token={Uri.EscapeDataString(rawToken)}";

        await _mailService.SendMail(new MailService.MailContent
        {
            To = invitation.Email,
            Subject = "Workspace invitation",
            Body = $"""
                    <h2>You have been invited to a workspace</h2>
                    <p>Your assigned role is: <strong>{invitation.Role}</strong></p>
                    <p>This invitation expires at: {invitation.ExpiresAt:O}</p>
                    <a href="{acceptUrl}">Accept invitation</a>
                    """

        });
        
        
        return new Response.CreateInvitationResponse
        {
            Id = invitation.Id,
            WorkspaceId = invitation.WorkspaceId,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt,
        };
    }

    public async Task<BasePaginationResponse> GetInvitation(
        Guid workspaceId, Guid currentPersonId, 
        int pageSize, int pageIndex, CancellationToken ct)
    {
        
        if (pageIndex < 1)
            throw new BadRequestException("INVALID_PAGE_INDEX", "Page index must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException("INVALID_PAGE_SIZE", "Page size must be between 1 and 100.");
        
        var currentMember = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId &&
                           x.PersonId == currentPersonId, ct);

        if (currentMember is null)
        {
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");
        }

        if (!currentMember.Role.Covers(WorkspaceRole.Manager))
        {
            throw new ForbiddenException("INVITATION_LIST_FORBIDDEN", "Only the Owner or a Manager can view invitations.");
        }

        var invitationQuery = _dbContext.WorkspaceInvitations
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId);
        
        var totalCount  = await invitationQuery.CountAsync(ct);
        
        var items = await invitationQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)   
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.InvitationListItemResponse
            {
                Id = x.Id,
                WorkspaceId = x.WorkspaceId,
                Email = x.Email,
                Role = x.Role.ToString(),
                Status = x.Status.ToString(),
                CreatedByPersonId = x.CreatedByPersonId,
                CreatedByDisplayName = x.CreatedByPerson.DisplayName,
                ExpiresAt = x.ExpiresAt,
                AcceptedAt = x.AcceptedAt,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(ct);
        
        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);


    }

    public async Task<Response.AcceptInvitationResponse> AcceptInvitation(Request.AcceptInvitationRequest request, Guid currentPersonId, CancellationToken ct)
    {

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new BadRequestException("INVITATION_TOKEN_REQUIRED", "Invitation token is required.");
        }
        
        var rawToken = request.Token.Trim();
        var tokenHash = HashInvitationToken(rawToken);
        var now = DateTimeOffset.UtcNow;
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var invitation = await _dbContext.WorkspaceInvitations
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (invitation is null)
        {
            throw new BadRequestException("INVALID_INVITATION_TOKEN", "Invitation token is invalid.");
        }
        
        if (invitation.Status != InvitationStatus.Pending)
        {
            throw invitation.Status switch
            {
                InvitationStatus.Accepted =>
                    new ConflictException("INVITATION_ALREADY_ACCEPTED", "Invitation has already been accepted."),

                InvitationStatus.Expired =>
                    new GoneException("INVITATION_EXPIRED", "Invitation has expired."),

                InvitationStatus.Revoked =>
                    new GoneException("INVITATION_REVOKED", "Invitation has been revoked."),

                _ => new ConflictException("INVITATION_UNAVAILABLE", "Invitation is not available.")
            };
        }

        if (invitation.ExpiresAt <= now)
        {
            throw new GoneException("INVITATION_EXPIRED", "Invitation has expired.");
        }
        
        var currentPerson = await _dbContext.People
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == currentPersonId &&
                x.IsActive, ct);
        
        if (currentPerson is null)
            throw new DemoPersonUnauthorizedException();

        if (!string.Equals(
                currentPerson.Email,
                invitation.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "INVITATION_EMAIL_MISMATCH",
                "This invitation belongs to another email address.");
        }

        var alreadyMember = await _dbContext.WorkspaceMembers
            .AnyAsync(x =>
                    x.WorkspaceId == invitation.WorkspaceId &&
                    x.PersonId == currentPersonId,
                ct);

        if (alreadyMember)
            throw new ConflictException(
                "WORKSPACE_MEMBER_ALREADY_EXISTS",
                "Current person is already a member of this workspace.");
        
        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = invitation.WorkspaceId,
            PersonId = currentPersonId,
            Role = invitation.Role
        };

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = now;

        _dbContext.WorkspaceMembers.Add(member);

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new Response.AcceptInvitationResponse
        {
            InvitationId = invitation.Id,
            WorkspaceId = invitation.WorkspaceId,
            PersonId = currentPersonId,
            Role = member.Role.ToString(),
            AcceptedAt = now
        };
    }

    private static string HashInvitationToken(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        
        return Convert.ToHexString(hashBytes)
            .ToLowerInvariant();
    }
    
    private static (string RawToken, string TokenHash) CreateInvitationToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(randomBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        
        var tokenHash = HashInvitationToken(rawToken);
        
        return(rawToken, tokenHash);
    }
}
