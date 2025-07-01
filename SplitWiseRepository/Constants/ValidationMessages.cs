namespace SplitWiseRepository.Constants;

public static class ValidationMessages
{
    // Required

    // Email
    public const string EmailRequired = "Email is required.";
    public const string ValidEmail = "Please anter a valid Email Address.";

    // Password
    public const string PasswordRequired = "Password is required.";
    public const string PasswordLength = "Password must be at least 8 characters long.";
    public const string ValidPassword = "Password must contain at least one letter, one number, and one special character.";

    // Current password
    public const string CurrentPasswordRequired = "Current Password is required.";

    // New password
    public const string NewPasswordRequired = "New Password is required.";
    public const string NewPasswordLength = "New Password must be at least 8 characters long.";
    public const string ValidNewPassword = "New Password must contain at least one letter, one number, and one special character.";

    // Confirm password
    public const string ConfirmPasswordRequired = "Confirm Password is required.";
    public const string ConfirmPasswordLength = "Confirm Password must be at least 8 characters long.";
    public const string ValidConfirmPassword = "Confirm Password must contain at least one letter, one number, and one special character.";
    public const string MatchPassword = "Password and Confirmation Password must match.";
    public const string MatchNewPassword = "New Password and confirmation Password must match.";

    // First name
    public const string FirstNameRequired = "First Name is required.";
    public const string FirstNameLength = "First Name can not exceed 50 characters.";
    public const string ValidFirstName = "First Name must contain only letters.";

    // Last name
    public const string LastNameRequired = "Last Name is required.";
    public const string LastNameLength = "Last Name can not exceed 50 characters.";
    public const string ValidLastName = "Last Name must contain only letters.";

    // Currency
    public const string CurrencyRequired = "Currency is required.";

    // Birthdate
    public const string BirthdateRequired = "Date of Birth is required.";
    public const string NoFutureBirthdate = "The date of birth must not be set in the future.";


    // Group
    public const string GroupName = "Group Name is required.";
    public const string GroupNameLength = "Group Name can not exceed 50 characters.";
    public const string ValidGroupName = "Group Name can not contain spaces.";


    // Expense
    public const string ExpenseTitleRequired = "Expense title is required.";
    public const string ExpenseTitleLength = "Expense title can not exceed 100 characters.";
    public const string ExpenseAmountRequired = "Expense amount is required.";
    public const string ValidExpenseAmount = "Expense amount must be less then 9999999.";
    public const string PaymentDateRequired = "Payment Date is required.";
    public const string NoFuturePaymentDate = "The payment date must not be set in the future.";
    public const string SplitTypeRequired = "Select expense split type.";

}
