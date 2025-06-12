using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
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

        // Send Email
        await smtp.SendAsync(email);

        // Disconnect SMTP Server
        await smtp.DisconnectAsync(true);

        return;
    }

}
