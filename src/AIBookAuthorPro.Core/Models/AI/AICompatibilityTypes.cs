// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Enums;

namespace AIBookAuthorPro.Core.Models.AI;

// ============================================================================
// Type aliases for backwards compatibility with Infrastructure layer
// These types alias the standard types for API consistency
// ============================================================================

/// <summary>
/// Alias for GenerationRequest used by AI providers.
/// </summary>
public sealed class AIGenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 4000;
    public bool Stream { get; set; }
    public GenerationContext? Context { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
    
    /// <summary>
    /// Converts from the standard GenerationRequest.
    /// </summary>
    public static AIGenerationRequest FromGenerationRequest(GenerationRequest request) => new()
    {
        Prompt = request.UserPrompt,
        SystemPrompt = request.SystemPrompt,
        Model = request.ModelId ?? string.Empty,
        Temperature = request.Temperature,
        MaxTokens = request.MaxTokens,
        Context = request.Context,
        Metadata = request.Metadata
    };
}

/// <summary>
/// Alias for GenerationResult used by AI providers.
/// </summary>
public sealed class AIGenerationResponse
{
    public string Content { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? FinishReason { get; set; }
    public TokenUsage? Usage { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Converts to the standard GenerationResult.
    /// </summary>
    public GenerationResult ToGenerationResult() => new()
    {
        IsSuccess = true,
        Content = Content,
        ModelUsed = Model,
        FinishReason = FinishReason,
        Usage = Usage
    };
}

/// <summary>
/// Extended AIModelInfo with additional properties used by Infrastructure.
/// Uses composition since AIModelInfo is sealed.
/// </summary>
public sealed class AIModelInfoExtended
{
    public AIModelInfo BaseInfo { get; init; } = null!;
    public bool SupportsVision { get; init; }
    public string Description { get; init; } = string.Empty;
    
    // Convenience properties that delegate to BaseInfo
    public string Id => BaseInfo.ModelId;
    public string Name => BaseInfo.DisplayName;
    public Enums.AIProviderType Provider => BaseInfo.Provider;
    public decimal CostPer1KInputTokens => BaseInfo.InputCostPer1K;
    public decimal CostPer1KOutputTokens => BaseInfo.OutputCostPer1K;
}

/// <summary>
/// Token usage with additional prompt/completion naming.
/// </summary>
public static class TokenUsageExtensions
{
    public static int PromptTokens(this TokenUsage usage) => usage.InputTokens;
    public static int CompletionTokens(this TokenUsage usage) => usage.OutputTokens;
}
