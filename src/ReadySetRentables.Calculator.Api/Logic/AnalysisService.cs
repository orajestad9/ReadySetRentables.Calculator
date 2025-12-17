using ReadySetRentables.Calculator.Api.Data;
using ReadySetRentables.Calculator.Api.Domain.Analysis;

namespace ReadySetRentables.Calculator.Api.Logic;

/// <summary>
/// Service for analyzing property investment opportunities.
/// </summary>
public sealed class AnalysisService : IAnalysisService
{
    private readonly IMarketRepository _repository;
    private const decimal DefaultInterestRate = 6.89m; // Freddie Mac PMMS fallback

    public AnalysisService(IMarketRepository repository)
    {
        _repository = repository;
    }

    public async Task<AnalysisResult> AnalyzeAsync(AnalyzeRequest request)
    {
        var data = await _repository.GetNeighborhoodDataAsync(
            request.Market,
            request.Neighborhood,
            request.Bedrooms,
            request.Bathrooms);

        if (data is null)
        {
            return new AnalysisResult
            {
                Success = false,
                ErrorMessage = $"No data available for {request.Neighborhood} {request.Bedrooms}BR/{request.Bathrooms}BA in {request.Market}"
            };
        }

        var percentiles = await _repository.GetPercentilesAsync(
            request.Market,
            request.Neighborhood,
            request.Bedrooms);

        var interestRate = request.InterestRate ?? DefaultInterestRate;
        var grossRevenue = data.AvgRevenue > 0 ? data.AvgRevenue : (percentiles?.RevenueP50 ?? 0);

        var expenses = CalculateExpenses(request, grossRevenue, data.AvgPrice, interestRate);
        var metrics = CalculateMetrics(request, grossRevenue, expenses, data.AvgPrice);

        var profileSource = data.ComboProfile != null ? "combo" : "neighborhood_fallback";
        var profileText = data.ComboProfile ?? data.NeighborhoodProfile ?? "No profile available for this combination.";
        var profileDate = data.ComputedAt ?? data.NeighborhoodGeneratedAt ?? DateTime.UtcNow;

        var recommendation = DetermineRecommendation(metrics.CashOnCashReturn);
        var confidence = DetermineConfidence(data.ListingCount);
        var headline = BuildHeadline(metrics.CashOnCashReturn);

        var response = new AnalyzeResponse
        {
            Summary = new SummarySection
            {
                Headline = headline,
                Recommendation = recommendation,
                Confidence = confidence
            },
            Profile = new ProfileSection
            {
                Text = profileText,
                Source = profileSource,
                GeneratedAt = profileDate
            },
            Insights = new InsightsSection
            {
                SuccessFactors = data.SuccessFactors,
                RiskFactors = data.RiskFactors,
                PremiumAmenities = data.PremiumAmenities,
                Source = profileSource
            },
            Metrics = metrics,
            Revenue = BuildRevenueSection(data, percentiles),
            Expenses = expenses,
            Metadata = BuildMetadata(request, percentiles, data, interestRate)
        };

        return new AnalysisResult
        {
            Success = true,
            Response = response
        };
    }

