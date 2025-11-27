// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Enums;

/// <summary>
/// Defines the generation modes for AI content creation.
/// </summary>
public enum GenerationMode
{
    /// <summary>
    /// Quick generation using fast models (Haiku, GPT-4o-mini, Flash).
    /// Single pass, fastest output, lowest cost.
    /// </summary>
    Quick = 0,

    /// <summary>
    /// Standard generation using balanced models (Sonnet, GPT-4o, 1.5 Pro).
    /// Single pass, good quality, moderate cost.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Premium generation using best models (Opus, o1-preview, 2.0 Flash).
    /// Multi-pass refinement, highest quality, highest cost.
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Custom generation with user-specified settings.
    /// </summary>
    Custom = 3
}
