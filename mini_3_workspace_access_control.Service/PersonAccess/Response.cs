namespace mini_3_workspace_access_control.Service.PersonAccess;

public class Response
{
    public class AccessPersonResponse
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public bool IsActive { get; set; }
    }
}