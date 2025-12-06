namespace ReadySetRentables.Calculator.Api.Domain;

/// <summary>
/// Calculated ROI metrics for a short-term rental property.
/// </summary>
/// <param name="MonthlyRevenue">Total monthly revenue from nightly rates and cleaning fees.</param>
/// <param name="MonthlyCosts">Monthly fixed costs (mortgage, utilities, insurance, etc.).</param>
/// <param name="MonthlyProfit">Net monthly profit (revenue minus costs).</param>
/// <param name="AnnualProfit">Projected annual profit (monthly profit × 12).</param>
/// <param name="CapRatePercent">Capitalization rate as a percentage (annual profit / purchase price × 100).</param>
public sealed record RentalResult(
    decimal MonthlyRevenue,
    decimal MonthlyCosts,
    decimal MonthlyProfit,
    decimal AnnualProfit,
    decimal CapRatePercent);
