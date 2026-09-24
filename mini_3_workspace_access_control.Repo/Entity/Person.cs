using mini_3_workspace_access_control.Repo.Abstraction;

namespace mini_3_workspace_access_control.Repo.Entity;

public class Person : BaseEntity<Guid>, IAuditableEntity
{
    public string Email { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();

    public ICollection<Workspace> CreatedWorkspaces { get; set; } = new List<Workspace>();

    public ICollection<WorkspaceInvitation> CreatedWorkspaceInvitations { get; set; } = new List<WorkspaceInvitation>();
}
