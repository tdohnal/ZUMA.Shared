using System.Security.Cryptography;
using System.Text;

namespace ZUMA.SharedKernel.Application.Utils;

public static class PasswordGenerator
{
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Specials = "!@#$%&*?";

    public static string Generate(
        int length = 16,
        bool includeSpecials = false)
    {
        if (length < 10)
            throw new ArgumentException("Password length must be at least 10.");

        var charset = new StringBuilder();
        charset.Append(Lower);
        charset.Append(Upper);
        charset.Append(Digits);

        if (includeSpecials)
            charset.Append(Specials);

        var chars = charset.ToString();
        var result = new char[length];

        using var rng = RandomNumberGenerator.Create();

        for (int i = 0; i < length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(chars.Length);
            result[i] = chars[index];
        }

        return new string(result);
    }
}
