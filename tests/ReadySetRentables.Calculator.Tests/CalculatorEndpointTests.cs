using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ReadySetRentables.Calculator.Api;
using ReadySetRentables.Calculator.Api.Domain;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ReadySetRentables.Calculator.Tests;

public class CalculatorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CalculatorEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RoiEndpoint_ReturnsOk_WithValidPayload()
    {
        var input = new RentalInputs
        {
            NightlyRate = 150m,
            NightsBookedPerMonth = 20,
            CleaningFeePerStay = 80m,
            StaysPerMonth = 10,
            MonthlyFixedCosts = 2500m,
            PurchasePrice = 400000m
        };

        var response = await _client.PostAsJsonAsync("/api/calculator/roi", input);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RentalResult>();
        Assert.NotNull(result);
        Assert.True(result!.MonthlyRevenue > 0);
        Assert.True(result.CapRatePercent > 0);
    }

    [Fact]
    public async Task RoiEndpoint_ReturnsBadRequest_OnInvalidPayload()
    {
        var input = new RentalInputs
        {
            NightlyRate = 100m,
            NightsBookedPerMonth = 10,
            CleaningFeePerStay = 50m,
            StaysPerMonth = 5,
            MonthlyFixedCosts = 1000m,
            PurchasePrice = 0m // invalid
        };

        var response = await _client.PostAsJsonAsync("/api/calculator/roi", input);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RoiEndpoint_EnforcesRateLimit()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
    
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    // Force a tiny bucket so we reliably hit 429 fast
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["RateLimiting:TokenLimit"] = "5",
                        ["RateLimiting:TokensPerPeriod"] = "5",
                        ["RateLimiting:ReplenishmentSeconds"] = "3600", // no refill during test run
                        ["RateLimiting:QueueLimit"] = "0"
                    });
                });
            });
    
        using var client = factory.CreateClient();
    
        var input = new RentalInputs
        {
            NightlyRate = 100m,
            NightsBookedPerMonth = 10,
            CleaningFeePerStay = 50m,
            StaysPerMonth = 5,
            MonthlyFixedCosts = 1000m,
            PurchasePrice = 300000m
        };
    
        var saw429 = false;
    
        for (var i = 0; i < 20; i++)
        {
            var response = await client.PostAsJsonAsync("/api/calculator/roi", input);
    
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                saw429 = true;
                break;
            }
        }
    
        Assert.True(saw429, "Expected to hit rate limit (429) but never did.");
    }
}
