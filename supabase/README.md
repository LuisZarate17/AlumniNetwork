# Supabase setup

CougarConnect uses a single Supabase Postgres database for **both** the alumni
domain data (reached over the PostgREST API by `SupabaseService`) and the
ASP.NET Core Identity account tables (reached over a direct Npgsql connection
by EF Core).

## Files

| File | What it creates | How it's applied |
|------|-----------------|------------------|
| `schema.sql` | The four domain tables: `Alumni`, `Messages`, `ConnectionRequests`, `Notifications` | Run manually in the Supabase **SQL Editor** |
| `seed.sql` | Ten fictional alumni profiles for the demo | Run manually **after** `schema.sql` |
| `identity-schema.sql` | The ASP.NET Identity tables (`AspNetUsers`, etc.) | **Auto-applied** by the app on startup — see below. Kept here as documentation/fallback. |

## First-time setup

1. In the Supabase dashboard for the project, open **SQL Editor**.
2. Paste and run `schema.sql` (choose **Run without RLS** — see note below).
3. Paste and run `seed.sql`.

That's it for manual steps. The Identity tables in `identity-schema.sql` are
created automatically the first time the app connects: `Program.cs` runs
`db.Database.Migrate()` on startup (guarded by `IsRelational()` so it's a no-op
under the in-memory provider the tests use). If you'd rather create them by hand
— or the app's DB user lacks DDL rights — run `identity-schema.sql` in the SQL
Editor too; it's idempotent and safe to re-run.

## Connection details the app needs

- **PostgREST (domain data):** `Supabase:Url` + `Supabase:ApiKey` (the project's
  **anon** key). Used server-side only.
- **Direct Postgres (Identity):** `ConnectionStrings:DefaultConnection`, an Npgsql
  connection string. Use the **Session pooler** string from
  Supabase → Project Settings → Database (it's IPv4-friendly, which matters on
  hosts like Render). Format:
  `Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<db-password>;SSL Mode=Require`

Set both locally via `dotnet user-secrets` and in production via the host's
environment variables (`Supabase__Url`, `Supabase__ApiKey`,
`ConnectionStrings__DefaultConnection`). Never commit real values.

## Why RLS is off

Row Level Security is intentionally disabled on the domain tables. The anon key
is only ever used server-side by the Blazor Server backend (never sent to the
browser), and per-user authorization is handled by ASP.NET Core Identity in
front of it. A "RLS disabled" warning in the Supabase Advisor for these tables
is expected and not a real exposure here.
