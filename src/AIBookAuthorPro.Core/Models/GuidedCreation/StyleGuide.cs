// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete style guide for consistent writing throughout the book.
/// </summary>
public sealed record StyleGuide
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Overall voice description.
    /// </summary>
    public required VoiceProfile Voice { get; init; }

    /// <summary>
    /// Prose style guidelines.
    /// </summary>
    public required ProseStyle Prose { get; init; }

    /// <summary>
    /// Dialogue guidelines.
    /// </summary>
    public required DialogueGuidelines Dialogue { get; init; }

    /// <summary>
    /// Description guidelines.
    /// </summary>
    public required DescriptionGuidelines Description { get; init; }

    /// <summary>
    /// Action scene guidelines.
    /// </summary>
    public required ActionGuidelines Action { get; init; }

    /// <summary>
    /// Emotional scene guidelines.
    /// </summary>
    public required EmotionalGuidelines Emotional { get; init; }

    /// <summary>
    /// Pacing guidelines.
    /// </summary>
    public required PacingGuidelines Pacing { get; init; }

    /// <summary>
    /// Genre-specific conventions.
    /// </summary>
    public required GenreConventions GenreConventions { get; init; }

    /// <summary>
    /// Sample passages demonstrating the style.
    /// </summary>
    public List<StyleSample> Samples { get; init; } = new();

    /// <summary>
    /// Words/phrases to use frequently.
    /// </summary>
    public List<string> PreferredVocabulary { get; init; } = new();

    /// <summary>
    /// Words/phrases to avoid.
    /// </summary>
    public List<string> AvoidVocabulary { get; init; } = new();

    /// <summary>
    /// Formatting conventions.
    /// </summary>
    public required FormattingConventions Formatting { get; init; }
}

/// <summary>
/// Voice profile.
/// </summary>
public sealed record VoiceProfile
{
    /// <summary>
    /// Overall voice description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Tone keywords.
    /// </summary>
    public List<string> ToneKeywords { get; init; } = new();

    /// <summary>
    /// Personality of the narrative voice.
    /// </summary>
    public required string NarrativePersonality { get; init; }

    /// <summary>
    /// Distance from characters (close, medium, distant).
    /// </summary>
    public required string NarrativeDistance { get; init; }

    /// <summary>
    /// Reliability of narrator.
    /// </summary>
    public required string NarratorReliability { get; init; }

    /// <summary>
    /// Humor level and type.
    /// </summary>
    public HumorProfile? Humor { get; init; }

    /// <summary>
    /// Emotional transparency.
    /// </summary>
    public required string EmotionalTransparency { get; init; }

    /// <summary>
    /// Philosophical depth.
    /// </summary>
    public required string PhilosophicalDepth { get; init; }
}

/// <summary>
/// Humor profile.
/// </summary>
public sealed record HumorProfile
{
    /// <summary>
    /// Level (none, light, moderate, heavy).
    /// </summary>
    public required string Level { get; init; }

    /// <summary>
    /// Types of humor used.
    /// </summary>
    public List<string> Types { get; init; } = new();

    /// <summary>
    /// When humor is appropriate.
    /// </summary>
    public List<string> AppropriateContexts { get; init; } = new();

    /// <summary>
    /// When to avoid humor.
    /// </summary>
    public List<string> InappropriateContexts { get; init; } = new();
}

/// <summary>
/// Prose style guidelines.
/// </summary>
public sealed record ProseStyle
{
    /// <summary>
    /// Sentence length preference.
    /// </summary>
    public required string SentenceLength { get; init; }

    /// <summary>
    /// Sentence variety guidance.
    /// </summary>
    public required string SentenceVariety { get; init; }

    /// <summary>
    /// Paragraph length.
    /// </summary>
    public required string ParagraphLength { get; init; }

    /// <summary>
    /// Vocabulary level.
    /// </summary>
    public required string VocabularyLevel { get; init; }

    /// <summary>
    /// Imagery density.
    /// </summary>
    public required string ImageryDensity { get; init; }

    /// <summary>
    /// Metaphor usage.
    /// </summary>
    public required MetaphorGuidelines Metaphors { get; init; }

    /// <summary>
    /// Rhythm and flow notes.
    /// </summary>
    public required string Rhythm { get; init; }

    /// <summary>
    /// Show vs tell ratio guidance.
    /// </summary>
    public required string ShowVsTell { get; init; }

    /// <summary>
    /// Filtering words (felt, saw, heard) usage.
    /// </summary>
    public required string FilteringWords { get; init; }

