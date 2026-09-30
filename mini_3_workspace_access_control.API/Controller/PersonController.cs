using Microsoft.AspNetCore.Mvc;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using PersonAccess = mini_3_workspace_access_control.Service.PersonAccess;

namespace mini_3_workspace_access_control.API.Controller;

[ApiController]
[Route("api/people")]
public sealed class PersonController : ControllerBase
{
    private readonly PersonAccess.IService _people;

    public PersonController(PersonAccess.IService people)
    {
        _people = people;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(
        [FromQuery] Guid currentPersonId,
        CancellationToken ct)
    {
        EnsureValidDemoPersonId(currentPersonId);
        var person = await _people.GetMeAsync(currentPersonId, ct);

        return Ok(ApiResponseFactory.Base(
            person,
            traceId: HttpContext.TraceIdentifier));
    }

    private static void EnsureValidDemoPersonId(Guid currentPersonId)
    {
        if (currentPersonId == Guid.Empty)
            throw new DemoPersonUnauthorizedException();
    }
}
