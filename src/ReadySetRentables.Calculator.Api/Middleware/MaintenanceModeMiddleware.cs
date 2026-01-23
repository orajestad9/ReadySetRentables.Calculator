namespace ReadySetRentables.Calculator.Api.Middleware;

/// <summary>
/// Middleware that checks for maintenance mode and returns 503 for all requests except /health
/// </summary>
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceModeMiddleware> _logger;
    private readonly bool _isMaintenanceMode;

    public MaintenanceModeMiddleware(
        RequestDelegate next,
        ILogger<MaintenanceModeMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        var maintenanceModeValue = configuration["MAINTENANCE_MODE"] ?? "false";
        _isMaintenanceMode = bool.TryParse(maintenanceModeValue, out var result) && result;

        if (_isMaintenanceMode)
        {
            _logger.LogWarning("API is running in MAINTENANCE MODE");
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow health checks to pass through even in maintenance mode
        if (_isMaintenanceMode && !context.Request.Path.StartsWithSegments("/health"))
        {
            _logger.LogInformation(
                "Request blocked due to maintenance mode: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "API is currently undergoing maintenance",
                status = "unavailable"
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _next(context);
    }
}
