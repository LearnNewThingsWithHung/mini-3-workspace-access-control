using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.Invitation;

public class Request
{
    public class CreateInvitationRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid CurrentPersonId { get; set; }
        public string Email { get; set; } = null!;
        public WorkspaceRole Role { get; set; }
    }
    
    public class AcceptInvitationRequest
    {
        public string Token { get; set; } = null!;
    }

    public class CreateWorkspaceInvitationBody
    {
        public string Email { get; set; } = null!;
        public WorkspaceRole Role { get; set; }
    }
}
