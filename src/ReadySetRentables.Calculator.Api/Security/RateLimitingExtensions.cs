using System.Threading.RateLimiting;

namespace ReadySetRentables.Calculator.Api.Security
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddDefaultRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Name of the policy used by .RequireRateLimiting("default")
                options.AddPolicy("default", httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,                      // 60 requests
                            Window = TimeSpan.FromMinutes(1),      // per minute
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0                          // no queuing
                        });
                });
            });

            return services;
        }
    }
}
