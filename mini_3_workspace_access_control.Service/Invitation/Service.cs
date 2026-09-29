using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Entity;
using mini_3_workspace_access_control.Repo.Enum;
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
        
        var email = request.Email.Trim().ToLowerInvariant();
        
        if(!Enum.IsDefined(request.Role))
            throw new Exception("Invalid role");
        
        var inventer = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => 
                x.PersonId == request.CurrentPersonId  &&
                x.WorkspaceId == request.WorkspaceId, ct);

        if (inventer == null)
        {
            throw new Exception($"Member {request.CurrentPersonId} does not exist in this workspace");
        }

        if (!inventer.Role.Covers(WorkspaceRole.Manager))
        {
            throw new Exception("Only Owner or Manager can create invitation");
        }

        if (!WorkspaceRole.Manager.Covers(request.Role))
        {
            throw new Exception("An invitation cannot assign the Owner role");
        }
        
        var alreadyMember = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .AnyAsync(x => x.WorkspaceId == request.WorkspaceId &&
                           x.Person.Email == email,ct);

        if (alreadyMember)
        {
            throw new Exception($"Member {request.CurrentPersonId} already existed in this workspace");
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
            throw new Exception($"A pending invitation with the email {email} already exists");
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
            
        _dbContext.WorkspaceInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(ct);
        
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
            throw new Exception("Page index must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new Exception("Page size must be between 1 and 100.");
        
        var currentMember = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId &&
                           x.PersonId == currentPersonId, ct);

        if (currentMember is null)
        {
            throw new Exception("Current person is not a member of this workspace");
        }

        if (!currentMember.Role.Covers(WorkspaceRole.Manager))
        {
            throw new Exception("Only Owner or Manager can seen invited");
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

    private static (string RawToken, string TokenHash) CreateInvitationToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(randomBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        var tokenHash = Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();
        
        return(rawToken, tokenHash);
    }
}