using System.Security.Cryptography;

namespace mini_3_workspace_access_control.Service.Utils;

public static class WorkspaceCodeGenerator
{
    private const string Alphabet = 
        "abcdefghjkmnpqrstuvwxyz23456789";

    public static string Generate(int length = 8)
    {
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(Alphabet.Length);
            chars[i] = Alphabet[index];
        }

        return $"ws-{new string(chars)}";
    }
}