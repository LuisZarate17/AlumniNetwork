using CougarConnect.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CougarConnect.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "CougarConnectTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=DoesNotMatter;Trusted_Connection=True;",
                ["Supabase:Url"] = "https://example.test",
                ["Supabase:ApiKey"] = "test-key",
                ["Email:SmtpHost"] = "smtp.example.test",
                ["Email:SmtpPort"] = "587",
                ["Email:FromAddress"] = "test@example.test",
                ["Email:AppPassword"] = "test-password",
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(TestDatabaseName));
        });
    }

    public async Task SeedTestUserAsync(string email, string password)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(user, password);
    }
}
