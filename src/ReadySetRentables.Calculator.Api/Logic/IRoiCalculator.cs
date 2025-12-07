using ReadySetRentables.Calculator.Api.Domain;

namespace ReadySetRentables.Calculator.Api.Logic;

/// <summary>
/// Defines the contract for ROI calculation operations.
/// </summary>
public interface IRoiCalculator
{
    /// <summary>
    /// Calculates ROI metrics for a short-term rental property.
    /// </summary>
    /// <param name="input">The rental property inputs.</param>
    /// <returns>Calculated ROI metrics including cap rate.</returns>
    /// <exception cref="ArgumentException">Thrown when input values are invalid.</exception>
    RentalResult Calculate(RentalInputs input);
}
