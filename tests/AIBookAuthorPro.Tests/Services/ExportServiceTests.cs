// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using AIBookAuthorPro.Infrastructure.Services;
using AIBookAuthorPro.Core.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIBookAuthorPro.Tests.Services;

public class ExportServiceTests
{
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly Mock<IDocxExporter> _docxExporterMock;
    private readonly Mock<IMarkdownExporter> _markdownExporterMock;
    private readonly Mock<IHtmlExporter> _htmlExporterMock;
    private readonly ExportService _exportService;

    public ExportServiceTests()
    {
        _loggerMock = new Mock<ILogger<ExportService>>();
        _docxExporterMock = new Mock<IDocxExporter>();
        _markdownExporterMock = new Mock<IMarkdownExporter>();
        _htmlExporterMock = new Mock<IHtmlExporter>();
        _exportService = new ExportService(
            _loggerMock.Object,
            _docxExporterMock.Object,
            _markdownExporterMock.Object,
            _htmlExporterMock.Object);
    }

    private Project CreateTestProject()
    {
        var project = new Project
        {
            Name = "Test Novel",
            Metadata = new BookMetadata
            {
                Title = "Test Novel",
                Author = "Test Author",
                Description = "A test novel for unit testing."
            }
        };

        var chapter1 = new Chapter
        {
            Title = "Chapter 1: The Beginning",
            Content = "<Paragraph>This is the first chapter.</Paragraph>",
            Order = 1,
            Status = ChapterStatus.FirstDraft
        };
        project.AddChapter(chapter1);

        var chapter2 = new Chapter
        {
            Title = "Chapter 2: The Middle",
            Content = "<Paragraph>This is the second chapter.</Paragraph>",
            Order = 2,
            Status = ChapterStatus.FirstDraft
        };
        project.AddChapter(chapter2);

        return project;
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

        _markdownExporterMock
            .Setup(x => x.ExportAsync(It.IsAny<Project>(), It.IsAny<IReadOnlyList<Chapter>>(), It.IsAny<ExportOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(options.OutputPath));

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
            Name = "Empty",
            Metadata = new BookMetadata { Title = "Empty" }
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

    [Fact]
    public void GetAvailableFormats_ShouldReturnAllFormats()
    {
        // Act
        var formats = _exportService.GetAvailableFormats();

        // Assert
        formats.Should().NotBeEmpty();
        formats.Should().Contain(f => f.Format == ExportFormat.Markdown);
        formats.Should().Contain(f => f.Format == ExportFormat.Html);
        formats.Should().Contain(f => f.Format == ExportFormat.Docx);
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
            ChapterFilter = new List<Guid> { selectedChapterId }
        };

        _markdownExporterMock
            .Setup(x => x.ExportAsync(It.IsAny<Project>(), It.IsAny<IReadOnlyList<Chapter>>(), It.IsAny<ExportOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(options.OutputPath));

        // Act
        var result = await _exportService.ExportAsync(project, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _markdownExporterMock.Verify(x => x.ExportAsync(
            project,
            It.Is<List<Chapter>>(chapters => chapters.Count == 1 && chapters.First().Title.Contains("Chapter 1")),
            options,
            It.IsAny<CancellationToken>()), Times.Once);
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

        _markdownExporterMock
            .Setup(x => x.ExportAsync(It.IsAny<Project>(), It.IsAny<IReadOnlyList<Chapter>>(), It.IsAny<ExportOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(options.OutputPath));

        // Act
        var result = await _exportService.ExportAsync(project, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
