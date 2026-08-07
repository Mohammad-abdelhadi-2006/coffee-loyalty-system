using System.Text.Json.Serialization;
using CoffeeLoyalty.Api.Data;
using CoffeeLoyalty.Api.Extensions;
using CoffeeLoyalty.Api.Middleware;
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

var app = builder.Build();

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

// Startup work: verify the Firebase credentials and bootstrap the admin account
// before the first request is served. Both fail fast on misconfiguration.
FirebaseInitializer.Initialize(
    app.Configuration,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(FirebaseInitializer)));

// Not fail-fast: an API serving only the Flutter app needs no origins at all, so this
// reports what the policy allows and warns when that is nothing (decision 41).
CorsSetupExtensions.WarnIfNoOriginsConfigured(
    app.Configuration,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(CorsSetupExtensions)));

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
