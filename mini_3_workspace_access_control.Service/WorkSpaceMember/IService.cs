using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.WorkSpaceMember;

public interface IService
{
    Task<IReadOnlyList<Response.WorkSpaceMemberResponse>> GetMembersAsync(
        Guid workspaceId,
        Guid currentPersonId,
        CancellationToken ct = default);
    
    Task<Response.WorkSpaceMemberResponse> UpdateMemberRoleAsync(
        Guid workspaceId,
        Guid targetPersonId,
        WorkspaceRole requestedRole,
        Guid currentPersonId,
        CancellationToken ct = default);
    
    Task RemoveMemberAsync(
        Guid workspaceId,
        Guid targetPersonId,
        Guid currentPersonId,
        CancellationToken ct = default);
    
    Task TransferOwnershipAsync(
        Guid workspaceId,
        Guid newOwnerPersonId,
        WorkspaceRole previousOwnerRole,
        Guid currentPersonId,
        CancellationToken ct = default);
}
