namespace ReadySetRentables.Calculator.Api.Data;

/// <summary>
/// Repository interface for neighborhood data access.
/// </summary>
public interface INeighborhoodRepository
{
    /// <summary>
    /// Gets insights for a specific neighborhood/bedroom/bathroom combination.
    /// </summary>
    Task<NeighborhoodInsight?> GetInsightsAsync(string neighborhood, int bedrooms, decimal bathrooms);

    /// <summary>
    /// Gets the narrative for a specific neighborhood.
    /// </summary>
    Task<NeighborhoodNarrative?> GetNarrativeAsync(int neighborhoodId);

    /// <summary>
    /// Gets estimated revenue metrics from listings data.
    /// </summary>
    Task<RevenueEstimate?> GetRevenueEstimateAsync(string neighborhood, int bedrooms, decimal bathrooms);

    /// <summary>
    /// Gets the list of supported neighborhood/bedroom/bathroom combinations.
    /// </summary>
    Task<IReadOnlyList<string>> GetSupportedCombinationsAsync();
}
