namespace SplitWiseService.Services.Interface;

public interface IEmailService
{
    public Task Send(string toEmail, string subject, string body);
}
