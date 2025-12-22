using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Integration;

/// <summary>
/// Integration tests for market endpoints that exercise the full API stack
/// including real database connectivity.
/// </summary>
public class MarketEndpointIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public MarketEndpointIntegrationTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMarkets_ReturnsOk_WithMarketData()
    {
        var response = await _client.GetAsync("/api/v1/markets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<MarketsResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Markets);
    }

    [Fact]
    public async Task GetMarkets_ReturnsMarketsWithRequiredFields()
    {
        var response = await _client.GetAsync("/api/v1/markets");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MarketsResponse>();

        foreach (var market in result!.Markets)
        {
            Assert.False(string.IsNullOrWhiteSpace(market.Id), "Market Id should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(market.Name), "Market Name should not be empty");
            Assert.True(market.NeighborhoodCount >= 0, "NeighborhoodCount should be non-negative");
            Assert.True(market.ListingCount >= 0, "ListingCount should be non-negative");
        }
    }

    [Fact]
    public async Task GetNeighborhoods_ReturnsOk_ForValidMarket()
    {
        // First get a valid market
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        Assert.NotNull(marketsResponse);
        Assert.NotEmpty(marketsResponse!.Markets);

        var marketId = marketsResponse.Markets.First().Id;

        // Then get neighborhoods for that market
        var response = await _client.GetAsync($"/api/v1/markets/{marketId}/neighborhoods");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<NeighborhoodsResponse>();
        Assert.NotNull(result);
        Assert.Equal(marketId, result!.Market);
    }

    [Fact]
    public async Task GetNeighborhoods_ReturnsNeighborhoodsWithRequiredFields()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var response = await _client.GetAsync($"/api/v1/markets/{marketId}/neighborhoods");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NeighborhoodsResponse>();

        Assert.NotEmpty(result!.Neighborhoods);

        foreach (var neighborhood in result.Neighborhoods)
        {
            Assert.False(string.IsNullOrWhiteSpace(neighborhood.Name), "Neighborhood Name should not be empty");
            Assert.True(neighborhood.ListingCount >= 0, "ListingCount should be non-negative");
            Assert.True(neighborhood.AvgPrice >= 0, "AvgPrice should be non-negative");
            Assert.True(neighborhood.AvgOccupancy >= 0, "AvgOccupancy should be non-negative");
        }
    }

    [Fact]
    public async Task GetConfigurations_ReturnsOk_ForValidMarketAndNeighborhood()
    {
        // Get a valid market
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        // Get a valid neighborhood
        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhoodName = neighborhoodsResponse!.Neighborhoods.First().Name;

        // Get configurations
        var response = await _client.GetAsync(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhoodName}/configurations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConfigurationsResponse>();
        Assert.NotNull(result);
        Assert.Equal(marketId, result!.Market);
        Assert.Equal(neighborhoodName, result.Neighborhood);
    }

    [Fact]
    public async Task GetConfigurations_ReturnsConfigurationsWithRequiredFields()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhoodName = neighborhoodsResponse!.Neighborhoods.First().Name;

        var response = await _client.GetAsync(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhoodName}/configurations");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ConfigurationsResponse>();

        Assert.NotEmpty(result!.Configurations);

        foreach (var config in result.Configurations)
        {
            Assert.True(config.Bedrooms >= 0, "Bedrooms should be non-negative");
            Assert.True(config.Bathrooms >= 0, "Bathrooms should be non-negative");
            Assert.True(config.ListingCount >= 0, "ListingCount should be non-negative");
        }
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
