using System.Reflection;

namespace mini_3_workspace_access_control.Service;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}