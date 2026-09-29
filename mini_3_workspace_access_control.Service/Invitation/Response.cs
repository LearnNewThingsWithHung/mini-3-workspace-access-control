namespace mini_3_workspace_access_control.Service.Invitation;

public class Response
{
    public class CreateInvitationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    public class InvitationListItemResponse
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Status { get; set; } = null!;
        public Guid CreatedByPersonId { get; set; }
        public string CreatedByDisplayName { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? AcceptedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}