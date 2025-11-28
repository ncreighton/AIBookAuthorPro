// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Models;
using FluentAssertions;
using Xunit;

namespace AIBookAuthorPro.Tests.Core;

public class ModelTests
{
    [Fact]
    public void Project_NewProject_ShouldHaveDefaults()
    {
        // Act
        var project = new Project();

        // Assert
        project.Id.Should().NotBeEmpty();
        project.Chapters.Should().NotBeNull();
        project.Characters.Should().NotBeNull();
        project.Locations.Should().NotBeNull();
    }

    [Fact]
    public void Chapter_MarkAsModified_ShouldUpdateTimestamp()
    {
        // Arrange
        var chapter = new Chapter
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            ModifiedAt = DateTime.UtcNow.AddDays(-1)
        };
        var originalModified = chapter.ModifiedAt;

        // Act
        chapter.MarkAsModified();

        // Assert
        chapter.ModifiedAt.Should().BeAfter(originalModified);
    }

    [Fact]
    public void Character_DefaultRole_ShouldBeSupporting()
    {
        // Act
        var character = new Character { Name = "Test" };

        // Assert
        character.Role.Should().Be(CharacterRole.Supporting);
    }

    [Fact]
    public void BookMetadata_WordCount_ShouldDefaultToZero()
    {
        // Act
        var metadata = new BookMetadata();

        // Assert
        metadata.WordCount.Should().Be(0);
        metadata.TargetWordCount.Should().Be(0);
    }

    [Fact]
    public void OutlineItem_WithChildren_ShouldTrackParentId()
    {
        // Arrange
        var parent = new OutlineItem
        {
            Id = Guid.NewGuid(),
            Title = "Act I",
            Type = OutlineItemType.Act
        };

        var child = new OutlineItem
        {
            Id = Guid.NewGuid(),
            Title = "Chapter 1",
            Type = OutlineItemType.Chapter,
            ParentId = parent.Id
        };

        // Assert
        child.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public void CharacterRelationship_ShouldStoreRelationshipType()
    {
        // Arrange
        var character1 = new Character { Id = Guid.NewGuid(), Name = "Alice" };
        var character2 = new Character { Id = Guid.NewGuid(), Name = "Bob" };

        var relationship = new CharacterRelationship
        {
            CharacterId = character2.Id,
            RelationshipType = "Sibling"
        };

        // Act
        character1.Relationships.Add(relationship);

        // Assert
        character1.Relationships.Should().HaveCount(1);
        character1.Relationships.First().RelationshipType.Should().Be("Sibling");
    }

    [Fact]
    public void ProjectInfo_ShouldContainSummaryInformation()
    {
        // Arrange
        var projectInfo = new ProjectInfo
        {
            Id = Guid.NewGuid(),
            Name = "My Novel",
            FilePath = "C:\\Projects\\novel.abpro",
            LastOpened = DateTime.UtcNow,
            WordCount = 50000,
            ChapterCount = 15
        };

        // Assert
        projectInfo.Name.Should().Be("My Novel");
        projectInfo.WordCount.Should().Be(50000);
        projectInfo.ChapterCount.Should().Be(15);
    }

    [Theory]
    [InlineData(ChapterStatus.Outlined, false)]
    [InlineData(ChapterStatus.FirstDraft, true)]
    [InlineData(ChapterStatus.Revising, true)]
    [InlineData(ChapterStatus.Complete, true)]
    public void ChapterStatus_ShouldIndicateExportability(ChapterStatus status, bool shouldExport)
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test",
            Status = status
        };

        // Act & Assert
        var isExportable = chapter.Status != ChapterStatus.Outlined;
        isExportable.Should().Be(shouldExport);
    }
}
