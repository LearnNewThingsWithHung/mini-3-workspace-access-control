using Microsoft.AspNetCore.Mvc;
using mini_3_workspace_access_control.Repo.Enum;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using InvitationService = mini_3_workspace_access_control.Service.Invitation;

namespace mini_3_workspace_access_control.API.Controller;

[ApiController]
[Route("api")]
public sealed class WorkspaceInvitationController : ControllerBase
{
    private const string DemoPersonHeader = "X-Demo-Person-Id";
    private readonly InvitationService.IService _invitations;

    public WorkspaceInvitationController(InvitationService.IService invitations)
    {
        _invitations = invitations;
    }

    [HttpPost("workspaces/{workspaceId:guid}/invitations")]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        [FromBody] InvitationService.Response.CreateWorkspaceInvitationBody body,
        CancellationToken ct)
    {
        var currentPersonId = GetCurrentPersonId();
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
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentPersonId = GetCurrentPersonId();
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
        [FromBody] InvitationService.Request.AcceptInvitationRequest request,
        CancellationToken ct)
    {
        var currentPersonId = GetCurrentPersonId();
        var result = await _invitations.AcceptInvitation(request, currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    private Guid GetCurrentPersonId()
    {
        var value = Request.Headers[DemoPersonHeader].FirstOrDefault();

        if (!Guid.TryParse(value, out var personId) || personId == Guid.Empty)
            throw new DemoPersonUnauthorizedException();

        return personId;
    }
}

