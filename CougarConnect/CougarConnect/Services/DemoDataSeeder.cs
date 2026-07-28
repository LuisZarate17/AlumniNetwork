using CougarConnect.Data;
using CougarConnect.Models;
using Microsoft.AspNetCore.Identity;

namespace CougarConnect.Services;

/// <summary>
/// Seeds a single, pre-confirmed demo account (plus a small, connected profile) so a
/// visitor to the live demo can log in and explore immediately, without registering or
/// clicking an email-confirmation link. Runs only when <c>DemoData:Seed</c> is true, and
/// is idempotent — safe to run on every startup.
/// </summary>
public static class DemoDataSeeder
{
    // The demo profile's Alumni id. Chosen to sit clear of the seeded rows (1-10) and the
    // random 10000-99999 ids real registrations use, so it can never collide.
    private const long DemoAlumniId = 100;

    public const string DefaultEmail = "demo@cougarconnect.demo";
    public const string DefaultPassword = "Demo123!";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        if (!config.GetValue<bool>("DemoData:Seed"))
        {
            return;
        }

        var email = config["DemoData:Email"] ?? DefaultEmail;
        var password = config["DemoData:Password"] ?? DefaultPassword;

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var supabase = scope.ServiceProvider.GetRequiredService<SupabaseService>();

        // 1. The Identity account, pre-confirmed so it can sign in without the email step.
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                logger.LogInformation("Seeded demo Identity account {Email}", email);
            }
            else
            {
                logger.LogError("Failed to seed demo account {Email}: {Errors}", email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        // 2. The matching Alumni profile plus a couple of connections and an inbound message,
        //    created once (only when the profile doesn't exist yet) so the demo dashboard
        //    isn't empty. Wrapped in try/catch so a Supabase hiccup can't stop the app booting.
        try
        {
            var existingProfile = await supabase.GetData<Alumni>(email, "Email");
            if (existingProfile.Length == 0)
            {
                await supabase.PostData("Alumni", new Alumni
                {
                    Id = DemoAlumniId,
                    First = "Demo",
                    Last = "User",
                    GradYear = "2021",
                    City = "Spokane",
                    Major = "Computer Science",
                    Company = "CougarConnect",
                    ConnectionList = Array.Empty<long>(),
                    Email = email,
                });

                // Connect the demo user to two seeded alumni, mutually so it shows both ways.
                await supabase.AddMutualConnection(DemoAlumniId, 1);
                await supabase.AddMutualConnection(DemoAlumniId, 2);

                // A sample inbound message so the messaging feature has something to show.
                await supabase.PostData("Messages", new Message
                {
                    Id = Random.Shared.NextInt64(1, long.MaxValue),
                    SenderId = 1,
                    RecipientId = DemoAlumniId,
                    Body = "Hi! Welcome to CougarConnect — great to connect with you here.",
                    SentAt = DateTimeOffset.UtcNow,
                    IsRead = false,
                });

                logger.LogInformation("Seeded demo Alumni profile and connections for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed demo Alumni profile for {Email}", email);
        }
    }
}
