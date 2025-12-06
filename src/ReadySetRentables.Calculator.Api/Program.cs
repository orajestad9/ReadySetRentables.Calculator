using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReadySetRentables.Calculator.Api.Endpoints;
using ReadySetRentables.Calculator.Api.Logic;
using ReadySetRentables.Calculator.Api.Security;

namespace ReadySetRentables.Calculator.Api;

/// <summary>
/// Application entry point. Marked as partial for WebApplicationFactory testability.
/// </summary>
public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddSingleton<IRoiCalculator, RoiCalculator>();

        // Rate limiting configuration
        builder.Services.AddDefaultRateLimiting(builder.Configuration);

        // OpenAPI/Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Use the rate limiter middleware
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Map calculator endpoints
        app.MapCalculatorEndpoints();

        app.MapGet("/", () => Results.Ok(new
        {
            name = "ReadySetRentables Calculator API",
            status = "ok",
            version = "v1"
        }));

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        app.Run();
    }
}
