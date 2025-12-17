namespace ReadySetRentables.Calculator.Api.Domain.Analysis;

/// <summary>
/// Complete investment analysis response.
/// </summary>
public sealed record AnalyzeResponse
{
    public required SummarySection Summary { get; init; }
    public required ProfileSection Profile { get; init; }
    public required InsightsSection Insights { get; init; }
    public required MetricsSection Metrics { get; init; }
    public required RevenueSection Revenue { get; init; }
    public required ExpensesSection Expenses { get; init; }
    public required MetadataSection Metadata { get; init; }
}

/// <summary>
/// Summary of the investment analysis with recommendation.
/// </summary>
public sealed record SummarySection
{
    public required string Headline { get; init; }
    public required string Recommendation { get; init; }
    public required string Confidence { get; init; }
}

/// <summary>
/// AI-generated neighborhood/combo profile.
/// </summary>
public sealed record ProfileSection
{
    public required string Text { get; init; }
    public required string Source { get; init; }
    public required DateTime GeneratedAt { get; init; }
}

/// <summary>
/// Insights from neighborhood data analysis.
/// </summary>
public sealed record InsightsSection
{
    public required List<string> SuccessFactors { get; init; }
    public required List<string> RiskFactors { get; init; }
    public required List<string> PremiumAmenities { get; init; }
    public required string Source { get; init; }
}

/// <summary>
/// Calculated financial metrics.
/// </summary>
public sealed record MetricsSection
{
    public required decimal CashOnCashReturn { get; init; }
    public required decimal CapRate { get; init; }
    public required decimal NetOperatingIncome { get; init; }
    public required decimal AnnualCashFlow { get; init; }
    public required decimal BreakEvenOccupancy { get; init; }
    public required decimal GrossYield { get; init; }
}

/// <summary>
/// Revenue projections and estimates.
/// </summary>
public sealed record RevenueSection
{
    public required RateInfo NightlyRate { get; init; }
    public required OccupancyInfo OccupancyRate { get; init; }
    public required decimal GrossAnnualRevenue { get; init; }
    public required int ComparablesCount { get; init; }
}

/// <summary>
/// Nightly rate with percentile information.
/// </summary>
public sealed record RateInfo
{
    public required decimal Value { get; init; }
    public required int Percentile { get; init; }
    public required RangeInfo Range { get; init; }
}

/// <summary>
/// Price range percentiles.
/// </summary>
public sealed record RangeInfo
{
    public required decimal P25 { get; init; }
    public required decimal P50 { get; init; }
    public required decimal P75 { get; init; }
}

/// <summary>
/// Occupancy rate with seasonal range.
/// </summary>
public sealed record OccupancyInfo
{
    public required decimal Value { get; init; }
    public required SeasonalRange SeasonalRange { get; init; }
}

/// <summary>
/// Seasonal occupancy range.
/// </summary>
public sealed record SeasonalRange
{
    public required decimal Low { get; init; }
    public required decimal High { get; init; }
}

/// <summary>
/// Annual expense breakdown with sources.
/// </summary>
public sealed record ExpensesSection
{
    public required decimal AnnualTotal { get; init; }
    public required decimal Monthly { get; init; }
    public required Dictionary<string, ExpenseItem> Breakdown { get; init; }
}

/// <summary>
/// Individual expense item with source citation.
/// </summary>
public sealed record ExpenseItem
{
    public required decimal Value { get; init; }
    public required bool Monthly { get; init; }
    public required string Source { get; init; }
}

/// <summary>
/// Metadata about the analysis including data sources.
/// </summary>
public sealed record MetadataSection
{
    public required DateTime AnalysisDate { get; init; }
    public required List<DataSourceInfo> DataSources { get; init; }
    public required List<string> Assumptions { get; init; }
}

/// <summary>
/// Information about a data source used in the analysis.
/// </summary>
public sealed record DataSourceInfo
{
    public required string Name { get; init; }
    public required string Date { get; init; }
    public required string Description { get; init; }
}
