using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Entity;
using mini_3_workspace_access_control.Repo.Enum;
using mini_3_workspace_access_control.Service.Utils;

namespace mini_3_workspace_access_control.Service.WorkSpace;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Response.WorkSpaceResponse> CreateWorkSpaceAsync(
        Request.CreateWorkSpaceRequest request,  Guid currentPersonId, CancellationToken ct = default)
    {
        
        var person = await _dbContext.People
            .FirstOrDefaultAsync(x => x.Id == currentPersonId, ct);

        if (person == null || !person.IsActive)
        {
            throw new Exception("Person not found or not active");
        }
        
        var name = request.Name.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description) 
            ? null : request.Description.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exception("Name is required");
        }

        var workspace = new Workspace()
        {
            Id = Guid.NewGuid(),
            Code = WorkspaceCodeGenerator.Generate(),
            Name = name,
            Description = description,
            CreatedByPersonId = currentPersonId,
        };

        var membership = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            PersonId = person.Id,
            Role = WorkspaceRole.Owner
        };
        
        _dbContext.Workspaces.Add(workspace);
        _dbContext.WorkspaceMembers.Add(membership);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception)
            when (IsWorkspaceCodeConflict(exception))
        {
            throw new Exception("Conflict code work space");
        }

        return new Response.WorkSpaceResponse
        {
            Id = workspace.Id,
            Code = workspace.Code,
            Name = workspace.Name,
            Description = workspace.Description,
            CreatedByPersonId = workspace.CreatedByPersonId,
            MyRole = membership.Role,
            CreatedAt = workspace.CreatedAt
        };
        
    }

    public async Task<IReadOnlyList<Response.WorkspaceSummaryResponse>> GetMineAsync(Guid currentPersonId, CancellationToken ct = default)
    {
        var person = await _dbContext.People
            .FirstOrDefaultAsync(x => x.Id == currentPersonId, ct);

        if (person == null || !person.IsActive)
        {
            throw new Exception("Person not found or not active");
        }
        
        return await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(x => x.PersonId == person.Id)
            .OrderBy(x => x.Workspace.Name)
            .ThenBy(x => x.Workspace.Id)
            .Select(x => new Response.WorkspaceSummaryResponse
            {
                Id = x.Id,
                Code = x.Workspace.Code,
                Name = x.Workspace.Name,
                Description = x.Workspace.Description,
                MyRole = x.Role,
                MemberCount = x.Workspace.Members.Count,
                CreatedAt = x.Workspace.CreatedAt
            }).ToListAsync(ct);
        
    }

    public async Task<Response.WorkSpaceResponse> GetAsync(
        Guid workspaceId,
        Guid currentPersonId,
        CancellationToken ct = default)
    {
        var person = await _dbContext.People
            .FirstOrDefaultAsync(x => x.Id == currentPersonId, ct);

        if (person == null || !person.IsActive)
        {
            throw new Exception("Person not found or not active");
        }

        var result = await _dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(x =>
                x.WorkspaceId == workspaceId &&
                x.PersonId == currentPersonId &&
                !x.Workspace.IsDeleted)
            .Select(x => new Response.WorkSpaceResponse
            {
                Id = x.Workspace.Id,
                Code = x.Workspace.Code,
                Name = x.Workspace.Name,
                Description = x.Workspace.Description,
                CreatedByPersonId = x.Workspace.CreatedByPersonId,
                MyRole = x.Role,
                CreatedAt = x.Workspace.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        return result ?? throw new Exception("Workspace not found");
    }

    public async Task<Response.WorkSpaceResponse> UpdateAsync(Guid workspaceId, Request.UpdateWorkSpaceRequest request, Guid currentPersonId,
        CancellationToken ct = default)
    {
        var person = await _dbContext.People
            .FirstOrDefaultAsync(x => x.Id == currentPersonId, ct);

        if (person == null || !person.IsActive)
        {
            throw new Exception("Person not found or not active");
        }
        
        var membership = await _dbContext.WorkspaceMembers
                             .Include(x => x.Workspace)
                             .FirstOrDefaultAsync(x =>
                                     x.WorkspaceId == workspaceId &&
                                     x.PersonId == currentPersonId &&
                                     !x.Workspace.IsDeleted,
                                 ct)
                         ?? throw new Exception("Workspace not found");

        if (!membership.Role.Covers(WorkspaceRole.Editor))
        {
            throw new Exception("You cannot have permission to update workspace membership");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
           throw new Exception("Name is required");
        }

        membership.Workspace.Name = name;
        membership.Workspace.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await _dbContext.SaveChangesAsync(ct);

        return new Response.WorkSpaceResponse
        {
            Id = membership.Workspace.Id,
            Code = membership.Workspace.Code,
            Name = membership.Workspace.Name,
            Description = membership.Workspace.Description,
            CreatedByPersonId = membership.Workspace.CreatedByPersonId,
            MyRole = membership.Role,
            CreatedAt = membership.Workspace.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid workspaceId, Guid currentPersonId, CancellationToken ct = default)
    {
        var person = await _dbContext.People
            .FirstOrDefaultAsync(x => x.Id == currentPersonId, ct);

        if (person == null || !person.IsActive)
        {
            throw new Exception("Person not found or not active");
        }

        var membership = await _dbContext.WorkspaceMembers
                             .Include(x => x.Workspace)
                             .FirstOrDefaultAsync(x =>
                                     x.WorkspaceId == workspaceId &&
                                     x.PersonId == currentPersonId &&
                                     !x.Workspace.IsDeleted,
                                 ct)
                         ?? throw new Exception("Workspace not found");

        if (membership.Role != WorkspaceRole.Owner)
        {
            throw new Exception("You cannot have permission to delete workspace membership");
        }

        membership.Workspace.IsDeleted = true;
        
    }


    private static bool IsWorkspaceCodeConflict(DbUpdateException exception)
        => exception.InnerException is Npgsql.PostgresException
        {
            SqlState: Npgsql.PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ux_workspaces_code"
        };
    
   
    
}