using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.WorkSpaceMember;

public class Response
{
    public class WorkSpaceMemberResponse
    {
        public Guid PersonId { get; set; }
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public WorkspaceRole Role { get; set; }
        public DateTimeOffset JoinedAt { get; set; }
    }
}
