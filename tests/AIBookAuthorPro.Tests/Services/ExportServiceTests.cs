// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIBookAuthorPro.Tests.Services;

public class ExportServiceTests
{
    private readonly Mock<IFileSystemService> _fileSystemMock;
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly ExportService _exportService;

    public ExportServiceTests()
    {
        _fileSystemMock = new Mock<IFileSystemService>();
        _loggerMock = new Mock<ILogger<ExportService>>();
        _exportService = new ExportService(_fileSystemMock.Object, _loggerMock.Object);
    }

    private Project CreateTestProject()
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Novel",
            Metadata = new BookMetadata
            {
                Title = "Test Novel",
                Author = "Test Author",
                Description = "A test novel for unit testing."
            },
            Chapters = new List<Chapter>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Chapter 1: The Beginning",
                    Content = "<Paragraph>This is the first chapter.</Paragraph>",
                    Order = 1,
                    Status = ChapterStatus.Draft
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Chapter 2: The Middle",
                    Content = "<Paragraph>This is the second chapter.</Paragraph>",
                    Order = 2,
                    Status = ChapterStatus.Draft
                }
            }
        };
    }

    [Fact]
    public async Task ExportAsync_WithValidProject_ShouldSucceed()
    {
        // Arrange
        var project = CreateTestProject();
        var options = new ExportOptions
        {
            Format = ExportFormat.Markdown,
            OutputPath = "C:\\test\\output.md",
            IncludeTableOfContents = true
        };

        _fileSystemMock
            .Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _exportService.ExportAsync(project, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_WithNullProject_ShouldFail()
    {
        // Arrange
        var options = new ExportOptions
        {
            Format = ExportFormat.Markdown,
            OutputPath = "C:\\test\\output.md"
        };

        // Act
        var result = await _exportService.ExportAsync(null!, options);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_WithNoChapters_ShouldFail()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Empty",
            Metadata = new BookMetadata { Title = "Empty" },
            Chapters = new List<Chapter>()
        };

        var options = new ExportOptions
        {
            Format = ExportFormat.Markdown,
            OutputPath = "C:\\test\\output.md"
        };

        // Act
        var result = await _exportService.ExportAsync(project, options);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("chapters");
    }

    [Theory]
    [InlineData(ExportFormat.Markdown, ".md")]
    [InlineData(ExportFormat.Html, ".html")]
    [InlineData(ExportFormat.PlainText, ".txt")]
    [InlineData(ExportFormat.Docx, ".docx")]
    public void GetFileExtension_ShouldReturnCorrectExtension(ExportFormat format, string expectedExtension)
    {
        // Act
        var extension = _exportService.GetFileExtension(format);

        // Assert
        extension.Should().Be(expectedExtension);
    }

    [Fact]
    public void GetSupportedFormats_ShouldReturnAllFormats()
    {
        // Act
        var formats = _exportService.GetSupportedFormats();

        // Assert
        formats.Should().Contain(ExportFormat.Markdown);
        formats.Should().Contain(ExportFormat.Html);
        formats.Should().Contain(ExportFormat.Docx);
        formats.Should().Contain(ExportFormat.PlainText);
    }

    [Fact]
    public async Task ExportAsync_WithChapterFilter_ShouldOnlyExportSelectedChapters()
    {
        // Arrange
        var project = CreateTestProject();
        var selectedChapterId = project.Chapters.First().Id;

        var options = new ExportOptions
        {
            Format = ExportFormat.Markdown,
            OutputPath = "C:\\test\\output.md",
            ChapterIds = new List<Guid> { selectedChapterId }
        };

        string? exportedContent = null;
        _fileSystemMock
            .Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((path, content, ct) => exportedContent = content)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _exportService.ExportAsync(project, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        exportedContent.Should().Contain("Chapter 1");
        exportedContent.Should().NotContain("Chapter 2");
    }

    [Fact]
    public async Task ExportAsync_ShouldReportProgress()
    {
        // Arrange
        var project = CreateTestProject();
        var options = new ExportOptions
        {
            Format = ExportFormat.Markdown,
            OutputPath = "C:\\test\\output.md"
        };

        var progressValues = new List<int>();
        var progress = new Progress<int>(value => progressValues.Add(value));

        _fileSystemMock
            .Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _exportService.ExportAsync(project, options, progress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Progress should have been reported
        await Task.Delay(100); // Allow progress callbacks to complete
        progressValues.Should().NotBeEmpty();
    }
}
