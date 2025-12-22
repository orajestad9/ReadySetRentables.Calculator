using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ReadySetRentables.Calculator.Api;

namespace ReadySetRentables.Calculator.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that configures the test server to use the Development environment,
/// which contains the correct PostgreSQL connection string for integration testing.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root to the API project directory so it finds appsettings.Development.json
        var apiProjectPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "src", "ReadySetRentables.Calculator.Api"));

        builder.UseContentRoot(apiProjectPath);
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(apiProjectPath);
            config.AddJsonFile("appsettings.json", optional: false);
            config.AddJsonFile("appsettings.Development.json", optional: false);
        });
    }
}
