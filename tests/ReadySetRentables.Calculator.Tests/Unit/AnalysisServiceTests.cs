using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReadySetRentables.Calculator.Api.Configuration;
using ReadySetRentables.Calculator.Api.Data;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using ReadySetRentables.Calculator.Api.Logic;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class AnalysisServiceTests
{
    private readonly IMarketRepository _repository;
    private readonly AnalysisService _service;
    private readonly AnalysisOptions _options;

    public AnalysisServiceTests()
    {
        _repository = Substitute.For<IMarketRepository>();
        _options = new AnalysisOptions();
        var optionsWrapper = Options.Create(_options);
        _service = new AnalysisService(_repository, optionsWrapper);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsFailure_WhenNoDataFound()
    {
        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(null));

        var request = CreateRequest();
        var result = await _service.AnalyzeAsync(request);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("No data available", result.ErrorMessage);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsSuccess_WhenDataFound()
    {
        var data = CreateNeighborhoodData();
        var percentiles = CreatePercentileData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(percentiles));

        var request = CreateRequest();
        var result = await _service.AnalyzeAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Response);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesHeadlineAndRecommendation()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.NotNull(result.Response!.Summary.Headline);
        Assert.Contains("investment potential", result.Response.Summary.Headline);
        Assert.NotNull(result.Response.Summary.Recommendation);
        Assert.Contains(result.Response.Summary.Recommendation, new[] { "buy", "consider", "caution" });
    }

    [Fact]
    public async Task AnalyzeAsync_CalculatesMortgageCorrectly()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest();
        var result = await _service.AnalyzeAsync(request);

        Assert.True(result.Success);
        Assert.True(result.Response!.Expenses.Breakdown.ContainsKey("mortgage"));
        Assert.True(result.Response.Expenses.Breakdown["mortgage"].Value > 0);
    }

    [Fact]
    public async Task AnalyzeAsync_CalculatesPropertyTaxAsPercentageOfPurchasePrice()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest();
        var result = await _service.AnalyzeAsync(request);

        Assert.True(result.Success);
        var expectedAnnualTax = (request.PurchasePrice ?? 0m) * _options.PropertyTaxRate;
        Assert.Equal(
            Math.Round(expectedAnnualTax, 2),
            result.Response!.Expenses.Breakdown["propertyTax"].Value);
    }

    [Fact]
    public async Task AnalyzeAsync_DeterminesHighConfidence_WhenSufficientListings()
    {
        var data = CreateNeighborhoodData() with { ListingCount = 100 };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal("high", result.Response!.Summary.Confidence);
    }

    [Fact]
    public async Task AnalyzeAsync_DeterminesMediumConfidence_WhenModerateListings()
    {
        var data = CreateNeighborhoodData() with { ListingCount = 30 };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal("medium", result.Response!.Summary.Confidence);
    }

    [Fact]
    public async Task AnalyzeAsync_DeterminesLowConfidence_WhenFewListings()
    {
        var data = CreateNeighborhoodData() with { ListingCount = 10 };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal("low", result.Response!.Summary.Confidence);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesSuccessFactors()
    {
        var data = CreateNeighborhoodData() with
        {
            SuccessFactors = ["Walking distance to beach", "Ocean views"]
        };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal(2, result.Response!.Insights.SuccessFactors.Count);
        Assert.Contains("Walking distance to beach", result.Response.Insights.SuccessFactors);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsEmptyLists_WhenNoInsights()
    {
        var data = CreateNeighborhoodData() with
        {
            SuccessFactors = [],
            RiskFactors = [],
            PremiumAmenities = []
        };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Empty(result.Response!.Insights.SuccessFactors);
        Assert.Empty(result.Response.Insights.RiskFactors);
        Assert.Empty(result.Response.Insights.PremiumAmenities);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesComboProfile_WhenAvailable()
    {
        var data = CreateNeighborhoodData() with
        {
            ComboProfile = "This is the combo-specific profile.",
            NeighborhoodProfile = "This is the neighborhood fallback."
        };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal("This is the combo-specific profile.", result.Response!.Profile.Text);
        Assert.Equal("combo", result.Response.Profile.Source);
    }

    [Fact]
    public async Task AnalyzeAsync_FallsBackToNeighborhoodProfile_WhenNoComboProfile()
    {
        var data = CreateNeighborhoodData() with
        {
            ComboProfile = null,
            NeighborhoodProfile = "This is the neighborhood fallback."
        };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal("This is the neighborhood fallback.", result.Response!.Profile.Text);
        Assert.Equal("neighborhood_fallback", result.Response.Profile.Source);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesNeighborhoodFallback_WhenNoComboInsightsExist()
    {
        // Simulates the repository-level fallback: no neighborhood_insights row exists,
        // so the repository returns data from neighborhood_profiles only (no combo-level insights)
        var data = CreateNeighborhoodData() with
        {
            ComboProfile = null,
            NeighborhoodProfile = "Neighborhood-level fallback profile.",
            SuccessFactors = [],
            RiskFactors = [],
            PremiumAmenities = [],
            ReviewCount = 0,
            ComputedAt = null
        };

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.True(result.Success);
        Assert.Equal("Neighborhood-level fallback profile.", result.Response!.Profile.Text);
        Assert.Equal("neighborhood_fallback", result.Response.Profile.Source);
        Assert.Empty(result.Response.Insights.SuccessFactors);
        Assert.Empty(result.Response.Insights.RiskFactors);
        Assert.Empty(result.Response.Insights.PremiumAmenities);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesExpenseBreakdownWithSources()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        var breakdown = result.Response!.Expenses.Breakdown;

        Assert.Contains("mortgage", breakdown.Keys);
        Assert.Contains("propertyTax", breakdown.Keys);
        Assert.Contains("insurance", breakdown.Keys);
        Assert.Contains("hoa", breakdown.Keys);
        Assert.Contains("utilities", breakdown.Keys);
        Assert.Contains("cleaning", breakdown.Keys);
        Assert.Contains("platformFees", breakdown.Keys);
        Assert.Contains("maintenance", breakdown.Keys);
        Assert.Contains("totTax", breakdown.Keys);
        Assert.Contains("strPermit", breakdown.Keys);
        Assert.Contains("propertyManagement", breakdown.Keys);

        // Each expense should have a source
        foreach (var expense in breakdown.Values)
        {
            Assert.NotNull(expense.Source);
            Assert.NotEmpty(expense.Source);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_SelfManaged_NoPropertyManagementFee()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest() with { SelfManaged = true };
        var result = await _service.AnalyzeAsync(request);

        Assert.Equal(0m, result.Response!.Expenses.Breakdown["propertyManagement"].Value);
        Assert.Contains("Self-managed", result.Response.Expenses.Breakdown["propertyManagement"].Source);
    }

    [Fact]
    public async Task AnalyzeAsync_NotSelfManaged_IncludesPropertyManagementFee()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest() with { SelfManaged = false };
        var result = await _service.AnalyzeAsync(request);

        Assert.True(result.Response!.Expenses.Breakdown["propertyManagement"].Value > 0);
        Assert.Contains("20%", result.Response.Expenses.Breakdown["propertyManagement"].Source);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesHoaFromRequest()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest() with { HoaMonthly = 400m };
        var result = await _service.AnalyzeAsync(request);

        Assert.Equal(4800m, result.Response!.Expenses.Breakdown["hoa"].Value); // 400 * 12
        Assert.Contains("$400/month", result.Response.Expenses.Breakdown["hoa"].Source);
    }

    [Fact]
    public async Task AnalyzeAsync_UsesDefaultInterestRate_WhenNotProvided()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest() with { InterestRate = null };
        var result = await _service.AnalyzeAsync(request);

        Assert.Contains($"{_options.DefaultInterestRate}%", result.Response!.Expenses.Breakdown["mortgage"].Source);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesMetadataWithDataSources()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.NotEmpty(result.Response!.Metadata.DataSources);
        Assert.Contains(result.Response.Metadata.DataSources, ds => ds.Name == "Inside Airbnb");
        Assert.Contains(result.Response.Metadata.DataSources, ds => ds.Name == "Freddie Mac PMMS");
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesAssumptions()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var request = CreateRequest();
        var result = await _service.AnalyzeAsync(request);

        Assert.NotEmpty(result.Response!.Metadata.Assumptions);
        Assert.Contains(result.Response.Metadata.Assumptions, a => a.Contains("down payment"));
        Assert.Contains(result.Response.Metadata.Assumptions, a => a.Contains("mortgage"));
    }

    [Fact]
    public async Task AnalyzeAsync_CalculatesMetricsCorrectly()
    {
        var data = CreateNeighborhoodData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(null));

        var result = await _service.AnalyzeAsync(CreateRequest());

        var metrics = result.Response!.Metrics;

        // Metrics should be decimal values (not percentages)
        Assert.True(metrics.CashOnCashReturn >= -1m && metrics.CashOnCashReturn <= 1m);
        Assert.True(metrics.CapRate >= -1m && metrics.CapRate <= 1m);
        Assert.True(metrics.GrossYield >= 0m && metrics.GrossYield <= 1m);
    }

    [Fact]
    public async Task AnalyzeAsync_IncludesRevenueWithPercentiles()
    {
        var data = CreateNeighborhoodData();
        var percentiles = CreatePercentileData();

        _repository.GetNeighborhoodDataAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal?>())
            .Returns(Task.FromResult<NeighborhoodData?>(data));
        _repository.GetPercentilesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult<PercentileData?>(percentiles));

        var result = await _service.AnalyzeAsync(CreateRequest());

        Assert.Equal(percentiles.PriceP25, result.Response!.Revenue.NightlyRate.Range.P25);
        Assert.Equal(percentiles.PriceP50, result.Response.Revenue.NightlyRate.Range.P50);
        Assert.Equal(percentiles.PriceP75, result.Response.Revenue.NightlyRate.Range.P75);
    }

    private static AnalyzeRequest CreateRequest() => new()
    {
        Market = "san-diego",
        Neighborhood = "Mission Bay",
        Bedrooms = 2,
        Bathrooms = 2m,
        PurchasePrice = 850000m,
        DownPaymentPercent = 20m,
        InterestRate = 7m,
        LoanTermYears = 30,
        SelfManaged = true,
        HoaMonthly = 0m
    };

    private static AnalyzeRequest CreateRequestWithoutOptionalFields() => new()
    {
        Market = "san-diego",
        Neighborhood = "Mission Bay",
        Bedrooms = 2,
        DownPaymentPercent = 20m,
        InterestRate = 7m,
        LoanTermYears = 30,
        SelfManaged = true,
        HoaMonthly = 0m
    };

    private static NeighborhoodData CreateNeighborhoodData() => new()
    {
        ComboProfile = "Test combo profile.",
        NeighborhoodProfile = "Test neighborhood profile.",
        SuccessFactors = ["Great location", "Beach access"],
        RiskFactors = ["Parking issues"],
        PremiumAmenities = ["ocean_view", "parking"],
        ReviewCount = 1000,
        ComputedAt = new DateTime(2024, 12, 15),
        NeighborhoodGeneratedAt = new DateTime(2024, 12, 10),
        AvgRevenue = 65000m,
        AvgOccupancy = 265m, // days per year
        AvgPrice = 225m,
        AvgRating = 4.8m,
        ListingCount = 50
    };

    private static PercentileData CreatePercentileData() => new()
    {
        RevenueP25 = 45000m,
        RevenueP50 = 65000m,
        RevenueP75 = 85000m,
        PriceP25 = 175m,
        PriceP50 = 225m,
        PriceP75 = 300m,
        ComparablesCount = 150
    };
}
