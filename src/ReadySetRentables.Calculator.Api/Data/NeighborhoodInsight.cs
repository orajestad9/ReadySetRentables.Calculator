namespace ReadySetRentables.Calculator.Api.Data;

/// <summary>
/// Database model for neighborhood_insights table.
/// </summary>
public sealed record NeighborhoodInsight
{
    public int NeighborhoodId { get; init; }
    public string Neighbourhood { get; init; } = string.Empty;
    public int Bedrooms { get; init; }
    public decimal Bathrooms { get; init; }
    public string? SuccessFactors { get; init; }
    public string? RiskFactors { get; init; }
    public string? PremiumAmenities { get; init; }
    public int ReviewCount { get; init; }
    public int ListingCount { get; init; }
}

/// <summary>
/// Database model for neighborhood_narratives table.
/// </summary>
public sealed record NeighborhoodNarrative
{
    public int NeighborhoodId { get; init; }
    public string NarrativeText { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
}

/// <summary>
/// Revenue estimates from listings data.
/// </summary>
public sealed record RevenueEstimate
{
    public decimal AvgRevenue { get; init; }
    public decimal AvgNightlyRate { get; init; }
    public decimal AvgOccupancy { get; init; }
}
