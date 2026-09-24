namespace mini_3_workspace_access_control.Repo.Abstraction;

public abstract class BaseEntity<Tkey>
{
    public Tkey Id { get; set; }
    
    public bool IsDeleted  { get; set; }
}