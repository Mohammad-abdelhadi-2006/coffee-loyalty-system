# Deployment — Coffee Loyalty (MonsterASP.NET + Cloudflare)

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

## 0. The shape of the deployment

**One site, one origin.** The API serves the dashboard itself, under a prefix:

```
https://<your-domain>/               →  the public site (home, /menu, /aboutus, /contact, /download)
https://<your-domain>/downloads/…    →  the Android build the download page links to
https://<your-domain>/dashboard      →  the React dashboard
https://<your-domain>/api/...        →  the API
```

All three are one deployment. `/api` always wins, `/dashboard` is the dashboard's subtree, and
everything else falls through to the site's `index.html` — which is what keeps `/menu` and the
other client-side routes working on a refresh or a pasted link.

That layout is not incidental — it is what lets the dashboard's API client keep its one-line
base URL, `const BASE = '/api'` in `dashboard/src/api/client.js`. That path is absolute from
the root, so it resolves the same whether the page is served from `/` or `/dashboard`. Same
origin means nothing the browser sends is cross-origin, so **CORS never comes into it**
(see §5).

The Flutter app is the one client that *is* remote. It talks to the same `/api` over the
public domain and is unaffected by any of this.

---

## 1. Build both front-ends into the API

Each `npm run build` writes straight into the API's `wwwroot` — the dashboard into
`wwwroot/dashboard`, the site into the root beside it. Both output paths live in the
respective `vite.config.js`, not copied by hand. `dotnet publish` then picks `wwwroot` up on
its own, so these commands produce the entire deployable:

```bash
# Delete wwwroot first — see the warning below.
rm -rf backend/CoffeeLoyalty.Api/wwwroot

cd dashboard
npm ci                 # or: npm install
npm run build

cd ../website
npm ci
npm run build

cd ../backend/CoffeeLoyalty.Api
dotnet publish -c Release -o ./publish
```

**Order matters only in that both must run before `dotnet publish`.** The site's build does
*not* empty `wwwroot` (`emptyOutDir: false`), because doing so would delete the dashboard that
was just built into it.

Upload the contents of `publish/` to the site root over FTP or Web Deploy.

**Verify before uploading.** Both of these must exist:

```
publish/wwwroot/index.html                 ← the public site
publish/wwwroot/dashboard/index.html       ← the dashboard
```

The startup log states which of the two it found:

```
Public site is being served at / from ...\wwwroot\index.html.                  ← good
Dashboard is being served at /dashboard from ...\wwwroot\dashboard\index.html.  ← good
No public site in wwwroot; / will 404.                                          ← you skipped website/
No dashboard build in wwwroot/dashboard; /dashboard will 404.                    ← you skipped dashboard/
```

**Always delete `backend/CoffeeLoyalty.Api/wwwroot` before a rebuild.** Vite writes new files
under content-hashed names and removes none of the old ones, so without this every deploy
carries the accumulated assets of every past deploy — and a stale `index.html` left behind
would keep being served in preference to the new one.

### The Android build

`website/public/downloads/nakhat-finjan.apk` is copied to `wwwroot/downloads/` by the site's
build, and the download page links at it. It is ~48 MB, so it is most of the upload.

`.apk` is not in ASP.NET Core's default content-type table, and the static file middleware
refuses to serve an extension it does not know. `Program.cs` registers
`application/vnd.android.package-archive` for it explicitly — without that the link answers
404 and nothing in the log says why.

---

## 2. `ConnectionStrings__DefaultConnection`

The production SQL Server database.

