using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Enum;
using mini_3_workspace_access_control.Service.Exceptions;

namespace mini_3_workspace_access_control.Service.WorkSpaceMember;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IReadOnlyList<Response.WorkSpaceMemberResponse>> GetMembersAsync(Guid workspaceId, Guid currentPersonId, CancellationToken ct = default)
    {
        var isMember = await _dbContext.WorkspaceMembers
            .AnyAsync(x => x.WorkspaceId == workspaceId
                           && x.PersonId == currentPersonId, ct);

        if (!isMember)
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");

        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderBy(x => x.Role)
            .ThenBy(x => x.Person.DisplayName)
            .Select(x => new Response.WorkSpaceMemberResponse(){
                    PersonId = x.PersonId,
                    Email = x.Person.Email,
                    DisplayName = x.Person.DisplayName,
                    Role = x.Role,
                    JoinedAt = x.CreatedAt 
            }).ToListAsync(ct);
    }

    public async Task<Response.WorkSpaceMemberResponse> UpdateMemberRoleAsync(Guid workspaceId, Guid targetPersonId, WorkspaceRole requestedRole, Guid currentPersonId,
        CancellationToken ct = default)
    {
        var currentMember = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == currentPersonId, ct);

        if (currentMember is null)
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");

        if (currentMember.Role is not (WorkspaceRole.Owner or WorkspaceRole.Manager))
            throw new ForbiddenException("MEMBER_ROLE_UPDATE_FORBIDDEN", "Only the Owner or a Manager can update member roles.");

        var targetMember = await _dbContext.WorkspaceMembers
            .Include(x => x.Person)
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == targetPersonId, ct);

        if (targetMember is null)
            throw new NotFoundException("WORKSPACE_MEMBER_NOT_FOUND", "The member was not found in this workspace.");

        if (targetMember.Role == WorkspaceRole.Owner)//mất owner cho that workspace
            throw new BadRequestException("OWNER_ROLE_REQUIRES_TRANSFER", "The Owner role must be changed through ownership transfer.");

        if (!Enum.IsDefined(requestedRole))
            throw new BadRequestException("INVALID_WORKSPACE_ROLE", "The requested workspace role is invalid.");

        if (requestedRole == WorkspaceRole.Owner)
            throw new BadRequestException("OWNER_ROLE_REQUIRES_TRANSFER", "The Owner role can only be assigned through ownership transfer.");

        targetMember.Role = requestedRole;

        await _dbContext.SaveChangesAsync(ct);

        return new Response.WorkSpaceMemberResponse
        {
            PersonId = targetMember.PersonId,
            Email = targetMember.Person.Email,
            DisplayName = targetMember.Person.DisplayName,
            Role = targetMember.Role,
            JoinedAt = targetMember.CreatedAt
        };

    }

    public async Task RemoveMemberAsync(Guid workspaceId, Guid targetPersonId, Guid currentPersonId, CancellationToken ct = default)
    {
        var currentMember = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == currentPersonId, ct);

        if (currentMember is null)
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");

        if (currentMember.Role is not (WorkspaceRole.Owner or WorkspaceRole.Manager))
            throw new ForbiddenException("MEMBER_REMOVE_FORBIDDEN", "Only the Owner or a Manager can remove members.");

        if (targetPersonId == currentPersonId)
            throw new BadRequestException("SELF_REMOVE_NOT_ALLOWED", "You cannot leave the workspace through this administration endpoint.");

        var targetMember = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == targetPersonId, ct);

        if (targetMember is null)
            throw new NotFoundException("WORKSPACE_MEMBER_NOT_FOUND", "The member was not found in this workspace.");

        if (targetMember.Role == WorkspaceRole.Owner)
            throw new BadRequestException("OWNER_REMOVE_NOT_ALLOWED", "Transfer ownership before removing the current Owner.");

        targetMember.IsDeleted = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task TransferOwnershipAsync(
        Guid workspaceId,
        Guid newOwnerPersonId,
        WorkspaceRole previousOwnerRole,
        Guid currentPersonId,
        CancellationToken ct = default)
    {
        if (newOwnerPersonId == currentPersonId)
            throw new BadRequestException("NEW_OWNER_IS_CURRENT_OWNER", "The selected person is already the current Owner.");

        if (!Enum.IsDefined(previousOwnerRole) || previousOwnerRole == WorkspaceRole.Owner)
            throw new BadRequestException("INVALID_PREVIOUS_OWNER_ROLE", "The previous Owner must become Manager, Editor, or Viewer.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var currentOwner = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == currentPersonId, ct);

        if (currentOwner is null)
            throw new NotFoundException("WORKSPACE_NOT_FOUND", "Workspace was not found.");

        if (currentOwner.Role != WorkspaceRole.Owner)
            throw new ForbiddenException("OWNERSHIP_TRANSFER_FORBIDDEN", "Only the current Owner can transfer ownership.");

        var newOwner = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == newOwnerPersonId, ct);

        if (newOwner is null)
            throw new NotFoundException("NEW_OWNER_NOT_FOUND", "The new Owner must already be a member of this workspace.");
        
        currentOwner.Role = previousOwnerRole;
        newOwner.Role = WorkspaceRole.Owner;

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
