using Microsoft.AspNetCore.Identity;

namespace CougarConnect.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's preferred UI theme ("dark" or "light"). Null means no explicit choice yet,
        /// so the device/OS default is used. Stored per account so different users on the same
        /// browser can each have their own theme.
        /// </summary>
        public string? Theme { get; set; }
    }

}
