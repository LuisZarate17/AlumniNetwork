Alumni Network Project

Project Description:
The Alumni Network is a web-based platform designed to help WSU alumni connect with one another and stay in touch with their alma mater. Users can search for alumni profiles
using filters like name, graduation year, city, and major. The platform enables users to view connections, manage requests, and access contact details for accepted connections.

Features:
Search Alumni: Search alumni profiles using a case-insensitive search bar with autocomplete and filtering options.
Manage Connections: View all connections, including accepted and declined.
View Contact Information: Access detailed contact information for accepted connections.
Interactive Interface: A user-friendly platform with streamlined functionality for alumni networking.

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio) or a reachable SQL Server instance, for ASP.NET Core Identity's account database
- Optional: a [Supabase](https://supabase.com) project if you want the alumni search/connections features to work end-to-end — login and registration work without one, but need an `Alumni` table matching `CougarConnect/CougarConnect/Models/Alumni.cs`

### Setup
1. Clone the repo.
2. Provide configuration — either copy `CougarConnect/CougarConnect/appsettings.Example.json` to `CougarConnect/CougarConnect/appsettings.json` and fill in real values, or use `dotnet user-secrets` (recommended, keeps real values out of any tracked file):
   ```
   cd CougarConnect/CougarConnect
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=CougarConnect;Trusted_Connection=True;MultipleActiveResultSets=true"
   dotnet user-secrets set "Supabase:Url" "https://your-project.supabase.co"
   dotnet user-secrets set "Supabase:ApiKey" "your-supabase-api-key"
   dotnet user-secrets set "Email:SmtpHost" "smtp.gmail.com"
   dotnet user-secrets set "Email:SmtpPort" "587"
   dotnet user-secrets set "Email:FromAddress" "your-app-email@gmail.com"
   dotnet user-secrets set "Email:AppPassword" "your-gmail-app-password"
   ```
   See the Configuration table below for what each key means.
3. Trust the local HTTPS development certificate (the app enforces HTTPS redirection):
   ```
   dotnet dev-certs https --trust
   ```
4. Apply the Identity database migrations (creates the login/account tables):
   ```
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project CougarConnect/CougarConnect
   ```
   Alternatively, in `Development` the app exposes a migrations-apply page in the browser the first time you hit an unmigrated database — but running `dotnet ef database update` up front is more reliable for a fresh clone.
5. Run the app:
   ```
   dotnet run --project CougarConnect/CougarConnect
   ```

## Configuration

The app reads its configuration from `CougarConnect/CougarConnect/appsettings.json`, which is gitignored since it holds real secrets. Copy `appsettings.Example.json` in the same folder to `appsettings.json` (or use `dotnet user-secrets` / `appsettings.Development.json`) and fill in real values — never commit real secrets.

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server / LocalDB connection string used by ASP.NET Core Identity for account storage (login, registration, 2FA). |
| `Email:SmtpHost` / `Email:SmtpPort` | SMTP server used to send account-confirmation and connection-request emails (defaults: `smtp.gmail.com` / `587`). |
| `Email:FromAddress` | The email address emails are sent from. |
| `Email:AppPassword` | Must be a Gmail **App Password**, not your real account password — generate one at https://myaccount.google.com/apppasswords (requires 2-Step Verification enabled). |
| `Supabase:Url` | The REST endpoint for your Supabase project (e.g. `https://your-project.supabase.co`), found under Project Settings > API. |
| `Supabase:ApiKey` | The Supabase API key used to authenticate REST calls to the `Alumni` table, also found under Project Settings > API. |

## Running Tests

```
dotnet test CougarConnect/CougarConnect.sln
```

Covers unit tests for `SupabaseService` (Supabase query/URL construction, connection de-duplication logic, a regression test for a past request-header leak) and `Alumni`'s JSON wire-format mapping, plus an integration test that exercises the real login flow end-to-end (via `WebApplicationFactory` with EF Core's in-memory provider, so no SQL Server/LocalDB is required to run the suite itself).
