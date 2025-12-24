using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using ReadySetRentables.Calculator.Api.Domain.Analysis;

namespace ReadySetRentables.Calculator.Api.Data;

/// <summary>
/// Repository for accessing market and neighborhood data from PostgreSQL.
/// </summary>
public sealed class MarketRepository : IMarketRepository
{
    private readonly string _connectionString;
    private readonly ILogger<MarketRepository> _logger;

    public MarketRepository(IConfiguration configuration, ILogger<MarketRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
        _logger = logger;
    }

    public async Task<List<MarketInfo>> GetMarketsAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                market AS Id,
                INITCAP(REPLACE(market, '-', ' ')) AS Name,
                COUNT(DISTINCT neighbourhood) AS NeighborhoodCount,
                COALESCE(SUM(listing_count), 0) AS ListingCount
            FROM neighborhood_metrics
            WHERE market IS NOT NULL
            GROUP BY market
            ORDER BY SUM(listing_count) DESC
            """;

        var results = await connection.QueryAsync<MarketInfo>(sql);
        return results.ToList();
    }

    public async Task<List<NeighborhoodInfo>> GetNeighborhoodsAsync(string market)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                neighbourhood AS Name,
                COALESCE(SUM(listing_count), 0) AS ListingCount,
                COALESCE(NULLIF(ROUND(SUM(avg_price * listing_count) / NULLIF(SUM(listing_count), 0), 2), 'NaN'), 0) AS AvgPrice,
                COALESCE(NULLIF(ROUND(SUM(avg_occupancy * listing_count) / NULLIF(SUM(listing_count), 0), 1), 'NaN'), 0) AS AvgOccupancy
            FROM neighborhood_metrics
            WHERE market = @Market
                AND room_type = 'Entire home/apt'
                AND property_type LIKE 'Entire%'
            GROUP BY neighbourhood
            ORDER BY SUM(listing_count) DESC
            """;

