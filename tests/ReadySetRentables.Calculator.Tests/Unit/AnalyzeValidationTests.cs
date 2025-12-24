using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class AnalyzeValidationTests
{
    [Fact]
    public void AnalyzeRequest_ValidatesRequiredMarket()
    {
        var request = CreateValidRequest() with { Market = null! };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("Market"));
    }

    [Fact]
    public void AnalyzeRequest_ValidatesRequiredNeighborhood()
    {
        var request = CreateValidRequest() with { Neighborhood = null! };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("Neighborhood"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public void AnalyzeRequest_ValidatesBedroomsRange(int bedrooms)
    {
        var request = CreateValidRequest() with { Bedrooms = bedrooms };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("Bedrooms"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void AnalyzeRequest_AcceptsValidBedrooms(int bedrooms)
    {
        var request = CreateValidRequest() with { Bedrooms = bedrooms };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("Bedrooms"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.4)]
    [InlineData(11)]
    [InlineData(-1)]
    public void AnalyzeRequest_ValidatesBathroomsRange(decimal bathrooms)
    {
        var request = CreateValidRequest() with { Bathrooms = bathrooms };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("Bathrooms"));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(2.5)]
    [InlineData(10)]
    public void AnalyzeRequest_AcceptsValidBathrooms(decimal bathrooms)
    {
        var request = CreateValidRequest() with { Bathrooms = bathrooms };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("Bathrooms"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100000)]
    public void AnalyzeRequest_ValidatesPurchasePriceGreaterThanZero(decimal purchasePrice)
    {
        var request = CreateValidRequest() with { PurchasePrice = purchasePrice };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("PurchasePrice"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100000)]
    [InlineData(1000000)]
    public void AnalyzeRequest_AcceptsValidPurchasePrice(decimal purchasePrice)
    {
        var request = CreateValidRequest() with { PurchasePrice = purchasePrice };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("PurchasePrice"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(101)]
    [InlineData(200)]
    public void AnalyzeRequest_ValidatesDownPaymentPercentRange(decimal downPayment)
    {
        var request = CreateValidRequest() with { DownPaymentPercent = downPayment };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("DownPaymentPercent"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void AnalyzeRequest_AcceptsValidDownPaymentPercent(decimal downPayment)
    {
        var request = CreateValidRequest() with { DownPaymentPercent = downPayment };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("DownPaymentPercent"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    [InlineData(100)]
    public void AnalyzeRequest_ValidatesInterestRateRange(decimal interestRate)
    {
        var request = CreateValidRequest() with { InterestRate = interestRate };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("InterestRate"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(15)]
    [InlineData(30)]
    public void AnalyzeRequest_AcceptsValidInterestRate(decimal interestRate)
    {
        var request = CreateValidRequest() with { InterestRate = interestRate };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("InterestRate"));
    }

    [Fact]
    public void AnalyzeRequest_AcceptsNullInterestRate()
    {
        var request = CreateValidRequest() with { InterestRate = null };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("InterestRate"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(41)]
    [InlineData(100)]
    public void AnalyzeRequest_ValidatesLoanTermYearsRange(int loanTermYears)
    {
        var request = CreateValidRequest() with { LoanTermYears = loanTermYears };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("LoanTermYears"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(40)]
    public void AnalyzeRequest_AcceptsValidLoanTermYears(int loanTermYears)
    {
        var request = CreateValidRequest() with { LoanTermYears = loanTermYears };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("LoanTermYears"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AnalyzeRequest_ValidatesHoaMonthlyNonNegative(decimal hoaMonthly)
    {
        var request = CreateValidRequest() with { HoaMonthly = hoaMonthly };

        var results = ValidateRequest(request);

        Assert.Contains(results, r => r.MemberNames.Contains("HoaMonthly"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500)]
    public void AnalyzeRequest_AcceptsValidHoaMonthly(decimal hoaMonthly)
    {
        var request = CreateValidRequest() with { HoaMonthly = hoaMonthly };

        var results = ValidateRequest(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains("HoaMonthly"));
    }

    [Fact]
    public void AnalyzeRequest_ValidRequestPassesAllValidation()
    {
        var request = CreateValidRequest();

        var results = ValidateRequest(request);

        Assert.Empty(results);
    }

    private static AnalyzeRequest CreateValidRequest() => new()
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

    private static List<ValidationResult> ValidateRequest(AnalyzeRequest request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }
}
