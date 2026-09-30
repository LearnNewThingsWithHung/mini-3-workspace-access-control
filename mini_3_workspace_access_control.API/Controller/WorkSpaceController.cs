using Microsoft.AspNetCore.Mvc;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using WorkspaceService = mini_3_workspace_access_control.Service.WorkSpace;
using WorkspaceMemberService = mini_3_workspace_access_control.Service.WorkSpaceMember;

namespace mini_3_workspace_access_control.API.Controller;

[ApiController]
[Route("api/workspaces")]
public sealed class WorkSpaceController : ControllerBase
{
    private readonly WorkspaceService.IService _workspaces;
    private readonly WorkspaceMemberService.IService _members;

    public WorkSpaceController(
        WorkspaceService.IService workspaces,
        WorkspaceMemberService.IService members)
    {
        _workspaces = workspaces;
        _members = members;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] WorkspaceService.Request.CreateWorkSpaceRequest request,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _workspaces.CreateWorkSpaceAsync(request, currentPersonId, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { workspaceId = result.Id },
            ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _workspaces.GetMineAsync(currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{workspaceId:guid}")]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _workspaces.GetAsync(workspaceId, currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{workspaceId:guid}")]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        [FromBody] WorkspaceService.Request.UpdateWorkSpaceRequest request,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _workspaces.UpdateAsync(workspaceId, request, currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{workspaceId:guid}")]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        await _workspaces.DeleteAsync(workspaceId, currentPersonId, ct);

        return NoContent();
    }

    [HttpGet("{workspaceId:guid}/members")]
    public async Task<IActionResult> GetMembers(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _members.GetMembersAsync(workspaceId, currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{workspaceId:guid}/members/{personId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRole(
        Guid workspaceId,
        Guid personId,
        [FromQuery] Guid currentPersonId,
        [FromBody] WorkspaceMemberService.Request.UpdateWorkSpaceMemberRoleRequest request,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var result = await _members.UpdateMemberRoleAsync(
            workspaceId,
            personId,
            request.Role,
            currentPersonId,
            ct);

        return Ok(ApiResponseFactory.Base(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{workspaceId:guid}/members/{personId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid workspaceId,
        Guid personId,
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        await _members.RemoveMemberAsync(workspaceId, personId, currentPersonId, ct);

        return NoContent();
    }

    [HttpPost("{workspaceId:guid}/ownership-transfer")]
    public async Task<IActionResult> TransferOwnership(
        Guid workspaceId,
        [FromQuery] Guid currentPersonId,
        [FromBody] WorkspaceMemberService.Request.TransferWorkSpaceOwnershipRequest request,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        await _members.TransferOwnershipAsync(
            workspaceId,
            request.NewOwnerPersonId,
            request.PreviousOwnerRole,
            currentPersonId,
            ct);

        return NoContent();
    }

    private static void EnsureValidDemoPersonId(Guid currentPersonId)
    {
        if (currentPersonId == Guid.Empty)
            throw new DemoPersonUnauthorizedException();
    }
}
