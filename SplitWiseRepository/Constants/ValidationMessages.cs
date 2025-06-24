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

    // Current password
    public const string CurrentPasswordRequired = "Current password is required";

    // New password
    public const string NewPasswordRequired = "New password is required";
    public const string NewPasswordLength = "New password must be at least 8 characters long";
    public const string ValidNewPassword = "New password must contain at least one letter, one number, and one special character";

    // Confirm password
    public const string ConfirmPasswordRequired = "Confirm password is required";
    public const string ConfirmPasswordLength = "Confirm password must be at least 8 characters long";
    public const string ValidConfirmPassword = "Confirm password must contain at least one letter, one number, and one special character";
    public const string MatchPassword = "New password and confirmation Password must match.";

    // First name
    public const string FirstNameRequired = "First name is required";
    public const string FirstNameLength = "First name can not exceed 50 characters";
    public const string ValidFirstName = "First name must contain only letters";

    // Last name
    public const string LastNameRequired = "Last name is required";
    public const string LastNameLength = "Last name can not exceed 50 characters";
    public const string ValidLastName = "Last name must contain only letters";

    // Currency
    public const string CurrencyRequired = "Currency is required";

    // Birthdate
    public const string BirthdateRequired = "Birthdate is required";
    public const string NoFutureBirthdate = "The birthdate must not be set in the future";


    // Group
    public const string GroupName = "Group name is required";
    public const string GroupNameLength = "Group name can not exceed 50 characters";


    // Expense
    public const string ExpenseTitleRequired = "Expense title is required";
    public const string ExpenseTitleLength = "Expense tile can not exceed 100 characters";
    public const string ExpenseAmountRequired = "Expense amount is required.";
    public const string ValidExpenseAmount = "Please enter a valid expense amount greater than zero and less then 10000000.";
    public const string PaymentDateRequired = "Payment date is required";
    public const string NoFuturePaymentDate = "The payment date must not be set in the future.";
    public const string SplitTypeRequired = "Select expense split type.";

}
