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
    public Task AddedToGroupEmail(string recieverName, string senderName, string groupName, string email);
    public Task RemovedFromGroupEmail(string recieverName, string senderName, string groupName, string email);
    public Task AddExpense(string recieverName, string senderName, string expenseName, string amount, string splitType, string shareAmount, string email, string oweVariable, string groupName = "");
    public Task UpdateExpense(string recieverName, string senderName, string expenseName, string amount, string splitType, string shareAmount, string email, string oweVariable, string groupName = "");
}
