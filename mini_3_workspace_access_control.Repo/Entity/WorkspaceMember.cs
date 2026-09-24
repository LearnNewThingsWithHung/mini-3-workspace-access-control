using mini_3_workspace_access_control.Repo.Abstraction;
using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Repo.Entity;

public class WorkspaceMember : BaseEntity<Guid>, IAuditableEntity
{
    public Guid WorkspaceId { get; set; }

    public Guid PersonId { get; set; }

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Viewer;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public Person Person { get; set; } = null!;
}
