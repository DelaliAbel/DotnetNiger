using System.Security.Cryptography;

namespace DotnetNiger.Api.Application.Services.Users;

/// <summary>Générateur de codes aléatoires pour les confirmations.</summary>
internal static class CodeGenerator
{
    private static readonly char[] CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    /// <summary>Génère un code aléatoire de 6 caractères.</summary>
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = CodeChars[bytes[i] % CodeChars.Length];
        return new string(code);
    }
}
