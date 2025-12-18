using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ReadySetRentables.Calculator.Api.Security;

/// <summary>
/// Extension methods for configuring rate limiting.
/// </summary>
public static class RateLimitingExtensions
{
        /// <summary>
        /// Adds rate limiting services with configurable settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration to read settings from.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddDefaultRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Token bucket defaults tuned for UI friendliness
        var tokenLimit = configuration.GetValue("RateLimiting:TokenLimit", 300);                 // burst
        var tokensPerPeriod = configuration.GetValue("RateLimiting:TokensPerPeriod", 100);      // refill amount
        var replenishmentSeconds = configuration.GetValue("RateLimiting:ReplenishmentSeconds", 10);
        var queueLimit = configuration.GetValue("RateLimiting:QueueLimit", 50);
    
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
            options.AddPolicy(RateLimitingPolicies.Default, httpContext =>
            {
                // Optional: don’t let Swagger “spend” the same bucket as your UI
                var referer = httpContext.Request.Headers.Referer.ToString();
                if (referer.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
                {
                    // Either no limiter:
                    // return RateLimitPartition.GetNoLimiter("swagger");
    
                    // Or a very generous separate limiter:
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: "swagger",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 1000,
                            TokensPerPeriod = 500,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 100
                        });
                }
    
                // Use first IP from X-Forwarded-For if present, else RemoteIpAddress
                var xff = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                var ip = xff?.Split(',').FirstOrDefault()?.Trim()
                         ?? httpContext.Connection.RemoteIpAddress?.ToString()
                         ?? "unknown";
    
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: ip,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = tokenLimit,
                        TokensPerPeriod = tokensPerPeriod,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentSeconds),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = queueLimit
                    });
            });
        });
    
        return services;
    }
}
