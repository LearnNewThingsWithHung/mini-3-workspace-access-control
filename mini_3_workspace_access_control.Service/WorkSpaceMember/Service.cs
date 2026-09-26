using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Enum;

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
            throw new Exception($"Workspace member {workspaceId} not found");

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
            throw new Exception($"Workspace member {workspaceId} not found");

        if (currentMember.Role is not (WorkspaceRole.Owner or WorkspaceRole.Manager))
            throw new Exception($"Workspace member {workspaceId} is not owner");

        var targetMember = await _dbContext.WorkspaceMembers
            .Include(x => x.Person)
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == targetPersonId, ct);

        if (targetMember is null)
            throw new Exception("Person not found int this workspace");

        if (targetMember.Role == WorkspaceRole.Owner)//mất owner cho that workspace
            throw new Exception("Không thể đổi role Owner tại đây. Hãy dùng ownership transfer.");

        if (currentMember.Role == WorkspaceRole.Manager &&
            requestedRole == WorkspaceRole.Owner)
            throw new Exception("Không có quyền đổi từ Manager -> Onwer");

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
            throw new Exception("Member is not in this workspace");

        if (currentMember.Role is not (WorkspaceRole.Owner or WorkspaceRole.Manager))
            throw new Exception("You have to owner or manager role");

        if (targetPersonId == currentPersonId)
            throw new Exception("Không thể tự rời workspace bằng API này.");

        var targetMember = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == targetPersonId, ct);

        if (targetMember is null)
            throw new Exception("Member is not in this workspace");

        if (targetMember.Role == WorkspaceRole.Owner)
            throw new Exception("Không thể xóa Owner. Hãy chuyển quyền sở hữu trước.");

        _dbContext.WorkspaceMembers.Remove(targetMember);
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
            throw new Exception("Bạn đã là Owner hiện tại.");

        if (previousOwnerRole == WorkspaceRole.Owner)
            throw new Exception("Previous owner cannot remain Owner after ownership transfer.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var currentOwner = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == currentPersonId, ct);

        if (currentOwner is null)
            throw new Exception("Current user is not in this workspace");

        if (currentOwner.Role != WorkspaceRole.Owner)
            throw new Exception("Just owner can able to use this API");

        var newOwner = await _dbContext.WorkspaceMembers
            .SingleOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == newOwnerPersonId, ct);

        if (newOwner is null)
            throw new Exception("Current user is not in this workspace");
        
        currentOwner.Role = previousOwnerRole;
        newOwner.Role = WorkspaceRole.Owner;

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
