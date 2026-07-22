namespace CougarConnect.Components.Account;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        var fromAddress = _configuration["Email:FromAddress"];
        var client = new SmtpClient(_configuration["Email:SmtpHost"], _configuration.GetValue<int>("Email:SmtpPort"))
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress, _configuration["Email:AppPassword"])
        };

        return client.SendMailAsync(
            new MailMessage(from: fromAddress,
                            to: email,
                            subject,
                            message
                            ));
    }
}