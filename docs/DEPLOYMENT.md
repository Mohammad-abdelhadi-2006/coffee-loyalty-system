# Deployment — Coffee Loyalty API (MonsterASP.NET)

> Everything on this page is configuration that is **deliberately not in the repository**.
> Nothing here is optional except where it says so. Work through it top to bottom **before**
> starting the app for the first time.
>
> Set every value in the hosting panel's **Environment Variables** section (MonsterASP.NET →
> your site → Configuration). The double underscore `__` is how a nested configuration key is
> written as an environment variable: `Jwt__Secret` means the `Jwt:Secret` setting.
>
> After changing any variable, **restart the site** — configuration is read at startup only.

---

## 1. `ConnectionStrings__DefaultConnection`

The production SQL Server database.

```
ConnectionStrings__DefaultConnection = Server=<db-host>;Database=<db-name>;User Id=<db-user>;Password=<db-password>;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Take the exact values from the MonsterASP.NET database panel. Do **not** reuse the LocalDB
string from `appsettings.Development.json` — it only exists on a developer machine.

**If missing:** the app starts and then every request fails with a 500.

---

## 2. `Jwt__Secret`

The HMAC-SHA256 key every token is signed with.

```
Jwt__Secret = <at least 32 characters, random>
```

Generate a fresh one; never reuse the development secret. Anyone holding this value can mint
tokens for any role.

```powershell
# one way to generate 48 random characters
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(36))
```

`Jwt__Issuer` and `Jwt__Audience` are already in the committed `appsettings.json` and need no
change.

**If missing or shorter than 32 characters:** the app **refuses to start** with
`Jwt:Secret is not configured` / `must be at least 32 characters long`.

---

## 3. `Firebase__CredentialsPath` (+ upload the JSON)

1. Download the service-account JSON from the Firebase console
   (Project settings → Service accounts → Generate new private key).
2. Upload it **outside the webroot** — e.g. `/home/site/secrets/firebase.json`, never inside
   `wwwroot` and never into the repository. It is a credential: anything under the webroot can
   be requested over HTTP.
3. Point the setting at the uploaded file:

```
Firebase__CredentialsPath = /full/path/to/firebase.json
```

**If missing:** the app starts and logs a warning; `POST /api/auth/firebase-login` rejects every
token, so **no customer can log in to the Flutter app**. The dashboard still works.
**If set but the file is not there:** the app **refuses to start**.

---

## 4. `Cors__AllowedOrigins__0`

The dashboard's origin, **exactly as the browser sends it**.

```
Cors__AllowedOrigins__0 = https://dashboard.example.com
```

Rules — get these wrong and the dashboard silently cannot talk to the API:

- Scheme + host + optional port. **No trailing slash. No path.**
  ✅ `https://dashboard.example.com`  ✅ `http://192.168.1.50:3000`
  ❌ `https://dashboard.example.com/`  ❌ `https://dashboard.example.com/login`
- Use the origin the **browser** shows in the address bar, not the server's internal name.
- `http://` and `https://` are different origins. So are different ports.
- For a second origin, add `Cors__AllowedOrigins__1`, then `__2`, and so on — indexes start at
  **0** and must not skip a number.
- Serve the API over **HTTPS** and point the dashboard at the `https://` URL. The API redirects
  HTTP to HTTPS, and browsers do not follow redirects on a CORS preflight.

**If missing:** the app starts and logs
`Cors:AllowedOrigins is empty — every browser request from the dashboard will be blocked by CORS`.
The Flutter app is unaffected; the dashboard is completely blocked.

**Verify it after deploying** (replace both URLs with the real ones):

```bash
curl -i -X OPTIONS https://api.example.com/api/orders \
  -H "Origin: https://dashboard.example.com" \
  -H "Access-Control-Request-Method: POST"
```

The response must be `204` and must contain:

```
Access-Control-Allow-Origin: https://dashboard.example.com
```

If that header is absent, the origin does not match — re-read the rules above.

---

## 5. Admin seed account

The dashboard has no sign-up (decision 4), so the first admin is seeded from configuration.

```
Seed__AdminUsername = <admin login name>
Seed__AdminPassword = <strong password>
Seed__AdminFullName = <display name>          # optional, defaults to the username
```

The seeder is idempotent: it creates the account only if that username does not exist, and it
never resets a password that has already been changed.

**If either the username or the password is missing:** the app **refuses to start**.

---

## 6. Run the migrations — BEFORE the first start

**There is no auto-migrate.** The application never creates or updates the schema, so the
database must already be at the latest migration before the app runs at all. If it is not,
`AdminSeeder` queries a table that does not exist and startup crashes.

From a developer machine with the .NET SDK, pointed at the **production** database:

```bash
cd backend/CoffeeLoyalty.Api

# quote the whole string; use the real production connection string
dotnet ef database update --connection "Server=<db-host>;Database=<db-name>;User Id=<db-user>;Password=<db-password>;TrustServerCertificate=True"
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Check it worked — the `Orders` table should exist and carry the `IdempotencyKey` column:

```sql
SELECT name FROM sys.tables ORDER BY name;
SELECT COUNT(*) FROM __EFMigrationsHistory;
```

**Repeat this step after every deploy that adds a migration**, before restarting the site.

---

## 7. Start the site and check

1. Restart the site in the hosting panel.
2. `GET https://api.example.com/api/health` → `{"status":"ok"}`.
3. Read the startup log and confirm:
   - `CORS policy 'CoffeeLoyaltyDashboard' allows 1 origin(s): https://dashboard.example.com.`
   - `Firebase Admin SDK initialised.`
   - `Seeded the admin account '<username>'.` (first run) or `already exists; seeding skipped.`
4. Log in to the dashboard with the seeded admin, then **change that password**.

Any `warn:` line in the startup log means one of the steps above was skipped.

---

## Quick checklist

| # | Setting | Missing ⇒ |
|---|---|---|
| 1 | `ConnectionStrings__DefaultConnection` | every request 500s |
| 2 | `Jwt__Secret` (≥ 32 chars) | **app will not start** |
| 3 | `Firebase__CredentialsPath` (+ file uploaded outside webroot) | no customer can log in |
| 4 | `Cors__AllowedOrigins__0` | dashboard blocked in the browser |
| 5 | `Seed__AdminUsername` + `Seed__AdminPassword` | **app will not start** |
| 6 | `dotnet ef database update` run first | **app crashes at startup** |
