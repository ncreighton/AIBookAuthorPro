// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;
using FluentAssertions;
using Xunit;
using CharacterRoleEnum = AIBookAuthorPro.Core.Enums.CharacterRole;
using OutlineItemTypeEnum = AIBookAuthorPro.Core.Enums.OutlineItemType;

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
    public void Chapter_ShouldHaveDefaults()
    {
        // Arrange & Act
        var chapter = new Chapter
        {
            Title = "Test"
        };

        // Assert
        chapter.Id.Should().NotBeEmpty();
        chapter.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        chapter.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Character_DefaultRole_ShouldBeSupporting()
    {
        // Act
        var character = new Character { Name = "Test" };

        // Assert
        character.Role.Should().Be((Core.Models.CharacterRole)CharacterRoleEnum.Supporting);
    }

    [Fact]
    public void BookMetadata_ShouldHaveDefaults()
    {
        // Act
        var metadata = new BookMetadata();

        // Assert
        metadata.Title.Should().BeEmpty();
        metadata.Author.Should().BeEmpty();
        metadata.Genre.Should().BeEmpty();
    }

    [Fact]
    public void OutlineItem_WithChildren_ShouldTrackHierarchy()
    {
        // Arrange
        var parent = new OutlineItem
        {
            Title = "Act I",
            ItemType = (Core.Models.OutlineItemType)OutlineItemTypeEnum.Act
        };

        var child = new OutlineItem
        {
            Title = "Chapter 1",
            ItemType = (Core.Models.OutlineItemType)OutlineItemTypeEnum.Chapter
        };

        // Act
        parent.AddChild(child);

        // Assert
        parent.Children.Should().Contain(child);
        parent.Children.Should().HaveCount(1);
    }

    [Fact]
    public void CharacterRelationship_ShouldStoreRelationshipType()
    {
        // Arrange
        var character1 = new Character { Name = "Alice" };
        var character2 = new Character { Name = "Bob" };

        var relationship = new CharacterRelationship
        {
            OtherCharacterId = character2.Id,
            RelationshipType = "Sibling"
        };

        // Act
        character1.AddRelationship(relationship);

        // Assert
        character1.Relationships.Should().HaveCount(1);
        character1.Relationships.First().RelationshipType.Should().Be("Sibling");
    }

    [Fact]
    public void ProjectSummary_ShouldContainSummaryInformation()
    {
        // Arrange
        var projectSummary = new ProjectSummary
        {
            Name = "My Novel",
            FilePath = "C:\\Projects\\novel.abpro",
            LastModified = DateTime.UtcNow,
            WordCount = 50000,
            ChapterCount = 15
        };

        // Assert
        projectSummary.Name.Should().Be("My Novel");
        projectSummary.WordCount.Should().Be(50000);
        projectSummary.ChapterCount.Should().Be(15);
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
