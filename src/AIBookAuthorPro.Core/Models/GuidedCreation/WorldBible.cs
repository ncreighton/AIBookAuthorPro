// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete world bible containing all world-building information.
/// </summary>
public sealed record WorldBible
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// World overview.
    /// </summary>
    public WorldOverview Overview { get; init; } = new();

    /// <summary>
    /// All locations.
    /// </summary>
    public List<LocationProfile> Locations { get; init; } = new();

    /// <summary>
    /// Time period details.
    /// </summary>
    public TimePeriodDetails TimePeriod { get; init; } = new();

    /// <summary>
    /// Story timeline.
    /// </summary>
    public StoryTimeline Timeline { get; init; } = new();

    /// <summary>
    /// Cultural details.
    /// </summary>
    public List<CultureProfile> Cultures { get; init; } = new();

    /// <summary>
    /// Political structures if relevant.
    /// </summary>
    public PoliticalStructure? Politics { get; init; }

    /// <summary>
    /// Magic/power system if applicable.
    /// </summary>
    public MagicSystem? MagicSystem { get; init; }

    /// <summary>
    /// World rules and constraints.
    /// </summary>
    public List<string> Rules { get; init; } = new();

    /// <summary>
    /// Technology level and details.
    /// </summary>
    public TechnologyLevel? Technology { get; init; }

    /// <summary>
    /// Economy details if relevant.
    /// </summary>
    public EconomyDetails? Economy { get; init; }

    /// <summary>
    /// Religion/belief systems.
    /// </summary>
    public List<ReligionProfile> Religions { get; init; } = new();

    /// <summary>
    /// History relevant to the story.
    /// </summary>
    public List<HistoricalEvent> RelevantHistory { get; init; } = new();

    /// <summary>
    /// Flora and fauna if relevant.
    /// </summary>
    public FloraFauna? FloraFauna { get; init; }

    /// <summary>
    /// World rules (physics, magic, etc.).
    /// </summary>
    public List<WorldRule> WorldRules { get; init; } = new();

    /// <summary>
    /// Terminology/glossary.
    /// </summary>
    public List<GlossaryEntry> Glossary { get; init; } = new();

    /// <summary>
    /// Sensory palette for the world.
    /// </summary>
    public SensoryPalette SensoryPalette { get; init; } = new();
}

/// <summary>
/// World overview.
/// </summary>
public sealed record WorldOverview
{
    /// <summary>
    /// World name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// High-level description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Type of world.
    /// </summary>
    public string WorldType { get; init; } = string.Empty;

    /// <summary>
    /// Scale (city, country, world, universe).
    /// </summary>
    public string Scale { get; init; } = string.Empty;

    /// <summary>
    /// Tone/atmosphere.
    /// </summary>
    public string Atmosphere { get; init; } = string.Empty;

    /// <summary>
    /// Key differentiators from real world.
    /// </summary>
    public List<string> KeyDifferences { get; init; } = new();

    /// <summary>
    /// What makes this world unique.
    /// </summary>
    public string UniqueElements { get; init; } = string.Empty;
}

/// <summary>
/// Location profile.
/// </summary>
public sealed class LocationProfile
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Location name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of location.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Parent location if nested.
    /// </summary>
    public Guid? ParentLocationId { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Significance to the story.
    /// </summary>
    public string Significance { get; init; } = string.Empty;

    /// <summary>
    /// Atmosphere/mood.
    /// </summary>
    public string Atmosphere { get; init; } = string.Empty;

    /// <summary>
    /// Sensory details.
    /// </summary>
    public List<SensoryDetail> SensoryDetails { get; init; } = new();

    /// <summary>
    /// Key features.
    /// </summary>
    public List<string> KeyFeatures { get; init; } = new();

    /// <summary>
    /// Who lives/frequents here.
    /// </summary>
    public List<Guid> AssociatedCharacters { get; init; } = new();

    /// <summary>
    /// Chapters where this location appears.
    /// </summary>
    public List<int> ChapterAppearances { get; init; } = new();

    /// <summary>
    /// History of this location.
    /// </summary>
    public string? History { get; init; }

    /// <summary>
    /// Dangers or challenges here.
    /// </summary>
    public List<string> Dangers { get; init; } = new();

    /// <summary>
    /// Secrets of this location.
    /// </summary>
    public List<string> Secrets { get; init; } = new();
}

/// <summary>
/// Time period details.
/// </summary>
public sealed record TimePeriodDetails
{
    /// <summary>
    /// Era name.
    /// </summary>
    public string Era { get; init; } = string.Empty;

    /// <summary>
    /// Time period description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Year or date if applicable.
    /// </summary>
    public string? SpecificTime { get; init; }

    /// <summary>
    /// Key characteristics of this period.
    /// </summary>
    public List<string> KeyCharacteristics { get; init; } = new();

