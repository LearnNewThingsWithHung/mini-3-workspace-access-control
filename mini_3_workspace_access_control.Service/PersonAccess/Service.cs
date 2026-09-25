using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Service.Exceptions;

namespace mini_3_workspace_access_control.Service.PersonAccess;

public class Service: IService
{
    
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Response.AccessPersonResponse> GetMeAsync(
        Guid personId,
        CancellationToken ct = default)
    {
        var person = await _dbContext.People
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == personId, ct);

        if (person == null)
        {
            throw new Exception("Person not found");
        }

        if (!person.IsActive)
        {
            throw new Exception("Person is not active");
        }

        return new Response.AccessPersonResponse
        {
            Id = person.Id,
            Email = person.Email,
            DisplayName = person.DisplayName,
            IsActive = person.IsActive,
        };
    }
}
