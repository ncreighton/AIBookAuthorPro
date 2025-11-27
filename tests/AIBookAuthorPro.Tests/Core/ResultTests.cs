// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Models;
using FluentAssertions;
using Xunit;

namespace AIBookAuthorPro.Tests.Core;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Act
        var result = Result<int>.Failure("Something went wrong");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Something went wrong");
    }

    [Fact]
    public void Failure_WithException_ShouldIncludeException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = Result<int>.Failure("Error", exception);

        // Assert
        result.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void Match_OnSuccess_ShouldCallOnSuccessFunction()
    {
        // Arrange
        var result = Result<int>.Success(10);

        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: error => $"Error: {error}");

        // Assert
        output.Should().Be("Value: 10");
    }

    [Fact]
    public void Match_OnFailure_ShouldCallOnFailureFunction()
    {
        // Arrange
        var result = Result<int>.Failure("Not found");

        // Act
        var output = result.Match(
            onSuccess: value => $"Value: {value}",
            onFailure: error => $"Error: {error}");

        // Assert
        output.Should().Be("Error: Not found");
    }

    [Fact]
    public void Success_WithNullValue_ShouldBeAllowed()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