    /// <summary>
    /// Social norms.
    /// </summary>
    public List<string> SocialNorms { get; init; } = new();

    /// <summary>
    /// Fashion/dress.
    /// </summary>
    public string Fashion { get; init; } = string.Empty;

    /// <summary>
    /// Transportation.
    /// </summary>
    public string Transportation { get; init; } = string.Empty;

    /// <summary>
    /// Communication methods.
    /// </summary>
    public string Communication { get; init; } = string.Empty;
}

/// <summary>
/// Story timeline.
/// </summary>
public sealed record StoryTimeline
{
    /// <summary>
    /// Total duration of story.
    /// </summary>
    public string TotalDuration { get; init; } = string.Empty;

    /// <summary>
    /// When story begins.
    /// </summary>
    public string StartPoint { get; init; } = string.Empty;

    /// <summary>
    /// When story ends.
    /// </summary>
    public string EndPoint { get; init; } = string.Empty;

    /// <summary>
    /// Key timeline events.
    /// </summary>
    public List<TimelineEvent> Events { get; init; } = new();

    /// <summary>
    /// Time jumps in the narrative.
    /// </summary>
    public List<TimeJump> TimeJumps { get; init; } = new();
}

/// <summary>
/// Timeline event.
/// </summary>
public sealed record TimelineEvent
{
    /// <summary>
    /// Story day.
    /// </summary>
    public int StoryDay { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Chapter where this occurs.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Significance.
    /// </summary>
    public string Significance { get; init; } = string.Empty;
}

/// <summary>
/// Time jump in narrative.
/// </summary>
public sealed record TimeJump
{
    /// <summary>
    /// From chapter.
    /// </summary>
    public int FromChapter { get; init; }

    /// <summary>
    /// To chapter.
    /// </summary>
    public int ToChapter { get; init; }

    /// <summary>
    /// Duration jumped.
    /// </summary>
    public string Duration { get; init; } = string.Empty;

    /// <summary>
    /// What happens during the gap.
    /// </summary>
    public string GapSummary { get; init; } = string.Empty;
}

/// <summary>
/// Culture profile.
/// </summary>
public sealed record CultureProfile
{
    /// <summary>
    /// Culture name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Values.
    /// </summary>
    public List<string> Values { get; init; } = new();

    /// <summary>
    /// Customs.
    /// </summary>
    public List<string> Customs { get; init; } = new();

    /// <summary>
    /// Taboos.
    /// </summary>
    public List<string> Taboos { get; init; } = new();

    /// <summary>
    /// Language notes.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Characters from this culture.
    /// </summary>
    public List<Guid> AssociatedCharacters { get; init; } = new();
}

/// <summary>
/// Political structure.
/// </summary>
public sealed record PoliticalStructure
{
    /// <summary>
    /// System type.
    /// </summary>
    public string SystemType { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Power centers.
    /// </summary>
    public List<PowerCenter> PowerCenters { get; init; } = new();

    /// <summary>
    /// Current political tensions.
    /// </summary>
    public List<string> Tensions { get; init; } = new();

    /// <summary>
    /// Laws relevant to the story.
    /// </summary>
    public List<string> RelevantLaws { get; init; } = new();
}

/// <summary>
/// Power center.
/// </summary>
public sealed record PowerCenter
{
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of power.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Influence level.
    /// </summary>
    public string Influence { get; init; } = string.Empty;

    /// <summary>
    /// Associated characters.
    /// </summary>
    public List<Guid> AssociatedCharacters { get; init; } = new();
}

/// <summary>
/// Magic system definition.
/// </summary>
public sealed record MagicSystem
{
    /// <summary>
    /// System name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Hardness (hard/soft magic spectrum).
    /// </summary>
    public string Hardness { get; init; } = string.Empty;

    /// <summary>
    /// Source of magic.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Rules.
    /// </summary>
    public List<string> Rules { get; init; } = new();

    /// <summary>
    /// Limitations.
    /// </summary>
    public List<string> Limitations { get; init; } = new();

    /// <summary>
    /// Costs.
    /// </summary>
    public List<string> Costs { get; init; } = new();

    /// <summary>
    /// Who can use it.
    /// </summary>
    public string Practitioners { get; init; } = string.Empty;

    /// <summary>
    /// Abilities/powers.
    /// </summary>
    public List<MagicAbility> Abilities { get; init; } = new();

    /// <summary>
    /// Social perception of magic.
    /// </summary>
    public string SocialPerception { get; init; } = string.Empty;
}

/// <summary>
/// Magic ability.
/// </summary>
public sealed record MagicAbility
{
    /// <summary>
    /// Ability name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Cost to use.
    /// </summary>
    public string Cost { get; init; } = string.Empty;

