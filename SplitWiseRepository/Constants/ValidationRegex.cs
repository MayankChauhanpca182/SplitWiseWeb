namespace SplitWiseRepository.Constants;

public static class ValidationRegex
{
    // Email
    public const string EmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    // Password
    public const string PasswordRegex = "^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$";

    // Name
    public const string NameRegex = @"^[A-Za-z]+$";

    // Group Name
    public const string GroupNameRegex = @"^[A-Za-z0-9]+$";
}
