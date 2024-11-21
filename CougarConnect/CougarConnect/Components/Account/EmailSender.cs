namespace CougarConnect.Components.Account;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("wsucougarconnect@gmail.com", "wmqe mbgb mfvc gqpq")
        };

        return client.SendMailAsync(
            new MailMessage(from: "wsucougarconnect@gmail.com",
                            to: email,
                            subject,
                            message
                            ));
    }
}