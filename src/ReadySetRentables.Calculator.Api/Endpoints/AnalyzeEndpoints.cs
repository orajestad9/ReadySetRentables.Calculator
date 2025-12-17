using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ReadySetRentables.Calculator.Api.Domain.Analysis;
using ReadySetRentables.Calculator.Api.Logic;
using ReadySetRentables.Calculator.Api.Security;

namespace ReadySetRentables.Calculator.Api.Endpoints;

/// <summary>
/// Extension methods for mapping analysis API endpoints.
/// </summary>
public static class AnalyzeEndpoints
{
    /// <summary>
    /// Maps the analyze endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapAnalyzeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1")
            .RequireRateLimiting(RateLimitingPolicies.Default);

        group.MapPost("/analyze", async (
            AnalyzeRequest request,
            IAnalysisService analysisService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation(
                "Analyzing property: Neighborhood={Neighborhood}, Bedrooms={Bedrooms}, Bathrooms={Bathrooms}",
                request.Neighborhood,
                request.Bedrooms,
                request.Bathrooms);

            var result = await analysisService.AnalyzeAsync(request);

            if (!result.Success)
            {
                logger.LogWarning(
                    "No data found for {Neighborhood} {Bedrooms}BR/{Bathrooms}BA",
                    request.Neighborhood,
                    request.Bedrooms,
                    request.Bathrooms);

                return Results.Problem(
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Data Not Found",
                    extensions: new Dictionary<string, object?>
                    {
                        ["supportedCombinations"] = result.SupportedCombinations
                    });
            }

            logger.LogInformation(
                "Analysis completed: {Market}/{Neighborhood} {Bedrooms}BR - CashOnCash={CashOnCash:P1}",
                request.Market,
                request.Neighborhood,
                request.Bedrooms,
                result.Response!.Metrics.CashOnCashReturn);

            return Results.Ok(result.Response);
        })
        .WithName("AnalyzeProperty")
        .WithSummary("Analyze a short-term rental investment opportunity")
        .WithDescription("Returns comprehensive investment analysis including ROI metrics, revenue estimates, expense projections, and neighborhood insights.")
        .Produces<AnalyzeResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
