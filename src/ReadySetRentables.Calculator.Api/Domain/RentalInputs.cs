namespace ReadySetRentables.Calculator.Api.Domain
{
    public sealed class RentalInputs
    {
        public decimal NightlyRate { get; init; }
        public int NightsBookedPerMonth { get; init; }
        public decimal CleaningFeePerStay { get; init; }
        public int StaysPerMonth { get; init; }
        public decimal MonthlyFixedCosts { get; init; }  // mortgage, utilities, etc.
        public decimal PurchasePrice { get; init; }      // for ROI / cap rate
    }
}
