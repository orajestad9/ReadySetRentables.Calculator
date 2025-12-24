namespace ReadySetRentables.Calculator.Api.Configuration;

/// <summary>
/// Configuration options for property investment analysis calculations.
/// </summary>
public sealed class AnalysisOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Analysis";

    /// <summary>
    /// Default annual interest rate as a percentage (e.g., 6.89 for 6.89%).
    /// Used when no rate is provided in the request. Based on Freddie Mac PMMS.
    /// </summary>
    public decimal DefaultInterestRate { get; init; } = 6.89m;

    /// <summary>
    /// Property tax rate as a decimal (e.g., 0.0125 for 1.25%).
    /// Based on San Diego County rates.
    /// </summary>
    public decimal PropertyTaxRate { get; init; } = 0.0125m;

    /// <summary>
    /// Annual insurance estimate in USD for STR properties.
    /// </summary>
    public decimal AnnualInsurance { get; init; } = 2400m;

    /// <summary>
    /// Annual utilities estimate in USD.
    /// </summary>
    public decimal AnnualUtilities { get; init; } = 3000m;

    /// <summary>
    /// Cost per cleaning turn in USD.
    /// </summary>
    public decimal CleaningCostPerTurn { get; init; } = 60m;

    /// <summary>
    /// Default estimated turns per year when average price is not available.
    /// </summary>
    public decimal DefaultEstimatedTurns { get; init; } = 80m;

    /// <summary>
    /// Platform fee rate as a decimal (e.g., 0.03 for 3% Airbnb host-only fee).
    /// </summary>
    public decimal PlatformFeeRate { get; init; } = 0.03m;

    /// <summary>
    /// Maintenance rate as a decimal of gross revenue (e.g., 0.02 for 2%).
    /// Based on VRMA benchmark.
    /// </summary>
    public decimal MaintenanceRate { get; init; } = 0.02m;

    /// <summary>
    /// Transient Occupancy Tax rate as a decimal (e.g., 0.105 for 10.5%).
    /// Based on San Diego Municipal Code 35.0103.
    /// </summary>
    public decimal TotTaxRate { get; init; } = 0.105m;

    /// <summary>
    /// Annual STR permit fee in USD.
    /// Based on San Diego STRO annual renewal.
    /// </summary>
    public decimal StrPermitFee { get; init; } = 125m;

    /// <summary>
    /// Property management fee rate as a decimal of gross revenue (e.g., 0.20 for 20%).
    /// </summary>
    public decimal PropertyManagementRate { get; init; } = 0.20m;

    /// <summary>
    /// Seasonal occupancy low estimate as a decimal.
    /// </summary>
    public decimal SeasonalOccupancyLow { get; init; } = 0.55m;

    /// <summary>
    /// Seasonal occupancy high estimate as a decimal.
    /// </summary>
    public decimal SeasonalOccupancyHigh { get; init; } = 0.89m;

    /// <summary>
    /// Cash-on-cash return threshold for "buy" recommendation as a decimal.
    /// </summary>
    public decimal BuyThreshold { get; init; } = 0.08m;

    /// <summary>
    /// Cash-on-cash return threshold for "consider" recommendation as a decimal.
    /// Below this is "caution".
    /// </summary>
    public decimal ConsiderThreshold { get; init; } = 0.05m;

    /// <summary>
    /// Cash-on-cash return threshold for "Strong" vs "Moderate" headline.
    /// </summary>
    public decimal StrongInvestmentThreshold { get; init; } = 0.06m;

    /// <summary>
    /// Listing count threshold for "high" confidence.
    /// </summary>
    public int HighConfidenceListingCount { get; init; } = 50;

    /// <summary>
    /// Listing count threshold for "medium" confidence.
    /// Below this is "low".
    /// </summary>
    public int MediumConfidenceListingCount { get; init; } = 20;
}
