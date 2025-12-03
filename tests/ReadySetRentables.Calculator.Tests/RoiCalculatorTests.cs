using ReadySetRentables.Calculator.Api.Domain;
using ReadySetRentables.Calculator.Api.Logic;
using System;
using Xunit;

namespace ReadySetRentables.Calculator.Tests
{
    public class RoiCalculatorTests
    {
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

            var result = RoiCalculator.Calculate(input);

            var expectedMonthlyRevenue = 150m * 20 + 80m * 10;
            var expectedMonthlyCosts = 2500m;
            var expectedMonthlyProfit = expectedMonthlyRevenue - expectedMonthlyCosts;
            var expectedAnnualProfit = expectedMonthlyProfit * 12;
            var expectedCapRate = expectedAnnualProfit / 400000m * 100m;

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

            var ex = Assert.Throws<ArgumentException>(() => RoiCalculator.Calculate(input));
            Assert.Contains("PurchasePrice", ex.Message);
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(100, -1)]
        public void Calculate_Throws_OnNegativeValues(decimal nightlyRate, int nightsBooked)
        {
            var input = new RentalInputs
            {
                NightlyRate = nightlyRate,
                NightsBookedPerMonth = nightsBooked,
                CleaningFeePerStay = 50m,
                StaysPerMonth = 5,
                MonthlyFixedCosts = 1000m,
                PurchasePrice = 300000m
            };

            Assert.Throws<ArgumentException>(() => RoiCalculator.Calculate(input));
        }
    }
}
