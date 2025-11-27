// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models;

namespace AIBookAuthorPro.Core.Validation;

/// <summary>
/// Validators for domain models.
/// </summary>
public static class Validators
{
    /// <summary>
    /// Validates a project.
    /// </summary>
    public static ValidationResult ValidateProject(Project project)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            errors.Add(new ValidationError(nameof(project.Name), "Project name is required"));
        }
        else if (project.Name.Length > 200)
        {
            errors.Add(new ValidationError(nameof(project.Name), "Project name cannot exceed 200 characters"));
        }

        if (project.Metadata == null)
        {
            errors.Add(new ValidationError(nameof(project.Metadata), "Project metadata is required"));
        }
        else
        {
            var metadataResult = ValidateBookMetadata(project.Metadata);
            errors.AddRange(metadataResult.Errors);
        }

        // Validate target word count on Project (not BookMetadata)
        if (project.TargetWordCount < 0)
        {
            errors.Add(new ValidationError(nameof(project.TargetWordCount), "Target word count cannot be negative"));
        }

        if (project.TargetWordCount > 1000000)
        {
            errors.Add(new ValidationError(nameof(project.TargetWordCount), "Target word count cannot exceed 1,000,000"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates book metadata.
    /// </summary>
    public static ValidationResult ValidateBookMetadata(BookMetadata metadata)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            errors.Add(new ValidationError(nameof(metadata.Title), "Book title is required"));
        }
        else if (metadata.Title.Length > 300)
        {
            errors.Add(new ValidationError(nameof(metadata.Title), "Book title cannot exceed 300 characters"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates a chapter.
    /// </summary>
    public static ValidationResult ValidateChapter(Chapter chapter)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(chapter.Title))
        {
            errors.Add(new ValidationError(nameof(chapter.Title), "Chapter title is required"));
        }
        else if (chapter.Title.Length > 200)
        {
            errors.Add(new ValidationError(nameof(chapter.Title), "Chapter title cannot exceed 200 characters"));
        }

        if (chapter.Order < 0)
        {
            errors.Add(new ValidationError(nameof(chapter.Order), "Chapter order cannot be negative"));
        }

        if (chapter.TargetWordCount < 0)
        {
            errors.Add(new ValidationError(nameof(chapter.TargetWordCount), "Target word count cannot be negative"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates a character.
    /// </summary>
    public static ValidationResult ValidateCharacter(Character character)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(character.Name))
        {
            errors.Add(new ValidationError(nameof(character.Name), "Character name is required"));
        }
        else if (character.Name.Length > 100)
        {
            errors.Add(new ValidationError(nameof(character.Name), "Character name cannot exceed 100 characters"));
        }

        if (character.Age.HasValue && (character.Age < 0 || character.Age > 10000))
        {
            errors.Add(new ValidationError(nameof(character.Age), "Character age must be between 0 and 10,000"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates a location.
    /// </summary>
    public static ValidationResult ValidateLocation(Location location)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(location.Name))
        {
            errors.Add(new ValidationError(nameof(location.Name), "Location name is required"));
        }
        else if (location.Name.Length > 200)
        {
            errors.Add(new ValidationError(nameof(location.Name), "Location name cannot exceed 200 characters"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates an outline item.
    /// </summary>
    public static ValidationResult ValidateOutlineItem(OutlineItem item)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            errors.Add(new ValidationError(nameof(item.Title), "Outline item title is required"));
        }
        else if (item.Title.Length > 300)
        {
            errors.Add(new ValidationError(nameof(item.Title), "Outline item title cannot exceed 300 characters"));
        }

        if (item.Order < 0)
        {
            errors.Add(new ValidationError(nameof(item.Order), "Order cannot be negative"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates export options.
    /// </summary>
    public static ValidationResult ValidateExportOptions(ExportOptions options)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            errors.Add(new ValidationError(nameof(options.OutputPath), "Output path is required"));
        }
        else
        {
            try
            {
                var directory = Path.GetDirectoryName(options.OutputPath);
                if (string.IsNullOrEmpty(directory))
                {
                    errors.Add(new ValidationError(nameof(options.OutputPath), "Invalid output path"));
                }
            }
            catch
            {
                errors.Add(new ValidationError(nameof(options.OutputPath), "Invalid output path format"));
            }
        }

        if (options.FontSize < 6 || options.FontSize > 72)
        {
            errors.Add(new ValidationError(nameof(options.FontSize), "Font size must be between 6 and 72"));
        }

        if (options.LineSpacing < 0.5 || options.LineSpacing > 3.0)
        {
            errors.Add(new ValidationError(nameof(options.LineSpacing), "Line spacing must be between 0.5 and 3.0"));
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates an API key format.
    /// </summary>
    public static ValidationResult ValidateApiKey(string? apiKey, string providerName)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ValidationResult.Failure("ApiKey", $"{providerName} API key is required");
        }

        if (apiKey.Length < 10)
        {
            return ValidationResult.Failure("ApiKey", $"{providerName} API key appears to be too short");
        }

        // Provider-specific validation
        if (providerName == "Anthropic" && !apiKey.StartsWith("sk-ant-"))
        {
            return ValidationResult.Failure("ApiKey", "Anthropic API keys should start with 'sk-ant-'");
        }

        if (providerName == "OpenAI" && !apiKey.StartsWith("sk-"))
        {
            return ValidationResult.Failure("ApiKey", "OpenAI API keys should start with 'sk-'");
        }

        return ValidationResult.Success();
    }
}
