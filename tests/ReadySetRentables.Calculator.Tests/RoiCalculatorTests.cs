using System;
using ReadySetRentables.Calculator.Api.Domain;
using ReadySetRentables.Calculator.Api.Logic;
using Xunit;

namespace ReadySetRentables.Calculator.Tests;

public class RoiCalculatorTests
{
    private readonly IRoiCalculator _calculator = new RoiCalculator();

    [Fact]
    public void Calculate_ReturnsExpectedValues_ForTypicalCase()
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

        var result = _calculator.Calculate(input);

        var expectedMonthlyRevenue = 150m * 20 + 80m * 10;
        var expectedMonthlyCosts = 2500m;
        var expectedMonthlyProfit = expectedMonthlyRevenue - expectedMonthlyCosts;
        var expectedAnnualProfit = expectedMonthlyProfit * 12;
        var expectedCapRate = Math.Round(expectedAnnualProfit / 400000m * 100m, 2);

        Assert.Equal(expectedMonthlyRevenue, result.MonthlyRevenue);
        Assert.Equal(expectedMonthlyCosts, result.MonthlyCosts);
        Assert.Equal(expectedMonthlyProfit, result.MonthlyProfit);
        Assert.Equal(expectedAnnualProfit, result.AnnualProfit);
        Assert.Equal(expectedCapRate, result.CapRatePercent);
    }

    [Fact]
    public void Calculate_Throws_WhenPurchasePriceIsZero()
    {
        var input = new RentalInputs
        {
            NightlyRate = 100m,
            NightsBookedPerMonth = 10,
            CleaningFeePerStay = 50m,
            StaysPerMonth = 5,
            MonthlyFixedCosts = 1000m,
            PurchasePrice = 0m
        };

        var ex = Assert.Throws<ArgumentException>(() => _calculator.Calculate(input));
        Assert.Contains("PurchasePrice", ex.Message);
    }

    [Theory]
    [InlineData(-1, 10, 50, 5, 1000, 300000, "NightlyRate")]
    [InlineData(100, -1, 50, 5, 1000, 300000, "NightsBookedPerMonth")]
    [InlineData(100, 10, -50, 5, 1000, 300000, "CleaningFeePerStay")]
    [InlineData(100, 10, 50, -5, 1000, 300000, "StaysPerMonth")]
    [InlineData(100, 10, 50, 5, -1000, 300000, "MonthlyFixedCosts")]
    [InlineData(100, 10, 50, 5, 1000, -300000, "PurchasePrice")]
    public void Calculate_Throws_OnNegativeValues(
        decimal nightlyRate,
        int nightsBooked,
        decimal cleaningFee,
        int stays,
        decimal monthlyCosts,
        decimal purchasePrice,
        string expectedFieldInMessage)
    {
        var input = new RentalInputs
        {
            NightlyRate = nightlyRate,
            NightsBookedPerMonth = nightsBooked,
            CleaningFeePerStay = cleaningFee,
            StaysPerMonth = stays,
            MonthlyFixedCosts = monthlyCosts,
            PurchasePrice = purchasePrice
        };

        var ex = Assert.Throws<ArgumentException>(() => _calculator.Calculate(input));
        Assert.Contains(expectedFieldInMessage, ex.Message);
    }

    [Fact]
    public void Calculate_HandlesZeroRevenue()
    {
        var input = new RentalInputs
        {
            NightlyRate = 0m,
            NightsBookedPerMonth = 0,
            CleaningFeePerStay = 0m,
            StaysPerMonth = 0,
            MonthlyFixedCosts = 1000m,
            PurchasePrice = 300000m
        };

        var result = _calculator.Calculate(input);

        Assert.Equal(0m, result.MonthlyRevenue);
        Assert.Equal(1000m, result.MonthlyCosts);
        Assert.Equal(-1000m, result.MonthlyProfit);
        Assert.Equal(-12000m, result.AnnualProfit);
        Assert.Equal(-4m, result.CapRatePercent);
    }

    [Fact]
    public void Calculate_RoundsCapRateToTwoDecimals()
    {
        var input = new RentalInputs
        {
            NightlyRate = 100m,
            NightsBookedPerMonth = 10,
            CleaningFeePerStay = 33.33m,
            StaysPerMonth = 3,
            MonthlyFixedCosts = 500m,
            PurchasePrice = 333333m
        };

        var result = _calculator.Calculate(input);

        // Verify cap rate has at most 2 decimal places
        var capRateString = result.CapRatePercent.ToString();
        var decimalIndex = capRateString.IndexOf('.');
        if (decimalIndex >= 0)
        {
            var decimalPlaces = capRateString.Length - decimalIndex - 1;
            Assert.True(decimalPlaces <= 2, $"Cap rate {result.CapRatePercent} has more than 2 decimal places");
        }
    }
}
