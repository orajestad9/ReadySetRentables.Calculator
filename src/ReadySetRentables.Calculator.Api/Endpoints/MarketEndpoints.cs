using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReadySetRentables.Calculator.Api.Data;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using ReadySetRentables.Calculator.Api.Security;

namespace ReadySetRentables.Calculator.Api.Endpoints;

/// <summary>
/// Extension methods for mapping market API endpoints.
/// </summary>
public static class MarketEndpoints
{
    /// <summary>
    /// Maps the market endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapMarketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1")
            .RequireRateLimiting(RateLimitingPolicies.Default);

        group.MapGet("/markets", async (IMarketRepository repo) =>
        {
            var markets = await repo.GetMarketsAsync();
            return Results.Ok(new MarketsResponse(markets));
        })
        .WithName("GetMarkets")
        .WithSummary("Get available markets")
        .WithDescription("Returns a list of all available markets with neighborhood and listing counts.")
        .Produces<MarketsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);

        group.MapGet("/markets/{market}/neighborhoods", async (string market, IMarketRepository repo) =>
        {
            var neighborhoods = await repo.GetNeighborhoodsAsync(market);
            return Results.Ok(new NeighborhoodsResponse(market, neighborhoods));
        })
        .WithName("GetNeighborhoods")
        .WithSummary("Get neighborhoods for a market")
        .WithDescription("Returns all neighborhoods in the specified market with average price and occupancy data.")
        .Produces<NeighborhoodsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);

        group.MapGet("/markets/{market}/neighborhoods/{neighborhood}/configurations",
            async (string market, string neighborhood, IMarketRepository repo) =>
        {
            var configs = await repo.GetConfigurationsAsync(market, neighborhood);
            return Results.Ok(new ConfigurationsResponse(market, neighborhood, configs));
        })
        .WithName("GetConfigurations")
        .WithSummary("Get available bed/bath configurations")
        .WithDescription("Returns available bedroom/bathroom configurations for a neighborhood with listing counts and insight availability.")
        .Produces<ConfigurationsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
