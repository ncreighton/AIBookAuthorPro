// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

/// <summary>
/// Request for chapter generation through the pipeline service.
/// </summary>
public sealed class ChapterGenerationPipelineRequest
{
    /// <summary>
    /// Gets or sets the project containing the chapter.
    /// </summary>
    public required Project Project { get; init; }

    /// <summary>
    /// Gets or sets the chapter to generate.
    /// </summary>
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Gets or sets the generation mode.
    /// </summary>
    public GenerationMode Mode { get; init; } = GenerationMode.Standard;

    /// <summary>
    /// Gets or sets the AI provider to use.
    /// </summary>
    public AIProviderType Provider { get; init; } = AIProviderType.Claude;

    /// <summary>
    /// Gets or sets the specific model ID (optional, auto-selected based on mode if not set).
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets or sets the temperature (creativity) setting.
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Gets or sets custom instructions for generation.
    /// </summary>
    public string? CustomInstructions { get; init; }

    /// <summary>
    /// Gets or sets whether to include character context.
    /// </summary>
    public bool IncludeCharacterContext { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include location context.
    /// </summary>
    public bool IncludeLocationContext { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include previous chapter summary.
    /// </summary>
    public bool IncludePreviousSummary { get; init; } = true;
}
