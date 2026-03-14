using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReadySetRentables.Calculator.Api.Middleware;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class MaintenanceModeMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsServiceUnavailable_WhenMaintenanceModeEnabled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            maintenanceMode: "true",
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/markets";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var payload = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        Assert.Equal("API is currently undergoing maintenance", payload.GetProperty("error").GetString());
        Assert.Equal("unavailable", payload.GetProperty("status").GetString());
    }

    [Fact]
    public async Task InvokeAsync_AllowsHealthEndpoint_WhenMaintenanceModeEnabled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            maintenanceMode: "true",
            next: context =>
            {
                nextCalled = true;
                context.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AllowsRequests_WhenMaintenanceModeDisabled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            maintenanceMode: "false",
            next: context =>
            {
                nextCalled = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/analyze";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
    }

    private static MaintenanceModeMiddleware CreateMiddleware(string maintenanceMode, RequestDelegate next)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MAINTENANCE_MODE"] = maintenanceMode
            })
            .Build();

        var logger = Substitute.For<ILogger<MaintenanceModeMiddleware>>();
        return new MaintenanceModeMiddleware(next, logger, configuration);
    }
}
