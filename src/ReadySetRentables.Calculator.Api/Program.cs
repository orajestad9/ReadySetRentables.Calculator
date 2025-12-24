using ReadySetRentables.Calculator.Api.Configuration;
using ReadySetRentables.Calculator.Api.Data;
using ReadySetRentables.Calculator.Api.Endpoints;
using ReadySetRentables.Calculator.Api.Logic;
using ReadySetRentables.Calculator.Api.Security;
using Microsoft.AspNetCore.Diagnostics;

namespace ReadySetRentables.Calculator.Api;

/// <summary>
/// Application entry point. Marked as partial for WebApplicationFactory testability.
/// </summary>
public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register configuration options
        builder.Services.Configure<AnalysisOptions>(
            builder.Configuration.GetSection(AnalysisOptions.SectionName));

        // Register services
        builder.Services.AddSingleton<IRoiCalculator, RoiCalculator>();
        builder.Services.AddSingleton<INeighborhoodRepository, NeighborhoodRepository>();
        builder.Services.AddSingleton<IMarketRepository, MarketRepository>();
        builder.Services.AddSingleton<IAnalysisService, AnalysisService>();

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

        // Global exception handler with RFC 7807 Problem Details
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error;

                logger.LogError(ex,
                    "Unhandled exception. {Method} {Path} TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "An unexpected error occurred",
                    status = StatusCodes.Status500InternalServerError,
                    detail = app.Environment.IsDevelopment() ? ex?.Message : "An internal server error occurred. Please try again later.",
                    instance = context.Request.Path.ToString(),
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });

        // Security headers
        app.UseSecurityHeaders();

        app.UseRouting();

        // CORS
        app.UseCors();

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
        var api = app.MapGroup("/api");

        api.MapCalculatorEndpoints();
        api.MapMarketEndpoints();
        api.MapAnalyzeEndpoints();


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
