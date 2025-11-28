// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIBookAuthorPro.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProjectService>>();
        _projectService = new ProjectService(_loggerMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidName_ShouldCreateProject()
    {
        // Arrange
        var projectName = "My New Novel";

        // Act
        var result = await _projectService.CreateAsync(projectName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(projectName);
        result.Value.Metadata.Should().NotBeNull();
        result.Value.Metadata.Title.Should().Be(projectName);
    }

    [Fact]
    public async Task CreateProjectAsync_WithEmptyName_ShouldFail()
    {
        // Arrange
        var projectName = "";

        // Act
        var result = await _projectService.CreateAsync(projectName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public async Task CreateProjectAsync_WithWhitespaceName_ShouldFail()
    {
        // Arrange
        var projectName = "   ";

        // Act
        var result = await _projectService.CreateAsync(projectName);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProjectAsync_WithValidProject_ShouldSave()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            FilePath = "C:\\test\\project.abpro",
            Metadata = new BookMetadata { Title = "Test" }
        };

        // Act
        var result = await _projectService.SaveAsync(project);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProjectAsync_WithNullProject_ShouldFail()
    {
        // Act
        var result = await _projectService.SaveAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProjectAsync_WithNoFilePath_ShouldFail()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test",
            FilePath = null,
            Metadata = new BookMetadata()
        };

        // Act
        var result = await _projectService.SaveAsync(project);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("path");
    }

    [Fact]
    public async Task LoadProjectAsync_WithNonExistentFile_ShouldFail()
    {
        // Arrange
        var filePath = "C:\\nonexistent\\project.abpro";

        // Act
        var result = await _projectService.LoadAsync(filePath);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetRecentProjectsAsync_ShouldReturnEmptyListInitially()
    {
        // Act
        var result = await _projectService.GetRecentProjectsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldInitializeCollections()
    {
        // Act
        var result = await _projectService.CreateAsync("Test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Chapters.Should().NotBeNull();
        result.Value.Characters.Should().NotBeNull();
        result.Value.Locations.Should().NotBeNull();
        result.Value.Outline.Should().NotBeNull();
    }
}
