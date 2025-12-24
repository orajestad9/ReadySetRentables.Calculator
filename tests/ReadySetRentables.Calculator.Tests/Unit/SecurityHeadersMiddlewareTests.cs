using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using ReadySetRentables.Calculator.Api.Security;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public void UseSecurityHeaders_ThrowsArgumentNullException_WhenAppIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SecurityHeadersMiddleware.UseSecurityHeaders(null!));
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsXContentTypeOptionsHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsXFrameOptionsHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsXXssProtectionHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.Equal("1; mode=block", response.Headers.GetValues("X-XSS-Protection").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsReferrerPolicyHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsContentSecurityPolicyHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.Equal("default-src 'none'; frame-ancestors 'none'",
            response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsPermissionsPolicyHeader()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("Permissions-Policy"));
        Assert.Equal("geolocation=(), microphone=(), camera=()",
            response.Headers.GetValues("Permissions-Policy").First());
    }

    [Fact]
    public async Task UseSecurityHeaders_AddsAllSecurityHeaders()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        var expectedHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Referrer-Policy",
            "Content-Security-Policy",
            "Permissions-Policy"
        };

        foreach (var header in expectedHeaders)
        {
            Assert.True(response.Headers.Contains(header), $"Missing header: {header}");
        }
    }

    private static async Task<IHost> CreateTestHost()
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseSecurityHeaders();
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });
                    });
            })
            .StartAsync();
    }
}
