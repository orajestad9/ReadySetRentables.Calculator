using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using ReadySetRentables.Calculator.Api;
using ReadySetRentables.Calculator.Api.Data;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using ReadySetRentables.Calculator.Api.Logic;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class ApiEndpointTests
{
    [Fact]
    public async Task Root_ReturnsApiMetadata()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_ReturnsValidationProblem_ForInvalidRequest()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var request = new AnalyzeRequest
        {
            Market = "",
            Neighborhood = "",
            Bedrooms = 99,
            Bathrooms = 0m,
            DownPaymentPercent = 20m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await client.PostAsJsonAsync("/api/v1/analyze", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(
            factory.AnalysisService.ReceivedCalls(),
            call => call.GetMethodInfo().Name == nameof(IAnalysisService.AnalyzeAsync));
    }

    [Fact]
    public async Task Analyze_ReturnsNoContent_WhenAnalysisFails()
    {
        using var factory = new TestApiFactory();
        factory.AnalysisService.AnalyzeAsync(Arg.Any<AnalyzeRequest>())
            .Returns(new AnalysisResult
            {
                Success = false,
                ErrorMessage = "No data available"
            });

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/analyze", CreateAnalyzeRequest());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Analyze_ReturnsAnalysisResponse_WhenAnalysisSucceeds()
    {
        using var factory = new TestApiFactory();
        factory.AnalysisService.AnalyzeAsync(Arg.Any<AnalyzeRequest>())
            .Returns(new AnalysisResult
            {
                Success = true,
                Response = CreateAnalyzeResponse()
            });

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/analyze", CreateAnalyzeRequest());
        var payload = await response.Content.ReadFromJsonAsync<AnalyzeResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("buy", payload!.Summary.Recommendation);
    }

    [Fact]
    public async Task Analyze_BindsMissingBedroomsAndBathrooms_AsNull()
    {
        using var factory = new TestApiFactory();
        factory.AnalysisService.AnalyzeAsync(Arg.Any<AnalyzeRequest>())
            .Returns(new AnalysisResult
            {
                Success = true,
                Response = CreateAnalyzeResponse()
            });

        using var client = factory.CreateClient();
        var request = new AnalyzeRequest
        {
            Market = "san-diego",
            Neighborhood = "Mission Beach",
            PurchasePrice = 900000m,
            DownPaymentPercent = 20m,
            InterestRate = 6.5m,
            LoanTermYears = 30,
            SelfManaged = true,
            HoaMonthly = 0m
        };

        var response = await client.PostAsJsonAsync("/api/v1/analyze", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.AnalysisService.Received(1).AnalyzeAsync(Arg.Is<AnalyzeRequest>(r =>
            r.Bedrooms is null &&
            r.Bathrooms is null));
    }

    [Fact]
    public async Task Markets_ReturnsRepositoryData()
    {
        using var factory = new TestApiFactory();
        factory.MarketRepository.GetMarketsAsync().Returns(
        [
            new MarketInfo("san-diego", "San Diego", 10, 250)
        ]);

        using var client = factory.CreateClient();
        var payload = await client.GetFromJsonAsync<MarketsResponse>("/api/v1/markets");

        Assert.NotNull(payload);
        Assert.Single(payload!.Markets);
        Assert.Equal("san-diego", payload.Markets.Single().Id);
    }

    [Fact]
    public async Task Neighborhoods_ReturnsRepositoryData()
    {
        using var factory = new TestApiFactory();
        factory.MarketRepository.GetNeighborhoodsAsync("san-diego").Returns(
        [
            new NeighborhoodInfo("Mission Beach", 42, 275m, 210m)
        ]);

        using var client = factory.CreateClient();
        var payload = await client.GetFromJsonAsync<NeighborhoodsResponse>("/api/v1/markets/san-diego/neighborhoods");

        Assert.NotNull(payload);
        Assert.Equal("san-diego", payload!.Market);
        Assert.Single(payload.Neighborhoods);
    }

    [Fact]
    public async Task Configurations_ReturnsRepositoryData()
    {
        using var factory = new TestApiFactory();
        factory.MarketRepository.GetConfigurationsAsync("san-diego", "Mission Beach").Returns(
        [
            new ConfigurationInfo(2, 1.5m, 12, true)
        ]);

        using var client = factory.CreateClient();
        var payload = await client.GetFromJsonAsync<ConfigurationsResponse>(
            "/api/v1/markets/san-diego/neighborhoods/Mission%20Beach/configurations");

        Assert.NotNull(payload);
        Assert.Equal("Mission Beach", payload!.Neighborhood);
        Assert.Single(payload.Configurations);
    }

    private static AnalyzeRequest CreateAnalyzeRequest() => new()
    {
        Market = "san-diego",
        Neighborhood = "Mission Beach",
        Bedrooms = 2,
        Bathrooms = 2m,
        PurchasePrice = 900000m,
        DownPaymentPercent = 20m,
        InterestRate = 6.5m,
        LoanTermYears = 30,
        SelfManaged = true,
        HoaMonthly = 0m
    };

    private static AnalyzeResponse CreateAnalyzeResponse() => new()
    {
        Summary = new SummarySection
        {
            Headline = "Strong investment potential",
            Recommendation = "buy",
            Confidence = "high"
        },
        Profile = new ProfileSection
        {
            Text = "Test profile",
            Source = "combo",
            GeneratedAt = new System.DateTime(2025, 1, 1)
        },
        Insights = new InsightsSection
        {
            SuccessFactors = ["Walkable"],
            RiskFactors = ["Seasonality"],
            PremiumAmenities = ["parking"],
            Source = "combo"
        },
        Metrics = new MetricsSection
        {
            CashOnCashReturn = 0.1m,
            CapRate = 0.08m,
            NetOperatingIncome = 25000m,
            AnnualCashFlow = 12000m,
            BreakEvenOccupancy = 0.55m,
            GrossYield = 0.12m
        },
        Revenue = new RevenueSection
        {
            NightlyRate = new RateInfo
            {
                Value = 275m,
                Percentile = 75,
                Range = new RangeInfo
                {
                    P25 = 200m,
                    P50 = 240m,
                    P75 = 300m
                }
            },
            OccupancyRate = new OccupancyInfo
            {
                Value = 0.72m,
                SeasonalRange = new SeasonalRange
                {
                    Low = 0.55m,
                    High = 0.89m
                }
            },
            GrossAnnualRevenue = 80000m,
            ComparablesCount = 50
        },
        Expenses = new ExpensesSection
        {
            AnnualTotal = 40000m,
            Monthly = 3333.33m,
            Breakdown = new Dictionary<string, ExpenseItem>
            {
                ["mortgage"] = new()
                {
                    Value = 25000m,
                    Monthly = false,
                    Source = "Test"
                }
            }
        },
        Metadata = new MetadataSection
        {
            AnalysisDate = new System.DateTime(2025, 1, 1),
            DataSources =
            [
                new DataSourceInfo
                {
                    Name = "Test Source",
                    Date = "2025-01-01",
                    Description = "Fixture"
                }
            ],
            Assumptions = ["Fixture"]
        }
    };

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        public IMarketRepository MarketRepository { get; } = Substitute.For<IMarketRepository>();
        public IAnalysisService AnalysisService { get; } = Substitute.For<IAnalysisService>();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMarketRepository>();
                services.RemoveAll<IAnalysisService>();

                services.AddSingleton(MarketRepository);
                services.AddSingleton(AnalysisService);
            });
        }
    }
}
