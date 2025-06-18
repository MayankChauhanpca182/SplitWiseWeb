namespace SplitWiseService.Constants;

public static class EmailTemplates
{
    public const string EmailLayout = "EmailLayout.html";
    public const string UserVerification = "UserVarification.html";
    public const string ResetPassword = "ResetPassword.html";
    public const string PasswordChangedNotification = "PasswordChangedNotification.html";
    public const string FriendRequest = "FriendRequest.html";
    public const string Referral = "Referral.html";
    public const string FriendRequestAccepted = "FriendRequestAccepted.html";
    public const string FriendRequestRejected = "FriendRequestRejected.html";
}

public static class EmailSubjects
{
    // Email subjects
    public const string UserVerificationSubject = "Almost There! Verify Your Email to Activate Your Account";
    public const string PasswordResetSubject = "Trouble Logging In? Reset Your Password";
    public const string PasswordChangedNotification = "Your password has been changed successfully.";
    public const string NewFriendRequest = "You’ve Received a New Friend Request.";
    public const string ReferralRequest = "You have been invited to SplitMate.";
    public const string FriendRequestAccepted = "Your Friend Request Has Been Accepted.";
    public const string FriendRequestRejected = "Your Friend Request Was Declined.";
}

