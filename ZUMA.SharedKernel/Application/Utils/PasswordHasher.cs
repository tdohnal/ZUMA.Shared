using Microsoft.AspNetCore.Identity;

namespace ZUMA.SharedKernel.Application.Utils;

public static class PasswordHasher
{
    private static readonly PasswordHasher<object> _hasher = new();

    public static string Hash(string password)
    {
        return _hasher.HashPassword(null!, password);
    }

    public static bool Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(null!, hash, password);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
