using System;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using ReadySetRentables.Calculator.Api.Data;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class NeighborhoodRepositoryTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NeighborhoodRepository(null!));
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenConnectionStringMissing()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("PostgreSQL").Returns((string?)null);

        Assert.Throws<InvalidOperationException>(() =>
            new NeighborhoodRepository(configuration));
    }

    [Fact]
    public void Constructor_Succeeds_WhenConnectionStringProvided()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("PostgreSQL").Returns("Host=localhost;Database=test");

        var repository = new NeighborhoodRepository(configuration);

        Assert.NotNull(repository);
    }
}
