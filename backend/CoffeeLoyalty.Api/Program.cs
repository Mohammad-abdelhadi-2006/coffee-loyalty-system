using System.Text.Json.Serialization;
using CoffeeLoyalty.Api.Common;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Enums travel as their member names ("Piece", "HotCoffee") per the contract, in
        // both directions. An unrecognized name fails binding, which the unified
        // validation errors below turn into VALIDATION_ERROR.
        // EmployeeRole is deliberately not on the wire as an enum: its wire form is
        // lowercase and is mapped by hand in the Employees module.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .UseUnifiedValidationErrors();

// JWT bearer + the customer / cashier / admin policies + the auth services.
builder.Services.AddCoffeeLoyaltyAuth(builder.Configuration);

// The services behind the product / customer / employee / order modules.
builder.Services.AddCoffeeLoyaltyServices();

// The dashboard calls this API from another origin, so the browser needs an explicit
// opt-in. Origins come from configuration, never from code (decision 41).
builder.Services.AddCoffeeLoyaltyCors(builder.Configuration);

// In production the app sits behind two proxies — MonsterASP's IIS, and Cloudflare in front
// of that — so the scheme Kestrel sees is the proxy's hop, not the browser's. Without this,
// UseHttpsRedirection below reads "http", answers an already-HTTPS request with a redirect to
// HTTPS, and Cloudflare loops it back: ERR_TOO_MANY_REDIRECTS on every page.
//
// KnownNetworks/KnownProxies are cleared because the forwarding hop is not on a network this
// app can enumerate. That is safe only because nothing here trusts the client address for a
// security decision — the login throttle keys on username and phone, never on IP.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Must run before anything that reads the scheme or the client address.
app.UseForwardedHeaders();

// First in the pipeline: nothing downstream may return a body that is not { code, message }.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
     app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Before authentication, and that ordering is the whole point: a preflight OPTIONS carries
// no Authorization header, so an authenticated pipeline would answer it with a 401 the
// browser never shows anyone — the dashboard would just see every request fail (decision 41).
app.UseCors(CorsSetupExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── The dashboard, served from this same site under /dashboard ───────────────────────────
// Present only when a build has been copied into wwwroot/dashboard (see docs/DEPLOYMENT.md);
// an API-only deployment simply has no such folder and skips all of this rather than failing
// to start.
//
// Serving it here is what makes the dashboard's `BASE = '/api'` work untouched: the page and
// the API share an origin, so nothing is cross-origin and the CORS policy never comes into it.
// `/api` is absolute from the root, so moving the pages under /dashboard does not affect it.
var dashboardIndex = Path.Combine(
    app.Environment.WebRootPath ?? string.Empty,
    "dashboard",
    "index.html");

if (File.Exists(dashboardIndex))
{
    // Static files only — no UseDefaultFiles. That middleware would answer "/" with
    // wwwroot/index.html, and the site root is deliberately empty until the public site
    // exists. Asset URLs mirror the folder layout (/dashboard/assets/… ->
    // wwwroot/dashboard/assets/…), so nothing here needs a rewrite.
    app.UseStaticFiles();

    // Three cases, in this order:
    //
    // 1. /api — a mistyped endpoint must stay a 404. Falling through to the HTML below would
    //    answer fetch() with 200 and a page of markup, and the dashboard's client parses the
    //    body as JSON, so the real error would surface as an unintelligible parse failure.
    // 2. /dashboard — anything under the prefix that is not a real file gets index.html. Today
    //    the app navigates by React state and lives entirely on /dashboard, so this mostly
    //    catches the bare prefix with no trailing slash; it is also what would keep a future
    //    client-side route working after a refresh.
    // 3. Everything else, "/" included — an explicit 404. The root is reserved for the public
    //    site and is not the dashboard's.
    app.MapFallback(async context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/dashboard"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(dashboardIndex);
    });

    app.Logger.LogInformation("Dashboard is being served at /dashboard from {Path}.", dashboardIndex);
}
else
{
    app.Logger.LogInformation(
        "No dashboard build in wwwroot/dashboard; serving the API only. Run 'npm run build' in dashboard/ to include it.");
}

// Startup work: verify the Firebase credentials and bootstrap the admin account before the
// first request is served. Both fail fast on misconfiguration — deliberately, but a fail-fast
// nobody can read is just a crash, so the reason is written somewhere durable first.
//
// Under IIS in-process hosting this is not paranoia. A throw here happens before the host is
// listening, and ANCM reports it to the Windows event log as nothing more useful than
// "process terminated unexpectedly, exit code 0xffffffff" — with stdoutLogEnabled on, the
// stdout file is routinely still empty, because the process died before that plumbing
// flushed. On shared hosting the event log is not yours to read either. So the exception is
// written to a file next to the application, where FTP can reach it.
try
{
    FirebaseInitializer.Initialize(
        app.Configuration,
        app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(FirebaseInitializer)),
        app.Environment.ContentRootPath);

    // Not fail-fast: an API serving only the Flutter app needs no origins at all, so this
    // reports what the policy allows and warns when that is nothing (decision 41).
    CorsSetupExtensions.WarnIfNoOriginsConfigured(
        app.Configuration,
        app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CorsSetupExtensions)));

    using (var scope = app.Services.CreateScope())
    {
        await AdminSeeder.SeedAsync(scope.ServiceProvider);
    }
}
catch (Exception ex)
{
    StartupFailure.Record(app.Environment.ContentRootPath, app.Logger, ex);

    // Rethrown on purpose: a misconfigured app must not come up half-working and start
    // taking orders. The difference from before is only that the reason is now legible.
    throw;
}

app.Run();
