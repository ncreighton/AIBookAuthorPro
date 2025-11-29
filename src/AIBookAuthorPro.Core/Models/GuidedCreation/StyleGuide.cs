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
    public VoiceProfile Voice { get; init; } = new();

    /// <summary>
    /// Prose style guidelines.
    /// </summary>
    public ProseStyle Prose { get; init; } = new();

    /// <summary>
    /// Dialogue guidelines.
    /// </summary>
    public DialogueGuidelines Dialogue { get; init; } = new();

    /// <summary>
    /// Description guidelines.
    /// </summary>
    public DescriptionGuidelines Description { get; init; } = new();

    /// <summary>
    /// Action scene guidelines.
    /// </summary>
    public ActionGuidelines Action { get; init; } = new();

    /// <summary>
    /// Emotional scene guidelines.
    /// </summary>
    public EmotionalGuidelines Emotional { get; init; } = new();

    /// <summary>
    /// Pacing guidelines.
    /// </summary>
    public PacingGuidelines Pacing { get; init; } = new();

    /// <summary>
    /// Genre-specific conventions.
    /// </summary>
    public GenreConventions GenreConventions { get; init; } = new();

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
    public FormattingConventions Formatting { get; init; } = new();
}

/// <summary>
/// Voice profile.
/// </summary>
public sealed record VoiceProfile
{
    /// <summary>
    /// Overall voice description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Point of view (First Person, Third Person Limited, etc.)
    /// </summary>
    public string PointOfView { get; init; } = "Third Person Limited";

    /// <summary>
    /// Tense (Past, Present).
    /// </summary>
    public string Tense { get; init; } = "Past";

    /// <summary>
    /// Tone keywords.
    /// </summary>
    public List<string> ToneKeywords { get; init; } = new();

    /// <summary>
    /// Personality of the narrative voice.
    /// </summary>
    public string NarrativePersonality { get; init; } = string.Empty;

    /// <summary>
    /// Distance from characters (close, medium, distant).
    /// </summary>
    public string NarrativeDistance { get; init; } = string.Empty;

    /// <summary>
    /// Reliability of narrator.
    /// </summary>
    public string NarratorReliability { get; init; } = string.Empty;

    /// <summary>
    /// Humor level and type.
    /// </summary>
    public HumorProfile? Humor { get; init; }

    /// <summary>
    /// Emotional transparency.
    /// </summary>
    public string EmotionalTransparency { get; init; } = string.Empty;

    /// <summary>
    /// Philosophical depth.
    /// </summary>
    public string PhilosophicalDepth { get; init; } = string.Empty;
}

/// <summary>
/// Humor profile.
/// </summary>
public sealed record HumorProfile
{
    /// <summary>
    /// Level (none, light, moderate, heavy).
    /// </summary>
    public string Level { get; init; } = string.Empty;

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
    public string SentenceLength { get; init; } = string.Empty;

    /// <summary>
    /// Sentence style description.
    /// </summary>
    public string SentenceStyle { get; init; } = string.Empty;

    /// <summary>
    /// Sentence variety guidance.
    /// </summary>
    public string SentenceVariety { get; init; } = string.Empty;

    /// <summary>
    /// Paragraph length.
    /// </summary>
    public string ParagraphLength { get; init; } = string.Empty;

    /// <summary>
    /// Vocabulary level.
    /// </summary>
    public string VocabularyLevel { get; init; } = string.Empty;

    /// <summary>
    /// Preferred words to use.
    /// </summary>
    public List<string> PreferredWords { get; init; } = new();

    /// <summary>
    /// Words to avoid.
    /// </summary>
    public List<string> AvoidWords { get; init; } = new();

    /// <summary>
    /// Imagery density.
    /// </summary>
    public string ImageryDensity { get; init; } = string.Empty;

    /// <summary>
    /// Metaphor usage.
    /// </summary>
    public MetaphorGuidelines Metaphors { get; init; } = new();

    /// <summary>
    /// Rhythm and flow notes.
    /// </summary>
    public string Rhythm { get; init; } = string.Empty;

    /// <summary>
    /// Show vs tell ratio guidance.
    /// </summary>
    public string ShowVsTell { get; init; } = string.Empty;

    /// <summary>
    /// Filtering words (felt, saw, heard) usage.
    /// </summary>
    public string FilteringWords { get; init; } = string.Empty;

    /// <summary>
    /// Adverb usage.
    /// </summary>
    public string AdverbUsage { get; init; } = string.Empty;

    /// <summary>
    /// Passive voice usage.
    /// </summary>
    public string PassiveVoice { get; init; } = string.Empty;
}

/// <summary>
/// Metaphor guidelines.
/// </summary>
public sealed record MetaphorGuidelines
{
    /// <summary>
    /// Frequency.
    /// </summary>
    public string Frequency { get; init; } = string.Empty;

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
    public string Metaphor { get; init; } = string.Empty;

    /// <summary>
    /// Meaning.
    /// </summary>
    public string Meaning { get; init; } = string.Empty;

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
    public string DialogueRatio { get; init; } = string.Empty;

    /// <summary>
    /// Alias for DialogueRatio.
    /// </summary>
    public string DialogueToNarrativeRatio { get => DialogueRatio; init => DialogueRatio = value; }

    /// <summary>
    /// Dialogue tag preferences.
    /// </summary>
    public DialogueTagGuidelines Tags { get; init; } = new();

    /// <summary>
    /// Tag style description (convenience).
    /// </summary>
    public string TagStyle { get => Tags?.TagPlacement ?? string.Empty; }

    /// <summary>
    /// Subtext level.
    /// </summary>
    public string SubtextLevel { get; init; } = string.Empty;

