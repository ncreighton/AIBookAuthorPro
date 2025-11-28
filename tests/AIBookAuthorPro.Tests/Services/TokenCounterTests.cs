// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AIBookAuthorPro.Tests.Services;

public class TokenCounterTests
{
    private readonly TokenCounter _tokenCounter;

    public TokenCounterTests()
    {
        _tokenCounter = new TokenCounter();
    }

    [Fact]
    public void EstimateTokens_EmptyString_ShouldReturnZero()
    {
        // Act
        var result = _tokenCounter.EstimateTokens("");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void EstimateTokens_NullString_ShouldReturnZero()
    {
        // Act
        string? nullText = null;
        var result = _tokenCounter.EstimateTokens(nullText);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("Hello", 1, 3)] // Short word
    [InlineData("Hello world", 2, 5)] // Two words
    [InlineData("The quick brown fox jumps over the lazy dog.", 8, 15)] // Sentence
    public void EstimateTokens_ShouldBeWithinReasonableRange(string text, int minExpected, int maxExpected)
    {
        // Act
        var result = _tokenCounter.EstimateTokens(text);

        // Assert
        result.Should().BeInRange(minExpected, maxExpected);
    }

    [Fact]
    public void EstimateTokens_LongText_ShouldScaleAppropriately()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Repeat("word", 1000));

        // Act
        var result = _tokenCounter.EstimateTokens(text);

        // Assert
        // Roughly 1 token per word for simple words
        result.Should().BeInRange(800, 1500);
    }

    [Fact]
    public void EstimateTokens_CodeContent_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var code = @"public class Test { void Method() { Console.WriteLine(""Hello""); } }";

        // Act
        var result = _tokenCounter.EstimateTokens(code);

        // Assert
        result.Should().BeGreaterThan(10);
    }

    [Fact]
    public void GetMaxContextTokens_Claude_ShouldReturn200K()
    {
        // Act
        var result = _tokenCounter.GetMaxContextTokens("claude-sonnet-4-20250514");

        // Assert
        result.Should().Be(200000);
    }

    [Fact]
    public void GetMaxContextTokens_GPT4o_ShouldReturn128K()
    {
        // Act
        var result = _tokenCounter.GetMaxContextTokens("gpt-4o");

        // Assert
        result.Should().Be(128000);
    }

    [Fact]
    public void GetMaxContextTokens_UnknownModel_ShouldReturnDefault()
    {
        // Act
        var result = _tokenCounter.GetMaxContextTokens("unknown-model");

        // Assert
        result.Should().Be(8000); // Conservative default
    }

    [Fact]
    public void EstimateCost_ShouldCalculateCorrectly()
    {
        // Arrange
        var inputTokens = 1000;
        var outputTokens = 500;
        var model = "claude-sonnet-4-20250514";

        // Act
        var cost = _tokenCounter.EstimateCost(model, inputTokens, outputTokens);

        // Assert
        cost.Should().BeGreaterThan(0);
    }
}