    /// <summary>
    /// Limitations.
    /// </summary>
    public List<string> Limitations { get; init; } = new();
}

/// <summary>
/// Technology level.
/// </summary>
public sealed record TechnologyLevel
{
    /// <summary>
    /// Overall level.
    /// </summary>
    public string Level { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Key technologies.
    /// </summary>
    public List<Technology> KeyTechnologies { get; init; } = new();

    /// <summary>
    /// Anachronisms if any.
    /// </summary>
    public List<string> Anachronisms { get; init; } = new();
}

/// <summary>
/// Technology.
/// </summary>
public sealed record Technology
{
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Impact on society.
    /// </summary>
    public string SocialImpact { get; init; } = string.Empty;

    /// <summary>
    /// Relevance to story.
    /// </summary>
    public string StoryRelevance { get; init; } = string.Empty;
}

/// <summary>
/// Economy details.
/// </summary>
public sealed record EconomyDetails
{
    /// <summary>
    /// Economic system.
    /// </summary>
    public string System { get; init; } = string.Empty;

    /// <summary>
    /// Currency.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Major industries.
    /// </summary>
    public List<string> Industries { get; init; } = new();

    /// <summary>
    /// Trade notes.
    /// </summary>
    public string? Trade { get; init; }

    /// <summary>
    /// Wealth distribution.
    /// </summary>
    public string WealthDistribution { get; init; } = string.Empty;
}

/// <summary>
/// Religion profile.
/// </summary>
public sealed record ReligionProfile
{
    /// <summary>
    /// Religion name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Beliefs.
    /// </summary>
    public List<string> Beliefs { get; init; } = new();

    /// <summary>
    /// Practices.
    /// </summary>
    public List<string> Practices { get; init; } = new();

    /// <summary>
    /// Social role.
    /// </summary>
    public string SocialRole { get; init; } = string.Empty;

    /// <summary>
    /// Followers.
    /// </summary>
    public List<Guid> Followers { get; init; } = new();
}

/// <summary>
/// Historical event.
/// </summary>
public sealed record HistoricalEvent
{
    /// <summary>
    /// Event name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// When it happened.
    /// </summary>
    public string When { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Impact on present.
    /// </summary>
    public string ImpactOnPresent { get; init; } = string.Empty;

    /// <summary>
    /// Story relevance.
    /// </summary>
    public string StoryRelevance { get; init; } = string.Empty;
}

/// <summary>
/// Flora and fauna.
/// </summary>
public sealed record FloraFauna
{
    /// <summary>
    /// Notable plants.
    /// </summary>
    public List<WorldCreature> Plants { get; init; } = new();

    /// <summary>
    /// Notable animals.
    /// </summary>
    public List<WorldCreature> Animals { get; init; } = new();

    /// <summary>
    /// Mythical creatures.
    /// </summary>
    public List<WorldCreature> MythicalCreatures { get; init; } = new();
}

/// <summary>
/// World creature or plant.
/// </summary>
public sealed record WorldCreature
{
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Story relevance.
    /// </summary>
    public string? StoryRelevance { get; init; }
}

/// <summary>
/// World rule.
/// </summary>
public sealed record WorldRule
{
    /// <summary>
    /// Rule name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Rule description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Consequences of breaking.
    /// </summary>
    public string? Consequences { get; init; }

    /// <summary>
    /// Exceptions.
    /// </summary>
    public List<string> Exceptions { get; init; } = new();
}

/// <summary>
/// Glossary entry.
/// </summary>
public sealed record GlossaryEntry
{
    /// <summary>
    /// Term.
    /// </summary>
    public string Term { get; init; } = string.Empty;

    /// <summary>
    /// Definition.
    /// </summary>
    public string Definition { get; init; } = string.Empty;

    /// <summary>
    /// Usage examples.
    /// </summary>
    public List<string> UsageExamples { get; init; } = new();

    /// <summary>
    /// First appearance chapter.
    /// </summary>
    public int? FirstAppearanceChapter { get; init; }
}

/// <summary>
/// Sensory palette for the world.
/// </summary>
public sealed record SensoryPalette
{
    /// <summary>
    /// Common sights.
    /// </summary>
    public List<string> Sights { get; init; } = new();

    /// <summary>
    /// Common sounds.
    /// </summary>
    public List<string> Sounds { get; init; } = new();

    /// <summary>
    /// Common smells.
    /// </summary>
    public List<string> Smells { get; init; } = new();

    /// <summary>
    /// Common tastes.
    /// </summary>
    public List<string> Tastes { get; init; } = new();

    /// <summary>
    /// Common textures.
    /// </summary>
    public List<string> Textures { get; init; } = new();

    /// <summary>
    /// Weather patterns.
    /// </summary>
    public List<string> Weather { get; init; } = new();

    /// <summary>
    /// Seasonal variations.
    /// </summary>
    public List<string> Seasons { get; init; } = new();
}