    /// <summary>
    /// Adverb usage.
    /// </summary>
    public required string AdverbUsage { get; init; }

    /// <summary>
    /// Passive voice usage.
    /// </summary>
    public required string PassiveVoice { get; init; }
}

/// <summary>
/// Metaphor guidelines.
/// </summary>
public sealed record MetaphorGuidelines
{
    /// <summary>
    /// Frequency.
    /// </summary>
    public required string Frequency { get; init; }

    /// <summary>
    /// Types preferred.
    /// </summary>
    public List<string> PreferredTypes { get; init; } = new();

    /// <summary>
    /// Source domains.
    /// </summary>
    public List<string> SourceDomains { get; init; } = new();

    /// <summary>
    /// Extended metaphors to use.
    /// </summary>
    public List<ExtendedMetaphor> ExtendedMetaphors { get; init; } = new();
}

/// <summary>
/// Extended metaphor.
/// </summary>
public sealed record ExtendedMetaphor
{
    /// <summary>
    /// The metaphor.
    /// </summary>
    public required string Metaphor { get; init; }

    /// <summary>
    /// Meaning.
    /// </summary>
    public required string Meaning { get; init; }

    /// <summary>
    /// Where it appears.
    /// </summary>
    public List<int> ChapterAppearances { get; init; } = new();
}

/// <summary>
/// Dialogue guidelines.
/// </summary>
public sealed record DialogueGuidelines
{
    /// <summary>
    /// Dialogue to narrative ratio.
    /// </summary>
    public required string DialogueRatio { get; init; }

    /// <summary>
    /// Dialogue tag preferences.
    /// </summary>
    public required DialogueTagGuidelines Tags { get; init; }

    /// <summary>
    /// Subtext level.
    /// </summary>
    public required string SubtextLevel { get; init; }

    /// <summary>
    /// Interruption style.
    /// </summary>
    public required string InterruptionStyle { get; init; }

    /// <summary>
    /// How to handle accents/dialects.
    /// </summary>
    public required string AccentHandling { get; init; }

    /// <summary>
    /// Exposition in dialogue guidance.
    /// </summary>
    public required string ExpositionHandling { get; init; }

    /// <summary>
    /// Phone/text message formatting.
    /// </summary>
    public string? DigitalCommunication { get; init; }

    /// <summary>
    /// Internal monologue style.
    /// </summary>
    public required string InternalMonologue { get; init; }
}

/// <summary>
/// Dialogue tag guidelines.
/// </summary>
public sealed record DialogueTagGuidelines
{
    /// <summary>
    /// Preferred simple tags.
    /// </summary>
    public List<string> PreferredSimpleTags { get; init; } = new();

    /// <summary>
    /// When to use action beats instead.
    /// </summary>
    public required string ActionBeatUsage { get; init; }

    /// <summary>
    /// Tag placement preferences.
    /// </summary>
    public required string TagPlacement { get; init; }

    /// <summary>
    /// Said/asked frequency.
    /// </summary>
    public required string SaidFrequency { get; init; }
}

/// <summary>
/// Description guidelines.
/// </summary>
public sealed record DescriptionGuidelines
{
    /// <summary>
    /// Setting description depth.
    /// </summary>
    public required string SettingDepth { get; init; }

    /// <summary>
    /// Character description approach.
    /// </summary>
    public required string CharacterDescriptionApproach { get; init; }

    /// <summary>
    /// Sensory balance.
    /// </summary>
    public required SensoryBalance SensoryBalance { get; init; }

    /// <summary>
    /// Description pacing.
    /// </summary>
    public required string DescriptionPacing { get; init; }

    /// <summary>
    /// Integration with action.
    /// </summary>
    public required string ActionIntegration { get; init; }

    /// <summary>
    /// World-building integration.
    /// </summary>
    public required string WorldBuildingIntegration { get; init; }
}

/// <summary>
/// Sensory balance.
/// </summary>
public sealed record SensoryBalance
{
    /// <summary>
    /// Visual emphasis (1-10).
    /// </summary>
    public int Visual { get; init; }

    /// <summary>
    /// Auditory emphasis.
    /// </summary>
    public int Auditory { get; init; }

    /// <summary>
    /// Olfactory emphasis.
    /// </summary>
    public int Olfactory { get; init; }

    /// <summary>
    /// Tactile emphasis.
    /// </summary>
    public int Tactile { get; init; }

    /// <summary>
    /// Gustatory emphasis.
    /// </summary>
    public int Gustatory { get; init; }

