using ReadySetRentables.Calculator.Api.Domain.Analysis;

namespace ReadySetRentables.Calculator.Api.Data;

/// <summary>
/// Repository interface for market and neighborhood data access.
/// </summary>
public interface IMarketRepository
{
    /// <summary>
    /// Gets all available markets.
    /// </summary>
    Task<List<MarketInfo>> GetMarketsAsync();

    /// <summary>
    /// Gets neighborhoods for a specific market.
    /// </summary>
    Task<List<NeighborhoodInfo>> GetNeighborhoodsAsync(string market);

    /// <summary>
    /// Gets available bed/bath configurations for a neighborhood.
    /// </summary>
    Task<List<ConfigurationInfo>> GetConfigurationsAsync(string market, string neighborhood);

    /// <summary>
    /// Gets comprehensive neighborhood data for analysis including insights, profile, and metrics.
    /// </summary>
    Task<NeighborhoodData?> GetNeighborhoodDataAsync(string market, string neighborhood, int bedrooms, decimal bathrooms);

    /// <summary>
    /// Gets price and revenue percentiles from listings data.
    /// </summary>
    Task<PercentileData?> GetPercentilesAsync(string market, string neighborhood, int bedrooms);
}

/// <summary>
/// Comprehensive neighborhood data combining insights, profiles, and metrics.
/// </summary>
public sealed record NeighborhoodData
{
    public string? ComboProfile { get; init; }
    public string? NeighborhoodProfile { get; init; }
    public List<string> SuccessFactors { get; init; } = [];
    public List<string> RiskFactors { get; init; } = [];
    public List<string> PremiumAmenities { get; init; } = [];
    public int ReviewCount { get; init; }
    public DateTime? ComputedAt { get; init; }
    public DateTime? NeighborhoodGeneratedAt { get; init; }
    public decimal AvgRevenue { get; init; }
    public decimal AvgOccupancy { get; init; }
    public decimal AvgPrice { get; init; }
    public decimal AvgRating { get; init; }
    public int ListingCount { get; init; }
}

/// <summary>
/// Price and revenue percentile data from listings.
/// </summary>
public sealed record PercentileData
{
    public decimal RevenueP25 { get; init; }
    public decimal RevenueP50 { get; init; }
    public decimal RevenueP75 { get; init; }
    public decimal PriceP25 { get; init; }
    public decimal PriceP50 { get; init; }
    public decimal PriceP75 { get; init; }
    public int ComparablesCount { get; init; }
}
