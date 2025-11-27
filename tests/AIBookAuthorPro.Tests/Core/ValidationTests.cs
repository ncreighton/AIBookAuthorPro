// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Core.Validation;
using FluentAssertions;
using Xunit;

namespace AIBookAuthorPro.Tests.Core;

public class ValidationTests
{
    [Fact]
    public void ValidateProject_WithValidProject_ShouldReturnSuccess()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "My Novel",
            Metadata = new BookMetadata
            {
                Title = "My Novel",
                Author = "John Doe"
            }
        };

        // Act
        var result = Validators.ValidateProject(project);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateProject_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "",
            Metadata = new BookMetadata { Title = "Test" }
        };

        // Act
        var result = Validators.ValidateProject(project);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void ValidateProject_WithNullMetadata_ShouldReturnError()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Metadata = null!
        };

        // Act
        var result = Validators.ValidateProject(project);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Metadata");
    }

    [Fact]
    public void ValidateChapter_WithValidChapter_ShouldReturnSuccess()
    {
        // Arrange
        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = "Chapter 1: The Beginning",
            Order = 1
        };

        // Act
        var result = Validators.ValidateChapter(chapter);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateChapter_WithEmptyTitle_ShouldReturnError()
    {
        // Arrange
        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = "",
            Order = 1
        };

        // Act
        var result = Validators.ValidateChapter(chapter);

        // Assert
        result.IsValid.Should().BeFalse();
        result.GetFirstErrorMessage().Should().Contain("title");
    }

    [Fact]
    public void ValidateCharacter_WithValidCharacter_ShouldReturnSuccess()
    {
        // Arrange
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "John Smith"
        };

        // Act
        var result = Validators.ValidateCharacter(character);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCharacter_WithInvalidAge_ShouldReturnError()
    {
        // Arrange
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = -5
        };

        // Act
        var result = Validators.ValidateCharacter(character);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Age");
    }

    [Theory]
    [InlineData("sk-ant-api03-xxxxxxxxxxxxx", "Anthropic", true)]
    [InlineData("sk-xxxxxxxxxxxxxxxx", "OpenAI", true)]
    [InlineData("invalid", "Anthropic", false)]
    [InlineData("", "OpenAI", false)]
    public void ValidateApiKey_ShouldValidateCorrectly(string apiKey, string provider, bool expectedValid)
    {
        // Act
        var result = Validators.ValidateApiKey(apiKey, provider);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void ValidationResult_Combine_ShouldMergeErrors()
    {
        // Arrange
        var result1 = ValidationResult.Failure("Name", "Name is required");
        var result2 = ValidationResult.Failure("Email", "Email is invalid");

        // Act
        var combined = ValidationResult.Combine(result1, result2);

        // Assert
        combined.IsValid.Should().BeFalse();
        combined.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void ValidationResult_GetErrorsFor_ShouldFilterByProperty()
    {
        // Arrange
        var result = ValidationResult.Failure(
            new ValidationError("Name", "Name is required"),
            new ValidationError("Name", "Name is too short"),
            new ValidationError("Email", "Email is invalid"));

        // Act
        var nameErrors = result.GetErrorsFor("Name");

        // Assert
        nameErrors.Should().HaveCount(2);
    }
}
