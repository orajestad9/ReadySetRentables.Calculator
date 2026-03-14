using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReadySetRentables.Calculator.Api.Data;
using NSubstitute;
using Xunit;

namespace ReadySetRentables.Calculator.Tests.Unit;

public class MarketRepositoryTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        var logger = Substitute.For<ILogger<ReadySetRentables.Calculator.Api.Data.MarketRepository>>();

        Assert.Throws<ArgumentNullException>(() =>
            new ReadySetRentables.Calculator.Api.Data.MarketRepository(null!, logger));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("PostgreSQL").Returns("Host=localhost;Database=test");

        Assert.Throws<ArgumentNullException>(() =>
            new ReadySetRentables.Calculator.Api.Data.MarketRepository(configuration, null!));
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenConnectionStringMissing()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("PostgreSQL").Returns((string?)null);
        var logger = Substitute.For<ILogger<ReadySetRentables.Calculator.Api.Data.MarketRepository>>();

        Assert.Throws<InvalidOperationException>(() =>
            new ReadySetRentables.Calculator.Api.Data.MarketRepository(configuration, logger));
    }
}

/// <summary>
/// Tests for JSON parsing functionality extracted for testability.
/// </summary>
public class JsonParsingTests
{
    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsNull()
    {
        var result = InvokeParseJsonArray(null);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsEmpty()
    {
        var result = InvokeParseJsonArray("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsWhitespace()
    {
        var result = InvokeParseJsonArray("   ");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ParsesValidJsonArray()
    {
        var json = "[\"item1\", \"item2\", \"item3\"]";
        var result = InvokeParseJsonArray(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("item1", result[0]);
        Assert.Equal("item2", result[1]);
        Assert.Equal("item3", result[2]);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsInvalid()
    {
        var result = InvokeParseJsonArray("not valid json");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsObject()
    {
        var result = InvokeParseJsonArray("{\"key\": \"value\"}");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_HandlesEmptyArray()
    {
        var result = InvokeParseJsonArray("[]");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_HandlesArrayWithNulls()
    {
        // JsonSerializer will include null as null strings in the list
        var json = "[\"item1\", null, \"item3\"]";
        var result = InvokeParseJsonArray(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("item1", result[0]);
        Assert.Null(result[1]);
        Assert.Equal("item3", result[2]);
    }

    [Fact]
    public void ParseJsonArray_HandlesUnicodeCharacters()
    {
        var json = "[\"café\", \"日本語\", \"emoji 🏠\"]";
        var result = InvokeParseJsonArray(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("café", result[0]);
        Assert.Equal("日本語", result[1]);
        Assert.Equal("emoji 🏠", result[2]);
    }

    [Fact]
    public void ParseJsonArray_HandlesSpecialCharacters()
    {
        var json = "[\"with\\\"quotes\", \"with\\nnewline\", \"with\\ttab\"]";
        var result = InvokeParseJsonArray(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("with\"quotes", result[0]);
        Assert.Equal("with\nnewline", result[1]);
        Assert.Equal("with\ttab", result[2]);
    }

    private static List<string?> InvokeParseJsonArray(string? json)
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration.GetConnectionString("PostgreSQL").Returns("Host=localhost;Database=test");
        var logger = Substitute.For<ILogger<MarketRepository>>();
        var repository = new MarketRepository(configuration, logger);

        var method = typeof(MarketRepository).GetMethod("ParseJsonArray", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        return (List<string?>)method!.Invoke(repository, [json])!;
    }
}
