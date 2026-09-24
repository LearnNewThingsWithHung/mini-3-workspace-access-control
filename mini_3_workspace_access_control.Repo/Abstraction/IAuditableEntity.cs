namespace mini_3_workspace_access_control.Repo.Abstraction;

public interface IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; } 
    public DateTimeOffset? UpdatedAt { get; set; } 
}