    /// <summary>
    /// Special notes.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Action scene guidelines.
/// </summary>
public sealed record ActionGuidelines
{
    /// <summary>
    /// Sentence structure in action.
    /// </summary>
    public required string SentenceStructure { get; init; }

    /// <summary>
    /// Detail level.
    /// </summary>
    public required string DetailLevel { get; init; }

    /// <summary>
    /// Choreography clarity.
    /// </summary>
    public required string ChoreographyClarity { get; init; }

    /// <summary>
    /// Violence level.
    /// </summary>
    public required string ViolenceLevel { get; init; }

    /// <summary>
    /// Emotional anchoring.
    /// </summary>
    public required string EmotionalAnchoring { get; init; }

    /// <summary>
    /// Time compression/expansion.
    /// </summary>
    public required string TimeManipulation { get; init; }
}

/// <summary>
/// Emotional scene guidelines.
/// </summary>
public sealed record EmotionalGuidelines
{
    /// <summary>
    /// Emotional depth.
    /// </summary>
    public required string EmotionalDepth { get; init; }

    /// <summary>
    /// Vulnerability level.
    /// </summary>
    public required string VulnerabilityLevel { get; init; }

    /// <summary>
    /// Physical manifestation of emotion.
    /// </summary>
    public required string PhysicalManifestation { get; init; }

    /// <summary>
    /// Restraint vs release.
    /// </summary>
    public required string RestraintVsRelease { get; init; }

    /// <summary>
    /// Romantic content level.
    /// </summary>
    public required string RomanticContentLevel { get; init; }

    /// <summary>
    /// Heat level if applicable.
    /// </summary>
    public string? HeatLevel { get; init; }
}

/// <summary>
/// Pacing guidelines.
/// </summary>
public sealed record PacingGuidelines
{
    /// <summary>
    /// Overall pacing description.
    /// </summary>
    public required string Overall { get; init; }

    /// <summary>
    /// Scene transition style.
    /// </summary>
    public required string SceneTransitions { get; init; }

    /// <summary>
    /// Chapter ending style.
    /// </summary>
    public required string ChapterEndings { get; init; }

    /// <summary>
    /// Chapter opening style.
    /// </summary>
    public required string ChapterOpenings { get; init; }

    /// <summary>
    /// Cliffhanger usage.
    /// </summary>
    public required string CliffhangerUsage { get; init; }

    /// <summary>
    /// Breather scene guidance.
    /// </summary>
    public required string BreatherScenes { get; init; }
}

/// <summary>
/// Genre conventions.
/// </summary>
public sealed record GenreConventions
{
    /// <summary>
    /// Primary genre.
    /// </summary>
    public required string PrimaryGenre { get; init; }

    /// <summary>
    /// Conventions to follow.
    /// </summary>
    public List<GenreConvention> ConventionsToFollow { get; init; } = new();

    /// <summary>
    /// Conventions to subvert.
    /// </summary>
    public List<GenreConvention> ConventionsToSubvert { get; init; } = new();

    /// <summary>
    /// Reader expectations to meet.
    /// </summary>
    public List<string> ReaderExpectations { get; init; } = new();

    /// <summary>
    /// Unique twists on genre.
    /// </summary>
    public List<string> UniqueTwists { get; init; } = new();
}

/// <summary>
/// Genre convention.
/// </summary>
public sealed record GenreConvention
{
    /// <summary>
    /// Convention name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// How to implement.
    /// </summary>
    public required string Implementation { get; init; }
}

/// <summary>
/// Style sample.
/// </summary>
public sealed record StyleSample
{
    /// <summary>
    /// Category (opening, dialogue, action, etc.).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// The sample text.
    /// </summary>
    public required string Sample { get; init; }

    /// <summary>
    /// What it demonstrates.
    /// </summary>
    public required string Demonstrates { get; init; }

    /// <summary>
    /// Notes.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Formatting conventions.
/// </summary>
public sealed record FormattingConventions
{
    /// <summary>
    /// Scene break marker.
    /// </summary>
    public required string SceneBreakMarker { get; init; }

    /// <summary>
    /// Thought formatting.
    /// </summary>
    public required string ThoughtFormatting { get; init; }

    /// <summary>
    /// Emphasis formatting.
    /// </summary>
    public required string EmphasisFormatting { get; init; }

    /// <summary>
    /// Letter/document formatting.
    /// </summary>
    public string? DocumentFormatting { get; init; }

    /// <summary>
    /// Flashback indication.
    /// </summary>
    public required string FlashbackIndication { get; init; }

    /// <summary>
    /// Time skip indication.
    /// </summary>
    public required string TimeSkipIndication { get; init; }
}
