using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ReadySetRentables.Calculator.Api.Domain;
using ReadySetRentables.Calculator.Api.Logic;

namespace ReadySetRentables.Calculator.Api.Endpoints;

/// <summary>
/// Extension methods for mapping calculator API endpoints.
/// </summary>
public static class CalculatorEndpoints
{
    /// <summary>
    /// Maps the calculator endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapCalculatorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/calculator");

        group.MapPost("/roi", (RentalInputs inputs, IRoiCalculator calculator, ILogger<Program> logger) =>
        {
            try
            {
                var result = calculator.Calculate(inputs);
                logger.LogInformation(
                    "ROI calculated successfully: CapRate={CapRate}%, MonthlyProfit={MonthlyProfit}",
                    result.CapRatePercent,
                    result.MonthlyProfit);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid input received for ROI calculation");
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error");
            }
        })
        .WithName("CalculateRoi")
        .WithSummary("Calculate basic rental ROI metrics")
        .WithDescription("Calculates monthly/annual profit and simple cap-rate for a short-term rental.")
        .Produces<RentalResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
