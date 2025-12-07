using System.ComponentModel.DataAnnotations;

namespace ReadySetRentables.Calculator.Api.Domain;

/// <summary>
/// Input parameters for calculating short-term rental ROI metrics.
/// </summary>
public sealed record RentalInputs
{
    /// <summary>
    /// Average nightly rental price.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "NightlyRate cannot be negative.")]
    public decimal NightlyRate { get; init; }

    /// <summary>
    /// Expected number of booked nights per month (0-31).
    /// </summary>
    [Range(0, 31, ErrorMessage = "NightsBookedPerMonth must be between 0 and 31.")]
    public int NightsBookedPerMonth { get; init; }

    /// <summary>
    /// Cleaning fee earned per guest stay.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "CleaningFeePerStay cannot be negative.")]
    public decimal CleaningFeePerStay { get; init; }

    /// <summary>
    /// Number of guest stays per month.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "StaysPerMonth cannot be negative.")]
    public int StaysPerMonth { get; init; }

    /// <summary>
    /// Monthly fixed costs including mortgage, utilities, insurance, etc.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "MonthlyFixedCosts cannot be negative.")]
    public decimal MonthlyFixedCosts { get; init; }

    /// <summary>
    /// Property purchase price used for cap rate calculation.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "PurchasePrice must be greater than zero.")]
    public decimal PurchasePrice { get; init; }
}
