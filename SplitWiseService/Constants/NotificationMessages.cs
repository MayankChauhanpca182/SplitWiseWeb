namespace SplitWiseService.Constants;

public static class NotificationMessages
{
    // Register success
    public const string RegisterSuccess = "Welcome aboard! Just one more step—please verify your email to activate your account.";

    // Login
    public const string LoginSuccess = "You have successfully logged in.";
    public const string LoginFailed = "We couldn’t log you in. Please make sure your credentials are correct.";

    // CRUD success
    public const string Created = "{0} has been added successfully.";
    public const string Saved = "{0} has been saved successfully.";
    public const string Updated = "{0} has been updated successfully.";
    public const string Deleted = "{0} has been deleted successfully.";

    // CRUD fail
    public const string Exist = "{0} already exists.";
    public const string SaveFailed = "Failed to add {0}." + TryAgain;
    public const string UpdateFailed = "Failed to update {0}." + TryAgain;
    public const string DeleteFailed = "Failed to delete {0}." + TryAgain;

    // Try again
    public const string TryAgain = " Please try again later.";

    // Email Notifications
    public const string InvalidEmailAddress = "Email address you entered is not valid. Enter a valid email address.";
    public const string EmailSendingFailed = "Failed to send the email." + TryAgain;
    public const string EmailSentSuccessfully = "Email sent successfully.";

    // Reset password email
    public const string ResetPasswordEmailSuccess = "Reset Password Link has been sent successfully.";
    public const string WrongResetPasswordLink = "It seems like Reset Password link has been either tampered or wrong.";
    public const string ConsumedResetPasswordLink = "Reset Password link has been consumed already.";
    public const string ExpiredResetPasswordLink = "Reset Password link has been expired. Kindly, reset password again.";

    // Validation
    public const string Invalid = "Invalid {0}.";

    // Not found
    public const string NotFound = "{0} not found.";

    // Unauthorize
    public const string UnauthorizeUser = "You are not authorized to perform this action.";

    // User not found
    public const string UserNotFound = "Oops! We couldn’t find a user with that information.";
    public const string UserNotFoundByEmail = "Oops! We couldn’t find a user with entered Email.";

    // Email verification
    public const string EmailNotVerified = "Oops! Your email isn’t verified yet. We’ve sent you a link to verify it.";
    public const string EmailAlreadyVerified = "Verification link has been expired.";
    public const string EmailVerificationSuccess = "Email has been verified successfully.";
    public const string EmailExists = "Looks like you've already signed up with {0}. Try logging in instead.";

    // System errors
    public const string InternalServerError = "Something went wrong." + TryAgain;
    public const string UnhandledException = "An unexpected error occurred.";

    // Link expired
    public const string LinkExpired = "The link has been expired. Please request a new one.";

    // Password reset success
    public const string IncorrectPassword = "The password you entered is incorrect.";
    public const string PasswordChangeSuccess = "Your password has been changed successfully.";
    public const string PasswordResetSuccess = "Your password has been reset successfully.";

    // Email subjects
    public const string UserVerificationSubject = "Almost There! Verify Your Email to Activate Your Account";
    public const string PasswordResetSubject = "Trouble Logging In? Reset Your Password";
    public const string PasswordChangedNotification = "Your password has been changed successfully.";

    // Profile update
    public const string ProfileUpdateSuccess = "Your profile has been updated successfully.";
}
