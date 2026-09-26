using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.WorkSpaceMember;

public class Request
{
    public class UpdateWorkSpaceMemberRoleRequest
    {
        public WorkspaceRole Role { get; set; }
    }

    public class TransferWorkSpaceOwnershipRequest
    {
        public Guid NewOwnerPersonId { get; set; }
        public WorkspaceRole PreviousOwnerRole { get; set; }
    }
}
