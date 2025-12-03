# ReadySetRentables.Calculator

A small, focused .NET 10 API for short-term rental return-on-investment (ROI) calculations.

This service is intentionally minimal:

- **One core endpoint**: `POST /api/calculator/roi`
- **Pure, static calculation logic** (easy to test and evolve)
- **Built-in rate limiting** (fixed window per IP, 429 on abuse)
- **Unit + integration tests** using xUnit and `WebApplicationFactory`

It’s meant to be the clean foundation for the larger ReadySetRentables platform.

---

## Tech Stack

- **Runtime**: .NET 10 (ASP.NET Core minimal API)
- **Language**: C#
- **Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing`
- **Rate limiting**: ASP.NET Core rate limiting middleware (fixed window policy)

---

## Project Structure

```text
ReadySetRentables.Calculator/
├─ ReadySetRentables.Calculator.slnx
├─ src/
│  └─ ReadySetRentables.Calculator.Api/
│     ├─ Program.cs
│     ├─ Domain/
│     │  ├─ RentalInputs.cs
│     │  └─ RentalResult.cs
│     ├─ Logic/
│     │  └─ RoiCalculator.cs
│     ├─ Endpoints/
│     │  └─ CalculatorEndpoints.cs
│     └─ Security/
│        └─ RateLimitingExtensions.cs
└─ tests/
   └─ ReadySetRentables.Calculator.Tests/
      ├─ RoiCalculatorTests.cs
      └─ CalculatorEndpointTests.cs

Getting Started
Prerequisites

.NET 10 SDK installed
(Check with: dotnet --list-sdks)

Restore & Build

From the solution root:

dotnet restore
dotnet build

Run the API

From the solution root:

dotnet run --project src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj


The app will start and log something like:

Now listening on: https://localhost:XXXXX
Now listening on: http://localhost:YYYYY


Use those ports for requests (examples below).

API Endpoints
Health

GET /health
Returns a simple "healthy" string (if you’ve wired it as suggested) and is useful for load balancers / uptime checks.

Calculate ROI

POST /api/calculator/roi

Calculates basic monthly/annual profit and simple cap rate.

Request Body
{
  "nightlyRate": 150,
  "nightsBookedPerMonth": 20,
  "cleaningFeePerStay": 80,
  "staysPerMonth": 10,
  "monthlyFixedCosts": 2500,
  "purchasePrice": 400000
}


nightlyRate – Average nightly price.

nightsBookedPerMonth – Expected booked nights per month.

cleaningFeePerStay – Cleaning fee earned per stay.

staysPerMonth – Number of stays per month.

monthlyFixedCosts – Mortgage, utilities, insurance, etc.

purchasePrice – Property purchase price (used for cap rate).

Example Response
{
  "monthlyRevenue": 4600,
  "monthlyCosts": 2500,
  "monthlyProfit": 2100,
  "annualProfit": 25200,
  "capRatePercent": 6.3
}

Example Requests

Assume the app is listening on http://localhost:YYYYY (check the console output when you run it).

PowerShell (Invoke-RestMethod)
$body = @{
    nightlyRate          = 150
    nightsBookedPerMonth = 20
    cleaningFeePerStay   = 80
    staysPerMonth        = 10
    monthlyFixedCosts    = 2500
    purchasePrice        = 400000
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "http://localhost:YYYYY/api/calculator/roi" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

curl (real curl, e.g. on WSL/macOS/Linux)
curl -X POST "http://localhost:YYYYY/api/calculator/roi" \
  -H "Content-Type: application/json" \
  -d '{
    "nightlyRate": 150,
    "nightsBookedPerMonth": 20,
    "cleaningFeePerStay": 80,
    "staysPerMonth": 10,
    "monthlyFixedCosts": 2500,
    "purchasePrice": 400000
  }'

Rate Limiting

The API uses ASP.NET Core’s rate limiting middleware with a fixed window policy:

Policy name: default

Limit: 60 requests per minute per client IP

Queue limit: 0 (no queuing)

Rejection status: 429 Too Many Requests

The policy is applied to the calculator routes via:

app.MapGroup("/api/calculator")
   .RequireRateLimiting("default");


This gives basic protection against abuse and accidental hammering while keeping the configuration small and easy to tweak later.

Testing

From the solution root:

dotnet test



What the tests cover:

RoiCalculatorTests

Verifies the math for typical cases.

Confirms validation rules (e.g., negative values, zero purchase price) throw as expected.

CalculatorEndpointTests

Integration tests using WebApplicationFactory<Program>.

Ensures:

Valid payloads return 200 OK with non-zero metrics.

Invalid payloads (e.g. purchasePrice = 0) return 400 Bad Request.

Rate limiting eventually returns 429 Too Many Requests when the endpoint is hammered.

Roadmap / Future Ideas

This project is intentionally minimal, but is designed to grow:

Add inverse calculators:

Required nightly rate for a target cap rate.

Required occupancy to reach a monthly profit goal.

Plug in real data sources:

Use DuckDB / Neon / DynamoDB as the ROI data backend.

Add authentication / API keys for public-facing use.

Containerize for deployment to AWS (ECS Fargate, etc.).

Expose additional metrics (cash-on-cash return, break-even analysis, scenario comparisons).

For now, it stays lean and focused so it’s easy to extend without turning into a ball of mud.


You can tweak the ports / health endpoint bits if you decide not to keep `/health`, but this should be a solid starting point to commit as-is.
::contentReference[oaicite:0]{index=0}