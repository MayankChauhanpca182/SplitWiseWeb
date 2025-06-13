namespace SplitWiseService.Constants;

public static class NotificationMessages
{
    // Register success
    public const string RegisterSuccess = "Welcome aboard! Just one more step—please verify your email to activate your account.";

    // Login success
    public const string LoginSuccess = "You have successfully logged in.";

    // Login fail
    public const string LoginFailed = "We couldn’t log you in. Please make sure your credentials are correct.";

    // CRUD success
    public const string Created = "{0} added successfully!";
    public const string Saved = "{0} saved successfully!";
    public const string Updated = "{0} updated successfully!";
    public const string Deleted = "{0} deleted successfully!";

    // CRUD fail
    public const string Exist = "{0} already exists";
    public const string SaveFailed = "Failed Adding {0}";
    public const string UpdateFailed = "Failed Updating {0}";
    public const string DeleteFailed = "Failed Deleting {0}";

    // Email sending
    public const string EmailSendingFailed = "Failed to send the email. Please try again.";
    public const string EmailSentSuccessfully = "Email sent successfully!";

    // Invalid
    public const string Invalid = "Invalid {0}";

    // Not found
    public const string NotFound = "{0} not found";

    // Unauthorize
    public const string UnauthorizeUser = "You are unauthorize";

    // User not found
    public const string UserNotFound = "Oops! We couldn’t find a user with that information.";

    // Email verification
    public const string EmailNotVerified = "Oops! Your email isn’t verified yet. We’ve sent you a link to verify it.";
    public const string EmailNotConfirmed = "Email is already verified.";
    public const string EmailVerificationSuccess = "Email has been verified successfully.";
    public const string EmailExists = "Looks like you've already signed up with {0}. Try logging in instead.";

    // Internal server error
    public const string InternalServerError = "Internal server error. Please try again later.";

    // Unhandled exception
    public const string UnhandledException = "An unhandled exception occurred.";
}
