namespace CougarConnect.Components.Account;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;

    public EmailSender(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        var fromAddress = _emailOptions.FromAddress;
        var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress, _emailOptions.AppPassword)
        };

        return client.SendMailAsync(
            new MailMessage(from: fromAddress,
                            to: email,
                            subject,
                            message
                            ));
    }
}