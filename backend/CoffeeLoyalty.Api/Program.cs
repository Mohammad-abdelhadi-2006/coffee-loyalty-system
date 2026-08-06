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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Startup work: verify the Firebase credentials and bootstrap the admin account
// before the first request is served. Both fail fast on misconfiguration.
FirebaseInitializer.Initialize(
    app.Configuration,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(FirebaseInitializer)));

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
