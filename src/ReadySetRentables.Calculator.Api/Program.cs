using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReadySetRentables.Calculator.Api.Endpoints;
using ReadySetRentables.Calculator.Api.Security;

namespace ReadySetRentables.Calculator.Api
{
    // Marked as partial so WebApplicationFactory<Program> in tests can see it
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Rate limiting configuration (extension in /Security)
            builder.Services.AddDefaultRateLimiting();

            // Optional but nice for later
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

            app.MapGet("/health", () => Results.Ok("healthy"));

            app.Run();
        }
    }
}
