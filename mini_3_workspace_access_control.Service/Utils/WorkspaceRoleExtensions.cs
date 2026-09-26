using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Service.Utils;

public static class WorkspaceRoleExtensions
{
    public static bool Covers(
        this WorkspaceRole actual,
        WorkspaceRole required) =>
        (int)actual <= (int)required;
}