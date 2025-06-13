namespace SplitWiseService.Constants;

public static class NotificationMessages
{
    // Register Success
    public const string RegisterSuccess = "Registration completed. Please verify you email.";

    // Login Success
    public const string LoginSuccess = "You have successfully logged in.";
    
    // Login Fail
    public const string LoginFailed = "Login failed";

    // CRUD Success
    public const string Created = "{0} added successfully!";
    public const string Saved = "{0} saved successfully!";
    public const string Updated = "{0} updated successfully!";
    public const string Deleted = "{0} deleted successfully!";

    // CRUD Fail
    public const string Exist = "{0} already exists";
    public const string SaveFailed = "Failed Adding {0}";
    public const string UpdateFailed = "Failed Updating {0}";
    public const string DeleteFailed = "Failed Deleting {0}";

    // Email
    public const string EmailSendingFailed = "Failed to send the email. Please try again.";
    public const string EmailSentSuccessfully = "Email sent successfully!";

    // Invalid
    public const string Invalid = "Invalid {0}";

    // Not Found
    public const string NotFound = "{0} not found";
    
    // Unauthorize
    public const string UnauthorizeUser = "You are unauthorize";
}
