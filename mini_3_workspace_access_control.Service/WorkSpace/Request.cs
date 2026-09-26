namespace mini_3_workspace_access_control.Service.WorkSpace;

public class Request
{
    public class CreateWorkSpaceRequest
    {
        public required String Name { get; set; }
        public String? Description { get; set; }
    }
    
    public class UpdateWorkSpaceRequest
    {
        public required String Name { get; set; }
        public string? Description { get; set; }
    }
}