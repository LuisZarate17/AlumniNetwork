namespace CougarConnect.Components.Account;

public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;
}
