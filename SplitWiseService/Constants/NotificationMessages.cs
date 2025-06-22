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
    public const string SaveFailed = "Failed to save {0}." + TryAgain;
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
    public const string NotFound = "We could not find {0}";

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
    public const string IncorrectPassword = "The current password you entered is incorrect.";
    public const string PasswordChangeSuccess = "Your password has been changed successfully.";
    public const string PasswordResetSuccess = "Your password has been reset successfully.";

    // Ajax errors
    public const string CanNot = "Can not {0} at this time." + TryAgain;

    // Friend
    public const string NoAccountFound = "No account was found with {0}. Would you like to refere.";
    public const string FriendRequestToSelf = "You cannot send a friend request to yourself.";
    public const string YouAlreadyRequested = "You have already sent friend request to {0}.";
    public const string FriendRequestExist = "{0} has already sent a friend request to you with email ({1}).";
    public const string AlreadyFriend = "You are already friend of {0}.";
    public const string FriendRequestSuccess = "Friend request has been sent successfully.";
    public const string ReferralSuccess = "Referral email has been sent successfully.";
    public const string AlreadyReferred = "You have already reaferred {0}.";
    public const string AlreadyReferredBySomeone = "{0} has been already referred by someone, once {0} will create account he/she will get your friend.";
    public const string FriendRequestAccepted = "Friend request from {0} has been accepted successfully.";
    public const string FriendRequestRejected = "Friend request from {0} has been rejected successfully.";
    public const string FriendRemoved = "{0} has been removed from your friend list successfully.";

    // Export
    public const string CanNotExportEmptyList = "The {0} list is empty, so there is nothing to export.";

    // Group
    public const string MemberAddedToGroup = "{0} has been successfully added to {1}.";
    public const string MemberRemovedFromGroup = "{0} has been successfully removed from {1}.";
}