    private static ExpensesSection CalculateExpenses(
        AnalyzeRequest request,
        decimal grossRevenue,
        decimal avgPrice,
        decimal interestRate)
    {
        var downPayment = request.PurchasePrice * (request.DownPaymentPercent / 100m);
        var loanAmount = request.PurchasePrice - downPayment;
        var monthlyMortgage = CalculateMonthlyMortgage(loanAmount, interestRate, request.LoanTermYears);
        var annualMortgage = monthlyMortgage * 12;

        var annualPropertyTax = request.PurchasePrice * 0.0125m;
        var annualInsurance = 2400m;
        var annualHoa = request.HoaMonthly * 12;
        var annualUtilities = 3000m;

        // Cleaning: $60 per turn, estimate turns from revenue/price
        var estimatedTurns = avgPrice > 0 ? grossRevenue / avgPrice : 80m;
        var annualCleaning = estimatedTurns * 60m;

        var platformFees = grossRevenue * 0.03m;
        var maintenance = grossRevenue * 0.02m;
        var totTax = grossRevenue * 0.105m;
        var strPermit = 125m;
        var propertyManagement = request.SelfManaged ? 0m : grossRevenue * 0.20m;

        var totalAnnual = annualMortgage + annualPropertyTax + annualInsurance + annualHoa
            + annualUtilities + annualCleaning + platformFees + maintenance + totTax + strPermit + propertyManagement;

        return new ExpensesSection
        {
            AnnualTotal = Math.Round(totalAnnual, 2),
            Monthly = Math.Round(totalAnnual / 12, 2),
            Breakdown = new Dictionary<string, ExpenseItem>
            {
                ["mortgage"] = new ExpenseItem
                {
                    Value = Math.Round(annualMortgage, 2),
                    Monthly = false,
                    Source = $"Calculated: ${loanAmount:N0} loan @ {interestRate}% (Freddie Mac PMMS), {request.LoanTermYears}yr"
                },
                ["propertyTax"] = new ExpenseItem
                {
                    Value = Math.Round(annualPropertyTax, 2),
                    Monthly = false,
                    Source = "San Diego County 1.25% of purchase price"
                },
                ["insurance"] = new ExpenseItem
                {
                    Value = Math.Round(annualInsurance, 2),
                    Monthly = false,
                    Source = "Estimated STR insurance, San Diego metro"
                },
                ["hoa"] = new ExpenseItem
                {
                    Value = Math.Round(annualHoa, 2),
                    Monthly = false,
                    Source = request.HoaMonthly > 0 ? $"User provided: ${request.HoaMonthly}/month" : "None"
                },
                ["utilities"] = new ExpenseItem
                {
                    Value = Math.Round(annualUtilities, 2),
                    Monthly = false,
                    Source = $"SDG&E average {request.Bedrooms}BR, 2024"
                },
                ["cleaning"] = new ExpenseItem
                {
                    Value = Math.Round(annualCleaning, 2),
                    Monthly = false,
                    Source = $"Calculated: $60/turn x estimated {estimatedTurns:N0} turns/year"
                },
                ["platformFees"] = new ExpenseItem
                {
                    Value = Math.Round(platformFees, 2),
                    Monthly = false,
                    Source = "Airbnb 3% host-only fee"
                },
                ["maintenance"] = new ExpenseItem
                {
                    Value = Math.Round(maintenance, 2),
                    Monthly = false,
                    Source = "2% of gross revenue (VRMA benchmark)"
                },
                ["totTax"] = new ExpenseItem
                {
                    Value = Math.Round(totTax, 2),
                    Monthly = false,
                    Source = "San Diego TOT 10.5% (Municipal Code 35.0103)"
                },
                ["strPermit"] = new ExpenseItem
                {
                    Value = Math.Round(strPermit, 2),
                    Monthly = false,
                    Source = "San Diego STRO annual renewal, 2024"
                },
                ["propertyManagement"] = new ExpenseItem
                {
                    Value = Math.Round(propertyManagement, 2),
                    Monthly = false,
                    Source = request.SelfManaged ? "Self-managed (user selected)" : "20% of gross revenue"
                }
            }
        };
    }