        var results = await connection.QueryAsync<NeighborhoodInfo>(sql, new { Market = market });
        return results.ToList();
    }

    public async Task<List<ConfigurationInfo>> GetConfigurationsAsync(string market, string neighborhood)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                ni.bedrooms AS Bedrooms,
                ni.bathrooms AS Bathrooms,
                COALESCE(nm.listing_count, 0) AS ListingCount,
                (ni.profile IS NOT NULL) AS HasInsights
            FROM neighborhood_insights ni
            LEFT JOIN (
                SELECT neighbourhood, bedrooms, SUM(listing_count) AS listing_count
                FROM neighborhood_metrics
                WHERE market = @Market AND room_type = 'Entire home/apt' AND property_type LIKE 'Entire%'
                GROUP BY neighbourhood, bedrooms
            ) nm ON ni.neighbourhood = nm.neighbourhood AND ni.bedrooms = nm.bedrooms
            WHERE ni.market = @Market AND ni.neighbourhood = @Neighborhood
            ORDER BY ni.bedrooms, ni.bathrooms
            """;

        var results = await connection.QueryAsync<ConfigurationInfo>(sql, new { Market = market, Neighborhood = neighborhood });
        return results.ToList();
    }

    public async Task<NeighborhoodData?> GetNeighborhoodDataAsync(string market, string neighborhood, int bedrooms, decimal bathrooms)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        // First try to get combo-level data from neighborhood_insights
        const string comboSql = """
            SELECT
                ni.profile AS ComboProfile,
                np.profile AS NeighborhoodProfile,
                ni.success_factors::text AS SuccessFactorsJson,
                ni.risk_factors::text AS RiskFactorsJson,
                ni.premium_amenities::text AS PremiumAmenitiesJson,
                COALESCE(ni.review_count, 0) AS ReviewCount,
                ni.computed_at AS ComputedAt,
                np.generated_at AS NeighborhoodGeneratedAt,
                NULLIF(ROUND(SUM(nm.avg_revenue * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgRevenue,
                NULLIF(ROUND(SUM(nm.avg_occupancy * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgOccupancy,
                NULLIF(ROUND(SUM(nm.avg_price * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgPrice,
                NULLIF(ROUND(SUM(nm.avg_rating * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgRating,
                COALESCE(SUM(nm.listing_count), 0) AS ListingCount
            FROM neighborhood_insights ni
            LEFT JOIN neighborhood_profiles np
                ON ni.neighbourhood = np.neighbourhood AND ni.market = np.market
            LEFT JOIN neighborhood_metrics nm
                ON ni.neighbourhood = nm.neighbourhood
                AND ni.bedrooms = nm.bedrooms
                AND ni.market = nm.market
                AND nm.room_type = 'Entire home/apt'
                AND nm.property_type LIKE 'Entire%'
            WHERE ni.market = @Market
                AND ni.neighbourhood = @Neighborhood
                AND ni.bedrooms = @Bedrooms
                AND ni.bathrooms = @Bathrooms
            GROUP BY ni.profile, ni.success_factors, ni.risk_factors, ni.premium_amenities,
                     ni.review_count, ni.computed_at, np.profile, np.generated_at
            """;

        // Fallback: get neighborhood-level profile when no combo exists
        const string fallbackSql = """
            SELECT
                NULL AS ComboProfile,
                np.profile AS NeighborhoodProfile,
                NULL AS SuccessFactorsJson,
                NULL AS RiskFactorsJson,
                NULL AS PremiumAmenitiesJson,
                0 AS ReviewCount,
                NULL AS ComputedAt,
                np.generated_at AS NeighborhoodGeneratedAt,
                NULLIF(ROUND(SUM(nm.avg_revenue * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgRevenue,
                NULLIF(ROUND(SUM(nm.avg_occupancy * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgOccupancy,
                NULLIF(ROUND(SUM(nm.avg_price * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgPrice,
                NULLIF(ROUND(SUM(nm.avg_rating * nm.listing_count) / NULLIF(SUM(nm.listing_count), 0), 2), 'NaN') AS AvgRating,
                COALESCE(SUM(nm.listing_count), 0) AS ListingCount
            FROM neighborhood_profiles np
            LEFT JOIN neighborhood_metrics nm
                ON np.neighbourhood = nm.neighbourhood
                AND np.market = nm.market
                AND nm.bedrooms = @Bedrooms
                AND nm.room_type = 'Entire home/apt'                
            WHERE np.market = @Market
                AND np.neighbourhood = @Neighborhood
            GROUP BY np.profile, np.generated_at
            """;

        var result = await connection.QueryFirstOrDefaultAsync<NeighborhoodDataRaw>(comboSql, new
        {
            Market = market,
            Neighborhood = neighborhood,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms
        });

        // If no combo-level data, fall back to neighborhood-level profile
        result ??= await connection.QueryFirstOrDefaultAsync<NeighborhoodDataRaw>(fallbackSql, new
        {
            Market = market,
            Neighborhood = neighborhood,
            Bedrooms = bedrooms
        });

        if (result == null) return null;

        return new NeighborhoodData
        {
            ComboProfile = result.ComboProfile,
            NeighborhoodProfile = result.NeighborhoodProfile,
            SuccessFactors = ParseJsonArray(result.SuccessFactorsJson),
            RiskFactors = ParseJsonArray(result.RiskFactorsJson),
            PremiumAmenities = ParseJsonArray(result.PremiumAmenitiesJson),
            ReviewCount = result.ReviewCount,
            ComputedAt = result.ComputedAt,
            NeighborhoodGeneratedAt = result.NeighborhoodGeneratedAt,
            AvgRevenue = result.AvgRevenue ?? 0m,
            AvgOccupancy = result.AvgOccupancy ?? 0m,
            AvgPrice = result.AvgPrice ?? 0m,
            AvgRating = result.AvgRating ?? 0m,
            ListingCount = result.ListingCount
        };
    }

    public async Task<PercentileData?> GetPercentilesAsync(string market, string neighborhood, int bedrooms)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        // Note: PostgreSQL lowercases unquoted aliases. Use double quotes to preserve
        // PascalCase for Dapper mapping to PercentileData record properties.
        const string sql = """
            SELECT
                ROUND(percentile_cont(0.25) WITHIN GROUP (ORDER BY estimated_revenue_l365d)::numeric, 2) AS "RevenueP25",
                ROUND(percentile_cont(0.50) WITHIN GROUP (ORDER BY estimated_revenue_l365d)::numeric, 2) AS "RevenueP50",
                ROUND(percentile_cont(0.75) WITHIN GROUP (ORDER BY estimated_revenue_l365d)::numeric, 2) AS "RevenueP75",
                ROUND(percentile_cont(0.25) WITHIN GROUP (ORDER BY price)::numeric, 2) AS "PriceP25",
                ROUND(percentile_cont(0.50) WITHIN GROUP (ORDER BY price)::numeric, 2) AS "PriceP50",
                ROUND(percentile_cont(0.75) WITHIN GROUP (ORDER BY price)::numeric, 2) AS "PriceP75",
                COUNT(*) AS "ComparablesCount"
            FROM listings
            WHERE market = @Market
                AND neighbourhood = @Neighborhood
                AND bedrooms = @Bedrooms
                AND room_type = 'Entire home/apt'
                AND estimated_revenue_l365d IS NOT NULL
            """;

        return await connection.QueryFirstOrDefaultAsync<PercentileData>(sql, new
        {
            Market = market,
            Neighborhood = neighborhood,
            Bedrooms = bedrooms
        });
    }

    private List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON array: {JsonPreview}",
                json.Length > 100 ? json[..100] + "..." : json);
            return [];
        }
    }

    /// <summary>
    /// Raw database result for neighborhood data query (before JSON parsing).
    /// </summary>
    private sealed record NeighborhoodDataRaw
    {
        public string? ComboProfile { get; init; }
        public string? NeighborhoodProfile { get; init; }
        public string? SuccessFactorsJson { get; init; }
        public string? RiskFactorsJson { get; init; }
        public string? PremiumAmenitiesJson { get; init; }
        public int ReviewCount { get; init; }
        public DateTime? ComputedAt { get; init; }
        public DateTime? NeighborhoodGeneratedAt { get; init; }
        public decimal? AvgRevenue { get; init; }
        public decimal? AvgOccupancy { get; init; }
        public decimal? AvgPrice { get; init; }
        public decimal? AvgRating { get; init; }
        public int ListingCount { get; init; }
    }
}
