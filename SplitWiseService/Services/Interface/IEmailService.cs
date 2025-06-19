namespace SplitWiseService.Services.Interface;

public interface IEmailService
{
    public Task Send(string toEmail, string subject, string body);
    public Task UserVarificationEmail(string firstName, string email);
    public Task ResetPasswordEmail(string firstName, string email, string token);
    public Task PasswordChangedEmail(string firstName, string email);
    public Task FriendRequestEmail(string recieverName, string senderName, string email);
    public Task ReferralEmail(string senderName, string email);
    public Task FriendRequestAcceptedEmail(string recieverName, string senderName, string email);
    public Task FriendRequestRejectedEmail(string recieverName, string senderName, string email);
    public Task FriendRemovedEmail(string recieverName, string senderName, string email);
}
