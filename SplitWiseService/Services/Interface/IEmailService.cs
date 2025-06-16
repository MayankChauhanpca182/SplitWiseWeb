namespace SplitWiseService.Services.Interface;

public interface IEmailService
{
    public Task Send(string toEmail, string subject, string body);
    public void UserVarificationEmail(string firstName, string email);
    public void ResetPasswordEmail(string firstName, string email, string token);
    public void ChangePasswordEmail(string firstName, string email);
}
