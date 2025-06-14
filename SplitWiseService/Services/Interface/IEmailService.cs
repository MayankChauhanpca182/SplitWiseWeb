namespace SplitWiseService.Services.Interface;

public interface IEmailService
{
    public Task Send(string toEmail, string subject, string body);
    public Task UserVarificationEmail(string firstName, string email);
    public Task ResetPasswordEmail(string firstName, string email, string token);
}
