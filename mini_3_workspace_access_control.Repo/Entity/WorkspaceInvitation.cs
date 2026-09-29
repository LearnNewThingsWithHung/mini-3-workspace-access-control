using mini_3_workspace_access_control.Repo.Abstraction;
using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Repo.Entity;

public class WorkspaceInvitation : BaseEntity<Guid>, IAuditableEntity
{
    public Guid WorkspaceId { get; set; }

    public string Email { get; set; } = null!;
    
    public InvitationStatus Status { get; set; }
    
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Viewer;

    public string TokenHash { get; set; } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public Guid CreatedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public Person CreatedByPerson { get; set; } = null!;
}
