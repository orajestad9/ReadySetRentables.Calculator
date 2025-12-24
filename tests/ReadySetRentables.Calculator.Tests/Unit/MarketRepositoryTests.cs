using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        var result = ParseJsonArrayHelper(null);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsEmpty()
    {
        var result = ParseJsonArrayHelper("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsWhitespace()
    {
        var result = ParseJsonArrayHelper("   ");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ParsesValidJsonArray()
    {
        var json = "[\"item1\", \"item2\", \"item3\"]";
        var result = ParseJsonArrayHelper(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("item1", result[0]);
        Assert.Equal("item2", result[1]);
        Assert.Equal("item3", result[2]);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsInvalid()
    {
        var result = ParseJsonArrayHelper("not valid json");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_ReturnsEmptyList_WhenJsonIsObject()
    {
        var result = ParseJsonArrayHelper("{\"key\": \"value\"}");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_HandlesEmptyArray()
    {
        var result = ParseJsonArrayHelper("[]");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseJsonArray_HandlesArrayWithNulls()
    {
        // JsonSerializer will include null as null strings in the list
        var json = "[\"item1\", null, \"item3\"]";
        var result = ParseJsonArrayHelper(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("item1", result[0]);
        Assert.Null(result[1]);
        Assert.Equal("item3", result[2]);
    }

    [Fact]
    public void ParseJsonArray_HandlesUnicodeCharacters()
    {
        var json = "[\"café\", \"日本語\", \"emoji 🏠\"]";
        var result = ParseJsonArrayHelper(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("café", result[0]);
        Assert.Equal("日本語", result[1]);
        Assert.Equal("emoji 🏠", result[2]);
    }

    [Fact]
    public void ParseJsonArray_HandlesSpecialCharacters()
    {
        var json = "[\"with\\\"quotes\", \"with\\nnewline\", \"with\\ttab\"]";
        var result = ParseJsonArrayHelper(json);

        Assert.Equal(3, result.Count);
        Assert.Equal("with\"quotes", result[0]);
        Assert.Equal("with\nnewline", result[1]);
        Assert.Equal("with\ttab", result[2]);
    }

    /// <summary>
    /// Helper method that mirrors the private ParseJsonArray logic for testing.
    /// </summary>
    private static List<string?> ParseJsonArrayHelper(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string?>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
