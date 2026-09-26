namespace mini_3_workspace_access_control.Service.WorkSpace;

public interface IService
{
    Task<Response.WorkSpaceResponse> CreateWorkSpaceAsync(
        Request.CreateWorkSpaceRequest request, Guid currentUserId, CancellationToken ct = default);
    
    Task<IReadOnlyList<Response.WorkspaceSummaryResponse>> GetMineAsync(
        Guid currentUserId,
        CancellationToken ct = default);
    
    Task<Response.WorkSpaceResponse> GetAsync(Guid workspaceId, Guid currentPersonId, CancellationToken ct = default);
    
    Task<Response.WorkSpaceResponse> UpdateAsync(Guid workspaceId, Request.UpdateWorkSpaceRequest request, 
        Guid currentPersonId,
        CancellationToken ct = default);
    
    Task DeleteAsync(Guid workspaceId, Guid currentPersonId,
        CancellationToken ct = default);

}