```
ConnectionStrings__DefaultConnection = Server=<db-host>;Database=<db-name>;User Id=<db-user>;Password=<db-password>;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Take the exact values from the MonsterASP.NET database panel. Do **not** reuse the LocalDB
string from `appsettings.Development.json` — it only exists on a developer machine.

**If missing:** the app starts and then every request fails with a 500.

---

## 3. `Jwt__Secret`

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

## 4. `Firebase__CredentialsPath` (+ upload the JSON)

1. Download the service-account JSON from the Firebase console
   (Project settings → Service accounts → Generate new private key).
2. Upload it **outside the webroot** — never inside `wwwroot`, never into the repository. It is
   a credential, and anything under the webroot can be requested over HTTP.
3. Point the setting at the uploaded file:

```
Firebase__CredentialsPath = <full path to firebase.json>
```

**If missing:** the app starts and logs a warning; `POST /api/auth/firebase-login` rejects every
token, so **no customer can log in to the Flutter app**. The dashboard still works.
**If set but the file is not there:** the app **refuses to start**.

---

## 5. CORS — nothing to configure

Leave `Cors__AllowedOrigins` unset. The dashboard is served from the same origin as the API
(§0), so the browser sends no cross-origin request and asks for no preflight.

The startup log will say:

```
warn: Cors:AllowedOrigins is empty — every browser request from the dashboard will be blocked by CORS
```

**That warning is expected here and is not a problem.** It describes the two-site layout this
deployment does not use. Set an origin only if you later move the dashboard to its own
hostname — and in that case you must also change `BASE` in `dashboard/src/api/client.js` to
that API's absolute URL, because a relative `/api` would then point at the dashboard's own host.

---

## 6. Admin seed account

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

## 7. Create the schema — BEFORE the first start

**There is no auto-migrate.** The application never creates or updates the schema, so the
database must already be at the latest migration before the app runs at all. If it is not,
`AdminSeeder` queries a table that does not exist and startup crashes.

Either run `docs/schema.sql` in the MonsterASP.NET SQL panel — it is generated with
`--idempotent`, so it creates only what is missing and is safe to re-run — or, from a machine
with the .NET SDK pointed at the production database:

```bash
cd backend/CoffeeLoyalty.Api
dotnet ef database update --connection "Server=<db-host>;Database=<db-name>;User Id=<db-user>;Password=<db-password>;TrustServerCertificate=True"
```

Regenerate `docs/schema.sql` after adding a migration:

```bash
dotnet ef migrations script --idempotent -o ../../docs/schema.sql
```

**Repeat this step after every deploy that adds a migration**, before restarting the site.

---

## 8. Import the opening balances — once

`import-opening-balances.sql` loads the shop's paper punch-card balances: 67 customers, 7,243
points, at 5 points per dinar (the rate those cards were punched under, which is *not* today's
`LoyaltyConstants.PointsPerDinar` of 3). Run it in the SQL panel after §7.

> **The script is deliberately not in this repository.** It is 67 real phone numbers, each
> paired with what that customer had spent, and this repository is public (decisions.md #4).
> It is `.gitignore`d and kept by whoever runs the deploy — ask them for it rather than
> recreating it, so nobody's balance gets imported twice at a different rate.

It is safe to re-run — both inserts are guarded, and
`UX_PointsTransaction_Customer_OpeningBalance` is the database's own refusal of a second
opening balance per customer. The script prints its own verification at the end: 67 customers,
67 opening rows, 7243 points, and an empty second result set. **A non-empty second result set
means a balance disagrees with its ledger — stop and investigate.**

`docs/deploy-reset.sql` is the development counterpart and is **destructive**: it empties
orders, customers and products. A freshly created production database has nothing to delete, so
do not run it there.

---

## 9. Cloudflare

Point the domain at the MonsterASP.NET site, then in **SSL/TLS → Overview** set the encryption
mode to **Full** or **Full (strict)**.

Do **not** use **Flexible**. Flexible makes Cloudflare talk to the origin over plain HTTP while
the browser is on HTTPS; `UseHttpsRedirection` then answers with a redirect to HTTPS, Cloudflare
sends it back over HTTP, and the browser gives up with `ERR_TOO_MANY_REDIRECTS` on every page.
`UseForwardedHeaders` in `Program.cs` is what lets the app read the browser's real scheme from
`X-Forwarded-Proto` — but under Flexible that header honestly says `http`, so the loop is real
and no amount of app configuration fixes it.

---

## 10. Start the site and check

1. Restart the site in the hosting panel.
2. `https://<your-domain>/api/health` → `{"status":"ok"}`.
3. `https://<your-domain>/dashboard` → the dashboard's login page.
4. `https://<your-domain>/dashboard/assets/…` (any file the page requests) → loads, not a 404.
   If the page renders unstyled or blank, this is what to check first.
5. `https://<your-domain>/` → the public site's home page.
6. `https://<your-domain>/menu` typed **directly into the address bar** → the menu page, not a
   404. This is the one that proves the SPA fallback is working; reaching it by clicking from
   the home page does not test it, because that never hits the server.
7. `https://<your-domain>/downloads/nakhat-finjan.apk` → downloads, ~48 MB. A 404 here means
   the `.apk` content type did not register; a 0-byte file means the upload was truncated.
8. `https://<your-domain>/api/nope` → a 404, **not** an HTML page. If this returns the site's
   home page, the fallback order is wrong and every failed API call will look like a JSON
   parse error to the dashboard.
9. Read the startup log and confirm:
   - `Public site is being served at / from ...`
   - `Dashboard is being served at /dashboard from ...`
   - `Firebase Admin SDK initialised.`
   - `Seeded the admin account '<username>'.` (first run) or `already exists; seeding skipped.`
10. Log in to the dashboard with the seeded admin, then **change that password**.
11. Enter the real menu from the dashboard's Products page — the catalogue ships empty.

Any `warn:` line other than the expected CORS one in §5 means a step above was skipped.

---

## 11. The Flutter app

`Config.baseUrl` in `mobile/nakhat_finjan/lib/core/config.dart` is still the emulator's address.
Before building a release APK, set it to `https://<your-domain>` and register the release
signing key's SHA-1 fingerprint in the Firebase console — phone sign-in fails on real devices
without it. See `mobile/nakhat_finjan/README-firebase.md`.

---

## Quick checklist

| # | Step | Skipped ⇒ |
|---|---|---|
| 1 | `npm run build` before `dotnet publish` | `/dashboard` 404s; API still works |
| 2 | `ConnectionStrings__DefaultConnection` | every request 500s |
| 3 | `Jwt__Secret` (≥ 32 chars) | **app will not start** |
| 4 | `Firebase__CredentialsPath` (+ file uploaded outside webroot) | no customer can log in |
| 5 | CORS — nothing to set | — |
| 6 | `Seed__AdminUsername` + `Seed__AdminPassword` | **app will not start** |
| 7 | schema created first | **app crashes at startup** |
| 8 | `import-opening-balances.sql` | 67 customers start from zero |
| 9 | Cloudflare SSL = Full | redirect loop on every page |
| 11 | app `baseUrl` + release SHA-1 | app cannot reach the API / cannot log in |
