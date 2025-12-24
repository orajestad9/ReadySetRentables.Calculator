using Dapper;
using Npgsql;

namespace ReadySetRentables.Calculator.Api.Data;

/// <summary>
/// Repository for accessing neighborhood data from PostgreSQL.
/// </summary>
public sealed class NeighborhoodRepository : INeighborhoodRepository
{
    private readonly string _connectionString;

    public NeighborhoodRepository(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
    }

    public async Task<NeighborhoodInsight?> GetInsightsAsync(string neighborhood, int bedrooms, decimal bathrooms)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                neighborhood_id AS NeighborhoodId,
                neighbourhood AS Neighbourhood,
                bedrooms AS Bedrooms,
                bathrooms AS Bathrooms,
                success_factors::text AS SuccessFactors,
                risk_factors::text AS RiskFactors,
                premium_amenities::text AS PremiumAmenities,
                review_count AS ReviewCount,
                listing_count AS ListingCount
            FROM neighborhood_insights
            WHERE LOWER(neighbourhood) = LOWER(@Neighborhood)
              AND bedrooms = @Bedrooms
              AND bathrooms = @Bathrooms
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<NeighborhoodInsight>(sql, new
        {
            Neighborhood = neighborhood,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms
        });
    }

    public async Task<NeighborhoodNarrative?> GetNarrativeAsync(int neighborhoodId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                neighborhood_id AS NeighborhoodId,
                narrative_text AS NarrativeText,
                generated_at AS GeneratedAt
            FROM neighborhood_narratives
            WHERE neighborhood_id = @NeighborhoodId
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<NeighborhoodNarrative>(sql, new
        {
            NeighborhoodId = neighborhoodId
        });
    }

    public async Task<RevenueEstimate?> GetRevenueEstimateAsync(string neighborhood, int bedrooms, decimal bathrooms)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT
                AVG(estimated_revenue_l365d) AS AvgRevenue,
                AVG(price) AS AvgNightlyRate,
                AVG(estimated_occupancy_l365d) AS AvgOccupancy
            FROM listings
            WHERE LOWER(neighbourhood) = LOWER(@Neighborhood)
              AND bedrooms = @Bedrooms
              AND bathrooms = @Bathrooms
            HAVING COUNT(*) > 0
            """;

        return await connection.QuerySingleOrDefaultAsync<RevenueEstimate>(sql, new
        {
            Neighborhood = neighborhood,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms
        });
    }

    public async Task<IReadOnlyList<string>> GetSupportedCombinationsAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT DISTINCT
                neighbourhood || ' (' || bedrooms || 'BR/' || bathrooms || 'BA)' AS Combination
            FROM neighborhood_insights
            ORDER BY Combination
            """;

        var results = await connection.QueryAsync<string>(sql);
        return results.ToList();
    }
}
