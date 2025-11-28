// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// Wizard session entity for persisting guided creation progress.
/// </summary>
public class WizardSessionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? BookProjectId { get; set; }  // Set once project is created
    public string Status { get; set; } = "InProgress";  // InProgress, Paused, Completed, Cancelled
    public string? CurrentStep { get; set; }
    public int StepNumber { get; set; }

    // Wizard data stored as JSON
    public string? SeedPromptJson { get; set; }
    public string? AnalysisResultJson { get; set; }
    public string? ExpandedBriefJson { get; set; }
    public string? ClarificationsJson { get; set; }
    public string? ClarificationResponsesJson { get; set; }
    public string? BlueprintJson { get; set; }
    public string? ConfigurationJson { get; set; }
    public string? GenerationSessionJson { get; set; }
    public string? StepHistoryJson { get; set; }

    // Progress tracking
    public bool BlueprintApproved { get; set; }
    public double ProgressPercentage { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public virtual UserEntity? User { get; set; }
    public virtual BookProjectEntity? BookProject { get; set; }
}
