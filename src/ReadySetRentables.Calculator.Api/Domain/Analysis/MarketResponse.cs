namespace ReadySetRentables.Calculator.Api.Domain.Analysis;

/// <summary>
/// Response containing available markets.
/// </summary>
public sealed record MarketsResponse(List<MarketInfo> Markets);

/// <summary>
/// Information about a market.
/// </summary>
public sealed record MarketInfo(
    string Id,
    string Name,
    long NeighborhoodCount,
    long ListingCount
);

/// <summary>
/// Response containing neighborhoods for a market.
/// </summary>
public sealed record NeighborhoodsResponse(
    string Market,
    List<NeighborhoodInfo> Neighborhoods
);

/// <summary>
/// Information about a neighborhood.
/// </summary>
public sealed record NeighborhoodInfo(
    string Name,
    long ListingCount,
    decimal AvgPrice,
    decimal AvgOccupancy
);

/// <summary>
/// Response containing bed/bath configurations for a neighborhood.
/// </summary>
public sealed record ConfigurationsResponse(
    string Market,
    string Neighborhood,
    List<ConfigurationInfo> Configurations
);

/// <summary>
/// Information about a bedroom/bathroom configuration.
/// </summary>
public sealed record ConfigurationInfo(
    int Bedrooms,
    decimal Bathrooms,
    long ListingCount,
    bool HasInsights
);
