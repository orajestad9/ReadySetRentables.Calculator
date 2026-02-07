using System.ComponentModel.DataAnnotations;

namespace ReadySetRentables.Calculator.Api.Domain.Analysis;

/// <summary>
/// Request model for property investment analysis.
/// </summary>
public sealed record AnalyzeRequest
{
    /// <summary>
    /// The market identifier (e.g., "san-diego", "austin").
    /// </summary>
    [Required(ErrorMessage = "Market is required.")]
    public required string Market { get; init; }

    /// <summary>
    /// The neighborhood name to analyze.
    /// </summary>
    [Required(ErrorMessage = "Neighborhood is required.")]
    public required string Neighborhood { get; init; }

    /// <summary>
    /// Number of bedrooms (0-10).
    /// </summary>
    [Range(0, 10, ErrorMessage = "Bedrooms must be between 0 and 10.")]
    public int Bedrooms { get; init; }

    /// <summary>
    /// Number of bathrooms (can be decimal, e.g., 1.5).
    /// </summary>
    [Required(ErrorMessage = "Bathrooms is required.")]
    [Range(0.5, 10, ErrorMessage = "Bathrooms must be between 0.5 and 10.")]
    public decimal? Bathrooms { get; init; }

    /// <summary>
    /// Property purchase price in USD. Optional.
    /// </summary>
    public decimal? PurchasePrice { get; init; }

    /// <summary>
    /// Down payment percentage (0-100). Defaults to 20%.
    /// </summary>
    [Range(0, 100, ErrorMessage = "DownPaymentPercent must be between 0 and 100.")]
    public decimal DownPaymentPercent { get; init; } = 20;

    /// <summary>
    /// Annual interest rate as a percentage (e.g., 7.0 for 7%).
    /// If null, uses current Freddie Mac PMMS rate.
    /// </summary>
    [Range(0, 30, ErrorMessage = "InterestRate must be between 0 and 30.")]
    public decimal? InterestRate { get; init; }

    /// <summary>
    /// Loan term in years. Defaults to 30.
    /// </summary>
    [Range(1, 40, ErrorMessage = "LoanTermYears must be between 1 and 40.")]
    public int LoanTermYears { get; init; } = 30;

    /// <summary>
    /// Whether the property will be self-managed. Defaults to true.
    /// If false, 20% property management fee is applied.
    /// </summary>
    public bool SelfManaged { get; init; } = true;

    /// <summary>
    /// Monthly HOA fee in USD. Defaults to 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "HoaMonthly cannot be negative.")]
    public decimal HoaMonthly { get; init; } = 0;
}
