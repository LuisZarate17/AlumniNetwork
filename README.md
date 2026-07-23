Alumni Network Project

Project Description:
The Alumni Network is a web-based platform designed to help WSU alumni connect with one another and stay in touch with their alma mater. Users can search for alumni profiles
using filters like name, graduation year, city, and major. The platform enables users to view connections, manage requests, and access contact details for accepted connections.

Features:
Search Alumni: Search alumni profiles using a case-insensitive search bar with autocomplete and filtering options.
Manage Connections: View all connections, including accepted and declined.
View Contact Information: Access detailed contact information for accepted connections.
Interactive Interface: A user-friendly platform with streamlined functionality for alumni networking.

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

## Testing

No automated test project exists yet. Adding one (e.g. an xUnit project covering `SupabaseService` and the Blazor components) is tracked as future work.
