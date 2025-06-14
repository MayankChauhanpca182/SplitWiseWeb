using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly AesHelper _aesHelper;
    private readonly UrlBuilder _urlBuilder;

    public EmailService(IConfiguration configuration, AesHelper aesHelper, UrlBuilder urlBuilder)
    {
        _configuration = configuration;
        _aesHelper = aesHelper;
        _urlBuilder = urlBuilder;
    }

    public async Task Send(string toEmail, string subject, string body)
    {
        IConfigurationSection? emailConfigurations = _configuration.GetSection("EmailConfigurations");

        // Create email
        MimeMessage email = new MimeMessage();
        email.From.Add(new MailboxAddress(emailConfigurations["SenderName"], emailConfigurations["SenderEmail"]));
        email.To.Add(new MailboxAddress(toEmail, toEmail));
        email.Subject = subject;
        BodyBuilder bodyBuilder = new() { HtmlBody = body };
        email.Body = bodyBuilder.ToMessageBody();

        // Configure SMTP Server
        using SmtpClient smtp = new SmtpClient();
        await smtp.ConnectAsync(emailConfigurations["SmtpServer"], int.Parse(emailConfigurations["Port"]), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(emailConfigurations["SenderEmail"], emailConfigurations["Password"]);

        // Send Email
        await smtp.SendAsync(email);

        // Disconnect SMTP Server
        await smtp.DisconnectAsync(true);

        return;
    }

    public async Task UserVarificationEmail(string firstName, string email)
    {
        string token = _aesHelper.Encrypt(email);
        string fileText = GetEmailTemplate("UserVarification.html");
        string verificationLink = await _urlBuilder.Create("UserVerification", "Auth", token);

        string emailBody = fileText.Replace("{name}", firstName).Replace("{link}", verificationLink);
        await Send(email, NotificationMessages.UserVerificationSubject, emailBody);
        return;
    }

    public async Task ResetPasswordEmail(string firstName, string email, string token)
    {
        string fileText = GetEmailTemplate("ResetPassword.html");
        string resetLink = await _urlBuilder.Create("ResetPassword", "Auth", token);

        string emailBody = fileText.Replace("{name}", firstName).Replace("{link}", resetLink);
        await Send(email, NotificationMessages.PasswordResetSubject, emailBody);
        return;
    }

    private string GetEmailTemplate(string fileName)
    {
        string layoutPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailTemplates", "EmailLayout.html");
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailTemplates", fileName);
        return File.ReadAllText(layoutPath).Replace("{content}", File.ReadAllText(filePath));
    }

}
