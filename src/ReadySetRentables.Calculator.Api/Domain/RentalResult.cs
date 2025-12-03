namespace ReadySetRentables.Calculator.Api.Domain
{
    public sealed class RentalResult
    {
        public decimal MonthlyRevenue { get; init; }
        public decimal MonthlyCosts { get; init; }
        public decimal MonthlyProfit { get; init; }
        public decimal AnnualProfit { get; init; }
        public decimal CapRatePercent { get; init; }
    }
}
