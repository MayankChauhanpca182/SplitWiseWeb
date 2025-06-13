namespace SplitWiseRepository.Constants;

public static class ValidationMessages
{
    // Required

    // Email
    public const string EmailRequired = "Email is required";
    public const string ValidEmail = "Enter a valid email address";

    // Password
    public const string PasswordRequired = "Password is required";
    public const string PasswordLength = "Password must be at least 8 characters long";
    public const string ValidPassword = "Password must contain at least one letter, one number, and one special character";

    // Confirm password
    public const string ConfirmPasswordRequired = "Confirm password is required";
    public const string ConfirmPasswordLength = "Confirm password must be at least 8 characters long";
    public const string ValidConfirmPassword = "Confirm password must contain at least one letter, one number, and one special character";
    public const string MatchPassword = "Password and Confirmation Password must match.";

    // First name
    public const string FirstNameRequired = "First name is required";
    public const string FirstNameLength = "First name can not exceed 50 characters";
    public const string ValidFirstName = "First name must contain only letters";

    // Last name
    public const string LastNameRequired = "Last name is required";
    public const string LastNameLength = "Last name can not exceed 50 characters";
    public const string ValidLastName = "Last name must contain only letters";
}
