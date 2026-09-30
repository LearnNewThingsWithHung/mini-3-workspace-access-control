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
    public async Task<IActionResult> GetMe(Guid personId, CancellationToken ct)
    {
        var person = await _people.GetMeAsync(personId, ct);

        return Ok(ApiResponseFactory.Base(
            person,
            traceId: HttpContext.TraceIdentifier));
    }
    
}
