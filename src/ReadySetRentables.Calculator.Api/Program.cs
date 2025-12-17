using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReadySetRentables.Calculator.Api.Data;
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
        builder.Services.AddSingleton<INeighborhoodRepository, NeighborhoodRepository>();
        builder.Services.AddSingleton<IMarketRepository, MarketRepository>();
        builder.Services.AddSingleton<IAnalysisService, AnalysisService>();

        // Rate limiting configuration
        builder.Services.AddDefaultRateLimiting(builder.Configuration);

        // CORS configuration
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });
        
        // OpenAPI/Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Problem Details for consistent error responses
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Global exception handler
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("An unhandled exception occurred");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    status = 500,
                    detail = "An unexpected error occurred. Please try again later."
                });
            });
        });

        // Security headers
        app.UseSecurityHeaders();

        app.UseRouting();

        // CORS
        app.UseCors();

        // Rate limiting
        app.UseRateLimiter();        

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                context.Response.Headers.Remove("Content-Security-Policy");
                context.Response.Headers.Remove("Content-Security-Policy-Report-Only");
            }

            await next();
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Map endpoints
        app.MapCalculatorEndpoints();
        app.MapMarketEndpoints();
        app.MapAnalyzeEndpoints();

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
