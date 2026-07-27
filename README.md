# CougarConnect

> Alumni networking platform for WSU alumni — search graduates, send connection requests, and message the people you connect with.

[![CI](https://github.com/LuisZarate17/AlumniNetwork/actions/workflows/ci.yml/badge.svg)](https://github.com/LuisZarate17/AlumniNetwork/actions/workflows/ci.yml)
![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=flat&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor%20Server-512BD4?style=flat&logo=blazor&logoColor=white)
![ASP.NET Core Identity](https://img.shields.io/badge/ASP.NET%20Core%20Identity-512BD4?style=flat&logo=dotnet&logoColor=white)
![Supabase](https://img.shields.io/badge/Supabase-3FCF8E?style=flat&logo=supabase&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat&logo=microsoftsqlserver&logoColor=white)
![xUnit](https://img.shields.io/badge/Tested%20with-xUnit-5A2D91?style=flat)

CougarConnect is a web platform that helps WSU alumni find each other and stay in touch with their alma mater. A single search box matches graduates across name, graduation year, city, major, and company; users send connection requests, exchange messages once connected, and get recommendations for people they may know.

## Screenshots

**Alumni search** — one box matches across name, major, city, company, and grad year, with live connection status on every result:

![Alumni search](docs/screenshots/search.png)

| Connections & recommendations | Direct messaging |
|:---:|:---:|
| ![Connections](docs/screenshots/connections.png) | ![Messaging](docs/screenshots/messaging.png) |
| **Recent activity** | **Dashboard** |
| ![Recent activity](docs/screenshots/recent-activity.png) | ![Dashboard](docs/screenshots/home.png) |

> The screenshots above run against local mock data — every profile is clearly labeled as such, not a real WSU alumnus.

## Features

- **Alumni search** — one search box matches across name, grad year, city, major, and company (case-insensitive partial match), so you don't have to pick a field first.
- **Connection requests** — send a request with a subject and message; the recipient accepts or declines in-app on the Recent Activity page or via a tokenized link emailed to them.
- **Direct messaging** — once two alumni connect, they can exchange messages one-on-one, with unread indicators surfaced on the Connections page.
- **People you may know** — recommendations built from the alumni directory, excluding yourself and people you're already connected to.
- **Notifications & recent activity** — a running feed of incoming requests and updates.
- **Secure accounts** — registration with email verification and optional two-factor authentication, built on ASP.NET Core Identity.

## Architecture

CougarConnect is a Blazor Server app: the browser holds a persistent SignalR connection to the server, which renders interactive components. Two data stores sit behind it — **ASP.NET Core Identity on SQL Server / LocalDB** handles accounts, sign-in, and 2FA, while **Supabase (PostgREST)** stores the alumni domain data (profiles, messages, connection requests, notifications), reached over `HttpClient` in [`SupabaseService`](CougarConnect/CougarConnect/Services/SupabaseService.cs). Transactional email (account confirmation, connection-request links) is sent over SMTP.

```mermaid
graph TD
    B[Browser] <-->|SignalR| BS[Blazor Server App]
    BS --> ID[ASP.NET Core Identity + EF Core]
    ID --> SQL[(SQL Server / LocalDB<br/>accounts, 2FA)]
    BS --> SS[SupabaseService<br/>HttpClient]
    SS --> SB[(Supabase PostgREST<br/>Alumni, Messages,<br/>ConnectionRequests, Notifications)]
    BS --> EM[EmailSender] --> SMTP[SMTP / Gmail]
```

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
