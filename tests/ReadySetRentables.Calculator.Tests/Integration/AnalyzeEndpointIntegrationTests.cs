using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Integration;

/// <summary>
/// Integration tests for the analyze endpoint that exercise the full API stack
/// including real database connectivity and analysis calculations.
/// </summary>
public class AnalyzeEndpointIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public AnalyzeEndpointIntegrationTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Analyze_ReturnsOk_WithValidRequest()
    {
        // First get valid market/neighborhood/configuration from the API
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        // Build the request using real data
        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_ReturnsCompleteResponse_WithAllSections()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result!.Summary);
        Assert.NotNull(result.Profile);
        Assert.NotNull(result.Insights);
        Assert.NotNull(result.Metrics);
        Assert.NotNull(result.Revenue);
        Assert.NotNull(result.Expenses);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public async Task Analyze_ReturnsValidMetrics()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        // Metrics should be reasonable decimal values
        Assert.True(result!.Metrics.CashOnCashReturn >= -1m && result.Metrics.CashOnCashReturn <= 1m,
            $"CashOnCashReturn {result.Metrics.CashOnCashReturn} should be between -1 and 1");
        Assert.True(result.Metrics.CapRate >= -1m && result.Metrics.CapRate <= 1m,
            $"CapRate {result.Metrics.CapRate} should be between -1 and 1");
        Assert.True(result.Metrics.GrossYield >= 0m && result.Metrics.GrossYield <= 1m,
            $"GrossYield {result.Metrics.GrossYield} should be between 0 and 1");
        Assert.True(result.Metrics.BreakEvenOccupancy >= 0m,
            $"BreakEvenOccupancy {result.Metrics.BreakEvenOccupancy} should be non-negative");
    }

    [Fact]
    public async Task Analyze_ReturnsValidRevenue()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.True(result!.Revenue.NightlyRate.Value > 0, "Nightly rate should be positive");
        Assert.True(result.Revenue.GrossAnnualRevenue > 0, "Annual revenue should be positive");
        Assert.True(result.Revenue.OccupancyRate.Value > 0, "Occupancy rate should be positive");
        Assert.True(result.Revenue.ComparablesCount >= 0, "Comparables count should be non-negative");
    }

    [Fact]
    public async Task Analyze_ReturnsValidExpenses()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.True(result!.Expenses.AnnualTotal > 0, "Annual expenses should be positive");
        Assert.True(result.Expenses.Monthly > 0, "Monthly expenses should be positive");
        Assert.NotEmpty(result.Expenses.Breakdown);

        // Check that expected expense categories exist
        Assert.True(result.Expenses.Breakdown.ContainsKey("mortgage"), "Should include mortgage");
        Assert.True(result.Expenses.Breakdown.ContainsKey("propertyTax"), "Should include property tax");
        Assert.True(result.Expenses.Breakdown.ContainsKey("insurance"), "Should include insurance");
    }

    [Fact]
    public async Task Analyze_ReturnsNotFound_ForInvalidNeighborhood()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = "NonexistentNeighborhood12345",
            Bedrooms = 2,
            Bathrooms = 1m,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_ReturnsBadRequest_ForInvalidPurchasePrice()
    {
        var request = new AnalyzeRequest
        {
            Market = "san-diego",
            Neighborhood = "Mission Bay",
            Bedrooms = 2,
            Bathrooms = 1m,
            PurchasePrice = 0m, // Invalid
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_IncludesPropertyManagement_WhenNotSelfManaged()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = false, // Not self-managed
            HoaMonthly = 0m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.True(result!.Expenses.Breakdown.ContainsKey("propertyManagement"));
        Assert.True(result.Expenses.Breakdown["propertyManagement"].Value > 0,
            "Property management fee should be positive when not self-managed");
    }

    [Fact]
    public async Task Analyze_IncludesHoa_WhenProvided()
    {
        var marketsResponse = await _client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");
        var marketId = marketsResponse!.Markets.First().Id;

        var neighborhoodsResponse = await _client.GetFromJsonAsync<NeighborhoodsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods");
        var neighborhood = neighborhoodsResponse!.Neighborhoods.First();

        var configurationsResponse = await _client.GetFromJsonAsync<ConfigurationsResponse>(
            $"/api/v1/markets/{marketId}/neighborhoods/{neighborhood.Name}/configurations");
        var config = configurationsResponse!.Configurations.First();

        var hoaMonthly = 350m;
        var request = new AnalyzeRequest
        {
            Market = marketId,
            Neighborhood = neighborhood.Name,
            Bedrooms = config.Bedrooms,
            Bathrooms = config.Bathrooms,
            PurchasePrice = 500000m,
            DownPaymentPercent = 20m,
            InterestRate = 7m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = hoaMonthly
        };

        var response = await _client.PostAsJsonAsync("/api/v1/analyze", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.True(result!.Expenses.Breakdown.ContainsKey("hoa"));
        Assert.Equal(hoaMonthly * 12, result.Expenses.Breakdown["hoa"].Value);
    }
}
