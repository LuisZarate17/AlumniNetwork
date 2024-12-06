using CougarConnect.Components.Account.Pages.Manage;
using CougarConnect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace CougarConnect.Components.Account
{
    internal sealed class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly EmailSender emailSender = new();

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirm your email", $"Welcome to Cougar Connect! \nPlease confirm your account email by following the link below \n\n{confirmationLink}");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by following the link below \n\n{resetLink}");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
