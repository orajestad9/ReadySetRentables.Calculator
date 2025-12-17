using ReadySetRentables.Calculator.Api.Domain.Analysis;

namespace ReadySetRentables.Calculator.Api.Logic;

/// <summary>
/// Service interface for property investment analysis.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes a property investment opportunity.
    /// </summary>
    /// <param name="request">The analysis request parameters.</param>
    /// <returns>Complete analysis results or null if no data available.</returns>
    Task<AnalysisResult> AnalyzeAsync(AnalyzeRequest request);
}

/// <summary>
/// Result of an analysis operation.
/// </summary>
public sealed record AnalysisResult
{
    public bool Success { get; init; }
    public AnalyzeResponse? Response { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string>? SupportedCombinations { get; init; }
}