    private static decimal CalculateMonthlyMortgage(decimal loanAmount, decimal annualRate, int years)
    {
        if (loanAmount <= 0)
        {
            return 0;
        }

        var monthlyRate = annualRate / 100m / 12m;
        var numberOfPayments = years * 12;

        if (monthlyRate == 0)
        {
            return loanAmount / numberOfPayments;
        }

        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), numberOfPayments);
        return loanAmount * monthlyRate * factor / (factor - 1);
    }

    private static MetricsSection CalculateMetrics(
        AnalyzeRequest request,
        decimal grossRevenue,
        ExpensesSection expenses,
        decimal avgPrice)
    {
        var mortgageExpense = expenses.Breakdown.TryGetValue("mortgage", out var m) ? m.Value : 0;
        var noi = grossRevenue - (expenses.AnnualTotal - mortgageExpense);
        var cashFlow = grossRevenue - expenses.AnnualTotal;

        var downPayment = request.PurchasePrice * (request.DownPaymentPercent / 100m);
        var cashOnCash = downPayment > 0 ? cashFlow / downPayment : 0;
        var capRate = request.PurchasePrice > 0 ? noi / request.PurchasePrice : 0;
        var grossYield = request.PurchasePrice > 0 ? grossRevenue / request.PurchasePrice : 0;

        var breakEvenOccupancy = avgPrice > 0 ? expenses.AnnualTotal / (avgPrice * 365) : 0;

        return new MetricsSection
        {
            CashOnCashReturn = Math.Round(cashOnCash, 4),
            CapRate = Math.Round(capRate, 4),
            NetOperatingIncome = Math.Round(noi, 2),
            AnnualCashFlow = Math.Round(cashFlow, 2),
            BreakEvenOccupancy = Math.Round(breakEvenOccupancy, 4),
            GrossYield = Math.Round(grossYield, 4)
        };
    }

    private static RevenueSection BuildRevenueSection(NeighborhoodData data, PercentileData? percentiles)
    {
        var occupancyRate = data.AvgOccupancy > 0 ? data.AvgOccupancy / 365m : 0;

        return new RevenueSection
        {
            NightlyRate = new RateInfo
            {
                Value = Math.Round(data.AvgPrice, 2),
                Percentile = CalculatePercentile(data.AvgPrice, percentiles),
                Range = new RangeInfo
                {
                    P25 = percentiles?.PriceP25 ?? 0,
                    P50 = percentiles?.PriceP50 ?? 0,
                    P75 = percentiles?.PriceP75 ?? 0
                }
            },
            OccupancyRate = new OccupancyInfo
            {
                Value = Math.Round(occupancyRate, 2),
                SeasonalRange = new SeasonalRange
                {
                    Low = 0.55m,
                    High = 0.89m
                }
            },
            GrossAnnualRevenue = Math.Round(data.AvgRevenue, 2),
            ComparablesCount = percentiles?.ComparablesCount ?? data.ListingCount
        };
    }

    private static MetadataSection BuildMetadata(
        AnalyzeRequest request,
        PercentileData? percentiles,
        NeighborhoodData data,
        decimal interestRate)
    {
        var downPayment = request.PurchasePrice * (request.DownPaymentPercent / 100m);

        return new MetadataSection
        {
            AnalysisDate = DateTime.UtcNow,
            DataSources =
            [
                new DataSourceInfo
                {
                    Name = "Inside Airbnb",
                    Date = "2024-12-15",
                    Description = $"{percentiles?.ComparablesCount ?? data.ListingCount} comparable listings, {data.ReviewCount} reviews analyzed"
                },
                new DataSourceInfo
                {
                    Name = "Freddie Mac PMMS",
                    Date = "2025-01-09",
                    Description = $"30-year fixed rate: {interestRate}%"
                }
            ],
            Assumptions =
            [
                $"{request.DownPaymentPercent}% down payment (${downPayment:N0})",
                $"{request.LoanTermYears}-year fixed mortgage",
                request.SelfManaged ? "Self-managed property" : "Professional management (20%)",
                "Occupancy based on 12-month trailing average"
            ]
        };
    }

    private static int CalculatePercentile(decimal value, PercentileData? percentiles)
    {
        if (percentiles == null) return 50;
        if (value <= percentiles.PriceP25) return 25;
        if (value <= percentiles.PriceP50) return 50;
        if (value <= percentiles.PriceP75) return 75;
        return 90;
    }

    private static string DetermineRecommendation(decimal cashOnCash)
    {
        if (cashOnCash >= 0.08m) return "buy";
        if (cashOnCash >= 0.05m) return "consider";
        return "caution";
    }

    private static string DetermineConfidence(int listingCount)
    {
        if (listingCount >= 50) return "high";
        if (listingCount >= 20) return "medium";
        return "low";
    }

    private static string BuildHeadline(decimal cashOnCash)
    {
        var strength = cashOnCash >= 0.06m ? "Strong" : "Moderate";
        return $"{strength} investment potential with {cashOnCash:P1} cash-on-cash return";
    }
}
