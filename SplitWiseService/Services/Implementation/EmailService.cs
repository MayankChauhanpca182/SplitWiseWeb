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

    private string GetEmailTemplate(string fileName)
    {
        string layoutPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailTemplates", EmailTemplates.EmailLayout);
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailTemplates", fileName);
        return File.ReadAllText(layoutPath).Replace("{content}", File.ReadAllText(filePath));
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
    }

    public async Task UserVarificationEmail(string firstName, string email)
    {
        string token = _aesHelper.Encrypt(email);
        string fileText = GetEmailTemplate(EmailTemplates.UserVerification);
        string verificationLink = _urlBuilder.Create("UserVerification", "Auth", token);

        string emailBody = fileText.Replace("{recieverName}", firstName).Replace("{link}", verificationLink);
        await Send(email, EmailSubjects.UserVerificationSubject, emailBody);
        return;
    }

    public async Task ResetPasswordEmail(string firstName, string email, string token)
    {
        string fileText = GetEmailTemplate(EmailTemplates.ResetPassword);
        string resetLink = _urlBuilder.Create("ResetPassword", "User", token);

        string emailBody = fileText.Replace("{recieverName}", firstName).Replace("{link}", resetLink);
        await Send(email, EmailSubjects.PasswordResetSubject, emailBody);
        return;
    }

    public async Task PasswordChangedEmail(string firstName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.PasswordChangedNotification);

        string emailBody = fileText.Replace("{recieverName}", firstName);
        await Send(email, EmailSubjects.PasswordChangedNotification, emailBody);
        return;
    }

    public async Task FriendRequestEmail(string recieverName, string senderName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.FriendRequest);
        string loginLink = _urlBuilder.Create("Login", "Auth");

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName).Replace("{link}", loginLink);
        await Send(email, EmailSubjects.NewFriendRequest, emailBody);
        return;
    }

    public async Task ReferralEmail(string senderName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.Referral);
        string loginLink = _urlBuilder.Create("Register", "User");

        string emailBody = fileText.Replace(" {recieverName}", "Sir or Madam").Replace("{senderName}", senderName).Replace("{link}", loginLink);
        await Send(email, EmailSubjects.ReferralRequest, emailBody);
        return;
    }

    public async Task FriendRequestAcceptedEmail(string recieverName, string senderName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.FriendRequestAccepted);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName);
        await Send(email, EmailSubjects.FriendRequestAccepted, emailBody);
        return;
    }

    public async Task FriendRequestRejectedEmail(string recieverName, string senderName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.FriendRequestRejected);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName);
        await Send(email, EmailSubjects.FriendRequestRejected, emailBody);
        return;
    }

    public async Task FriendRemovedEmail(string recieverName, string senderName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.FriendRemoved);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName);
        await Send(email, EmailSubjects.FriendRemoved, emailBody);
        return;
    }

    public async Task AddedToGroupEmail(string recieverName, string senderName, string groupName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.AddedToGroup);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName).Replace("{groupName}", groupName);
        await Send(email, EmailSubjects.AddedToGroup, emailBody);
        return;
    }

    public async Task RemovedFromGroupEmail(string recieverName, string senderName, string groupName, string email)
    {
        string fileText = GetEmailTemplate(EmailTemplates.RemovedFromGroup);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName).Replace("{groupName}", groupName);
        await Send(email, EmailSubjects.RemovedFromGroup, emailBody);
        return;
    }

    public async Task AddIndividualExpense(string recieverName, string senderName, string expenseName, string amount, string splitType, string shareAmount, string email, string oweVariable)
    {
        string fileText = GetEmailTemplate(EmailTemplates.AddIndividualExpense);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName).Replace("{splittype}", splitType).Replace("{amount}", amount).Replace("{shareamount}", shareAmount).Replace("{owe}", oweVariable).Replace("{expenseName}", expenseName);
        await Send(email, EmailSubjects.AddIndividualExpense, emailBody);
        return;
    }

    public async Task UpdateIndividualExpense(string recieverName, string senderName, string expenseName, string amount, string splitType, string shareAmount, string email, string oweVariable)
    {
        string fileText = GetEmailTemplate(EmailTemplates.UpdateIndividualExpense);

        string emailBody = fileText.Replace("{recieverName}", recieverName).Replace("{senderName}", senderName).Replace("{splittype}", splitType).Replace("{amount}", amount).Replace("{shareamount}", shareAmount).Replace("{owe}", oweVariable).Replace("{expenseName}", expenseName);
        await Send(email, EmailSubjects.UpdateIndividualExpense, emailBody);
        return;
    }
}
