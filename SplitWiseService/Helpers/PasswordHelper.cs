using BCrypt.Net;

namespace SplitWiseService.Helpers;

public class PasswordHelper
{
    public static string Hash(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public static bool Verify(string plainPassword, string hashPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashPassword);
    }
}
