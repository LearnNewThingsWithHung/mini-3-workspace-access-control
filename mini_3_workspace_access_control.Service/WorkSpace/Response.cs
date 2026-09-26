using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.WorkSpace;

public class Response
{
    public class WorkSpaceResponse
    {
        public Guid Id { get; set; }
        public Guid CreatedByPersonId { get; set; }
        public WorkspaceRole MyRole { get; set; } = WorkspaceRole.Owner;
        public required String Code { get; set; }
        public required String Name { get; set; }
        public String? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    public class WorkspaceSummaryResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public WorkspaceRole MyRole { get; set; }
        public int MemberCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}