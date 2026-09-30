using Microsoft.AspNetCore.Mvc;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using InvitationService = mini_3_workspace_access_control.Service.Invitation;

namespace mini_3_workspace_access_control.API.Controller;

[ApiController]
[Route("api")]
public sealed class WorkspaceInvitationController : ControllerBase
{
    private readonly InvitationService.IService _invitations;

    public WorkspaceInvitationController(InvitationService.IService invitations)
    {
        _invitations = invitations;
    }

    [HttpPost("workspaces/{workspaceId:guid}/invitations")]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        [FromBody] InvitationService.Request.CreateWorkspaceInvitationBody body,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);

        var request = new InvitationService.Request.CreateInvitationRequest
        {
            WorkspaceId = workspaceId,
            CurrentPersonId = currentPersonId,
            Email = body.Email,
            Role = body.Role
        };

        var result = await _invitations.CreateInvitation(request, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("workspaces/{workspaceId:guid}/invitations")]
    public async Task<IActionResult> GetInvitations(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        EnsureValidDemoPersonId(currentPersonId);

        var result = await _invitations.GetInvitation(
            workspaceId,
            currentPersonId,
            pageSize,
            pageIndex,
            ct);

        result.TraceId = HttpContext.TraceIdentifier;
        result.TimestampUtc = DateTime.UtcNow;

        return Ok(result);
    }

    [HttpPost("invitations/accept")]
    public async Task<IActionResult> Accept(
        [FromQuery] Guid currentPersonId,
        [FromBody] InvitationService.Request.AcceptInvitationRequest request,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);

        var result = await _invitations.AcceptInvitation(request, currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    private static void EnsureValidDemoPersonId(Guid currentPersonId)
    {
        if (currentPersonId == Guid.Empty)
            throw new DemoPersonUnauthorizedException();
    }
}
