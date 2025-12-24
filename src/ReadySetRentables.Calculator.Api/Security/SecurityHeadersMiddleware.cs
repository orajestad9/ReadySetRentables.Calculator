using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ReadySetRentables.Calculator.Api.Security;

/// <summary>
/// Extension methods for adding security headers to responses.
/// </summary>
public static class SecurityHeadersMiddleware
{
    /// <summary>
    /// Adds security headers to all responses.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(async (context, next) =>
        {
            // Prevent MIME type sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Prevent clickjacking
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Enable XSS filter in browsers
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // Control referrer information
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Content Security Policy for API (restrictive since we only serve JSON)
            context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

            // Permissions Policy (disable unnecessary browser features)
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            await next();
        });
    }
}
