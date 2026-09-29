using mini_3_workspace_access_control.Service.Models;

namespace mini_3_workspace_access_control.Service.Invitation;

public interface IService
{
    Task<Response.CreateInvitationResponse> CreateInvitation(Request.CreateInvitationRequest request, CancellationToken ct);
    Task<BasePaginationResponse> GetInvitation(
        Guid workspaceId, Guid currentPersonId, int pageSize, int pageIndex, CancellationToken ct);
    Task<Response.AcceptInvitationResponse> AcceptInvitation(
        Request.AcceptInvitationRequest request, Guid currentPersonId,CancellationToken ct);
} 