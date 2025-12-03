using ReadySetRentables.Calculator.Api.Domain;

namespace ReadySetRentables.Calculator.Api.Logic
{
    public static class RoiCalculator
    {
        public static RentalResult Calculate(RentalInputs input)
        {
            Validate(input);

            var monthlyRevenue =
                (input.NightlyRate * input.NightsBookedPerMonth) +
                (input.CleaningFeePerStay * input.StaysPerMonth);

            var monthlyCosts = input.MonthlyFixedCosts;
            var monthlyProfit = monthlyRevenue - monthlyCosts;
            var annualProfit = monthlyProfit * 12;

            var capRatePercent = annualProfit / input.PurchasePrice * 100m;

            return new RentalResult
            {
                MonthlyRevenue = monthlyRevenue,
                MonthlyCosts = monthlyCosts,
                MonthlyProfit = monthlyProfit,
                AnnualProfit = annualProfit,
                CapRatePercent = capRatePercent
            };
        }

        private static void Validate(RentalInputs input)
        {
            if (input.PurchasePrice <= 0)
            {
                throw new ArgumentException("PurchasePrice must be greater than zero.", nameof(input));
            }

            if (input.NightlyRate < 0)
            {
                throw new ArgumentException("NightlyRate cannot be negative.", nameof(input));
            }

            if (input.NightsBookedPerMonth < 0)
            {
                throw new ArgumentException("NightsBookedPerMonth cannot be negative.", nameof(input));
            }

            if (input.CleaningFeePerStay < 0)
            {
                throw new ArgumentException("CleaningFeePerStay cannot be negative.", nameof(input));
            }

            if (input.StaysPerMonth < 0)
            {
                throw new ArgumentException("StaysPerMonth cannot be negative.", nameof(input));
            }

            if (input.MonthlyFixedCosts < 0)
            {
                throw new ArgumentException("MonthlyFixedCosts cannot be negative.", nameof(input));
            }
        }
    }
}