    /// <summary>
    /// Interruption style.
    /// </summary>
    public string InterruptionStyle { get; init; } = string.Empty;

    /// <summary>
    /// How to handle accents/dialects.
    /// </summary>
    public string AccentHandling { get; init; } = string.Empty;

    /// <summary>
    /// Exposition in dialogue guidance.
    /// </summary>
    public string ExpositionHandling { get; init; } = string.Empty;

    /// <summary>
    /// Phone/text message formatting.
    /// </summary>
    public string? DigitalCommunication { get; init; }

    /// <summary>
    /// Internal monologue style.
    /// </summary>
    public string InternalMonologue { get; init; } = string.Empty;
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
    public string ActionBeatUsage { get; init; } = string.Empty;

    /// <summary>
    /// Tag placement preferences.
    /// </summary>
    public string TagPlacement { get; init; } = string.Empty;

    /// <summary>
    /// Said/asked frequency.
    /// </summary>
    public string SaidFrequency { get; init; } = string.Empty;
}

/// <summary>
/// Description guidelines.
/// </summary>
public sealed record DescriptionGuidelines
{
    /// <summary>
    /// Setting description depth.
    /// </summary>
    public string SettingDepth { get; init; } = string.Empty;

    /// <summary>
    /// Character description approach.
    /// </summary>
    public string CharacterDescriptionApproach { get; init; } = string.Empty;

    /// <summary>
    /// Sensory balance.
    /// </summary>
    public SensoryBalance SensoryBalance { get; init; } = new();

    /// <summary>
    /// Description pacing.
    /// </summary>
    public string DescriptionPacing { get; init; } = string.Empty;

    /// <summary>
    /// Integration with action.
    /// </summary>
    public string ActionIntegration { get; init; } = string.Empty;

    /// <summary>
    /// World-building integration.
    /// </summary>
    public string WorldBuildingIntegration { get; init; } = string.Empty;
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
    public string SentenceStructure { get; init; } = string.Empty;

    /// <summary>
    /// Detail level.
    /// </summary>
    public string DetailLevel { get; init; } = string.Empty;

    /// <summary>
    /// Choreography clarity.
    /// </summary>
    public string ChoreographyClarity { get; init; } = string.Empty;

    /// <summary>
    /// Violence level.
    /// </summary>
    public string ViolenceLevel { get; init; } = string.Empty;

    /// <summary>
    /// Emotional anchoring.
    /// </summary>
    public string EmotionalAnchoring { get; init; } = string.Empty;

    /// <summary>
    /// Time compression/expansion.
    /// </summary>
    public string TimeManipulation { get; init; } = string.Empty;
}

/// <summary>
/// Emotional scene guidelines.
/// </summary>
public sealed record EmotionalGuidelines
{
    /// <summary>
    /// Emotional depth.
    /// </summary>
    public string EmotionalDepth { get; init; } = string.Empty;

    /// <summary>
    /// Vulnerability level.
    /// </summary>
    public string VulnerabilityLevel { get; init; } = string.Empty;

    /// <summary>
    /// Physical manifestation of emotion.
    /// </summary>
    public string PhysicalManifestation { get; init; } = string.Empty;

    /// <summary>
    /// Restraint vs release.
    /// </summary>
    public string RestraintVsRelease { get; init; } = string.Empty;

    /// <summary>
    /// Romantic content level.
    /// </summary>
    public string RomanticContentLevel { get; init; } = string.Empty;

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
    public string Overall { get; init; } = string.Empty;

    /// <summary>
    /// Scene transition style.
    /// </summary>
    public string SceneTransitions { get; init; } = string.Empty;

    /// <summary>
    /// Chapter ending style.
    /// </summary>
    public string ChapterEndings { get; init; } = string.Empty;

    /// <summary>
    /// Chapter opening style.
    /// </summary>
    public string ChapterOpenings { get; init; } = string.Empty;

    /// <summary>
    /// Cliffhanger usage.
    /// </summary>
    public string CliffhangerUsage { get; init; } = string.Empty;

    /// <summary>
    /// Breather scene guidance.
    /// </summary>
    public string BreatherScenes { get; init; } = string.Empty;
}

/// <summary>
/// Genre conventions.
/// </summary>
public sealed record GenreConventions
{
    /// <summary>
    /// Primary genre.
    /// </summary>
    public string PrimaryGenre { get; init; } = string.Empty;

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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// How to implement.
    /// </summary>
    public string Implementation { get; init; } = string.Empty;
}

/// <summary>
/// Style sample.
/// </summary>
public sealed record StyleSample
{
    /// <summary>
    /// Category (opening, dialogue, action, etc.).
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// The sample text.
    /// </summary>
    public string Sample { get; init; } = string.Empty;

    /// <summary>
    /// What it demonstrates.
    /// </summary>
    public string Demonstrates { get; init; } = string.Empty;

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
    public string SceneBreakMarker { get; init; } = string.Empty;

    /// <summary>
    /// Thought formatting.
    /// </summary>
    public string ThoughtFormatting { get; init; } = string.Empty;

    /// <summary>
    /// Emphasis formatting.
    /// </summary>
    public string EmphasisFormatting { get; init; } = string.Empty;

    /// <summary>
    /// Letter/document formatting.
    /// </summary>
    public string? DocumentFormatting { get; init; }

    /// <summary>
    /// Flashback indication.
    /// </summary>
    public string FlashbackIndication { get; init; } = string.Empty;

    /// <summary>
    /// Time skip indication.
    /// </summary>
    public string TimeSkipIndication { get; init; } = string.Empty;
}
