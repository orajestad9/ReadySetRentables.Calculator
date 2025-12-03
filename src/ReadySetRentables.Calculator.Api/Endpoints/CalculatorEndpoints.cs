using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReadySetRentables.Calculator.Api.Domain;
using ReadySetRentables.Calculator.Api.Logic;

namespace ReadySetRentables.Calculator.Api.Endpoints
{
    public static class CalculatorEndpoints
    {
        public static IEndpointRouteBuilder MapCalculatorEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup("/api/calculator")
                .RequireRateLimiting("default");

            group.MapPost("/roi", (RentalInputs inputs) =>
            {
                try
                {
                    var result = RoiCalculator.Calculate(inputs);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CalculateRoi")
            .WithSummary("Calculate basic rental ROI metrics")
            .WithDescription("Calculates monthly/annual profit and simple cap-rate for a short-term rental.");

            return app;
        }
    }
}
