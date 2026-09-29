using Microsoft.AspNetCore.Mvc;
using mini_3_workspace_access_control.Service.Exceptions;
using mini_3_workspace_access_control.Service.Models;
using PersonAccess = mini_3_workspace_access_control.Service.PersonAccess;

namespace mini_3_workspace_access_control.API.Controller;

[ApiController]
[Route("api/people")]
public sealed class PersonController : ControllerBase
{
    private const string DemoPersonHeader = "X-Demo-Person-Id";
    private readonly PersonAccess.IService _people;

    public PersonController(PersonAccess.IService people)
    {
        _people = people;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var personId = GetCurrentPersonId();
        var person = await _people.GetMeAsync(personId, ct);

        return Ok(ApiResponseFactory.Base(
            person,
            traceId: HttpContext.TraceIdentifier));
    }

    private Guid GetCurrentPersonId()
    {
        var value = Request.Headers[DemoPersonHeader].FirstOrDefault();

        if (!Guid.TryParse(value, out var personId) || personId == Guid.Empty)
            throw new DemoPersonUnauthorizedException();

        return personId;
    }
}
