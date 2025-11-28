// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Models.GuidedCreation;

/// <summary>
/// Complete character bible containing all character information for the book.
/// </summary>
public sealed record CharacterBible
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Main/protagonist characters.
    /// </summary>
    public List<CharacterProfile> MainCharacters { get; init; } = new();

    /// <summary>
    /// Supporting characters with significant roles.
    /// </summary>
    public List<CharacterProfile> SupportingCharacters { get; init; } = new();

    /// <summary>
    /// Minor/background characters.
    /// </summary>
    public List<CharacterProfile> MinorCharacters { get; init; } = new();

    /// <summary>
    /// Character relationship webs.
    /// </summary>
    public List<CharacterRelationshipWeb> RelationshipWebs { get; init; } = new();

    /// <summary>
    /// Character arc map showing all arcs.
    /// </summary>
    public required CharacterArcMap ArcMap { get; init; }

    /// <summary>
    /// Character groups/factions.
    /// </summary>
    public List<CharacterGroup> Groups { get; init; } = new();

    /// <summary>
    /// Naming conventions used.
    /// </summary>
    public NamingConventions? NamingConventions { get; init; }
}

/// <summary>
/// Complete character profile.
/// </summary>
public sealed class CharacterProfile
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Full name.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Preferred name (how they're usually referred to).
    /// </summary>
    public required string PreferredName { get; init; }

    /// <summary>
    /// Aliases, nicknames, titles.
    /// </summary>
    public List<string> Aliases { get; init; } = new();

    /// <summary>
    /// Role in the story.
    /// </summary>
    public required CharacterRole Role { get; init; }

    /// <summary>
    /// Character archetype.
    /// </summary>
    public required string Archetype { get; init; }

    /// <summary>
    /// One-line character concept.
    /// </summary>
    public required string Concept { get; init; }

    // ================== PHYSICAL DESCRIPTION ==================

    /// <summary>
    /// Physical description.
    /// </summary>
    public required PhysicalDescription Physical { get; init; }

    // ================== PSYCHOLOGICAL PROFILE ==================

    /// <summary>
    /// Psychological profile.
    /// </summary>
    public required PsychologicalProfile Psychology { get; init; }

    // ================== BACKGROUND ==================

    /// <summary>
    /// Character background.
    /// </summary>
    public required CharacterBackground Background { get; init; }

    // ================== VOICE & MANNERISMS ==================

    /// <summary>
    /// Voice and speaking patterns.
    /// </summary>
    public required CharacterVoice Voice { get; init; }

    // ================== CHARACTER ARC ==================

    /// <summary>
    /// Character arc definition.
    /// </summary>
    public required CharacterArc Arc { get; init; }

    // ================== STORY FUNCTION ==================

    /// <summary>
    /// Function in the story.
    /// </summary>
    public required string StoryFunction { get; init; }

    /// <summary>
    /// Thematic significance.
    /// </summary>
    public required string ThematicRole { get; init; }

    /// <summary>
    /// Chapters this character appears in.
    /// </summary>
    public List<int> ChapterAppearances { get; init; } = new();

    /// <summary>
    /// First introduction chapter.
    /// </summary>
    public required int IntroductionChapter { get; init; }

    // ================== WRITING GUIDELINES ==================

    /// <summary>
    /// Guidelines for writing this character.
    /// </summary>
    public required CharacterWritingGuidelines WritingGuidelines { get; init; }

    // ================== RELATIONSHIPS ==================

    /// <summary>
    /// Relationships with other characters.
    /// </summary>
    public List<CharacterRelationship> Relationships { get; init; } = new();

    /// <summary>
    /// User notes on this character.
    /// </summary>
    public List<string> Notes { get; init; } = new();
}

/// <summary>
/// Physical description of a character.
/// </summary>
public sealed record PhysicalDescription
{
    /// <summary>
    /// Age or apparent age.
    /// </summary>
    public required string Age { get; init; }

    /// <summary>
    /// Gender.
    /// </summary>
    public required string Gender { get; init; }

    /// <summary>
    /// Height description.
    /// </summary>
    public required string Height { get; init; }

    /// <summary>
    /// Build/body type.
    /// </summary>
    public required string Build { get; init; }

    /// <summary>
    /// Hair description.
    /// </summary>
    public required string Hair { get; init; }

    /// <summary>
    /// Eye description.
    /// </summary>
    public required string Eyes { get; init; }

    /// <summary>
    /// Skin description.
    /// </summary>
    public required string Skin { get; init; }

    /// <summary>
    /// Distinguishing features.
    /// </summary>
    public List<string> DistinguishingFeatures { get; init; } = new();

    /// <summary>
    /// Typical clothing/style.
    /// </summary>
    public required string TypicalAttire { get; init; }

    /// <summary>
    /// How they carry themselves.
    /// </summary>
    public required string Bearing { get; init; }

    /// <summary>
    /// First impression they give.
    /// </summary>
    public required string FirstImpression { get; init; }

    /// <summary>
    /// Physical quirks or habits.
    /// </summary>
    public List<string> PhysicalQuirks { get; init; } = new();
}

/// <summary>
/// Psychological profile of a character.
/// </summary>
public sealed record PsychologicalProfile
{
    /// <summary>
    /// Core personality traits.
    /// </summary>
    public List<string> CoreTraits { get; init; } = new();

    /// <summary>
    /// Personality type (e.g., MBTI, Enneagram).
    /// </summary>
    public string? PersonalityType { get; init; }

    /// <summary>
    /// Strengths.
    /// </summary>
    public List<string> Strengths { get; init; } = new();

    /// <summary>
    /// Weaknesses/flaws.
    /// </summary>
    public List<string> Weaknesses { get; init; } = new();

    /// <summary>
    /// Fears.
    /// </summary>
    public List<string> Fears { get; init; } = new();

    /// <summary>
    /// Desires.
    /// </summary>
    public List<string> Desires { get; init; } = new();

    /// <summary>
    /// Values.
    /// </summary>
    public List<string> Values { get; init; } = new();

    /// <summary>
    /// Beliefs.
    /// </summary>
    public List<string> Beliefs { get; init; } = new();

    /// <summary>
    /// Moral alignment.
    /// </summary>
    public required string MoralAlignment { get; init; }

    /// <summary>
    /// Emotional patterns.
    /// </summary>
    public required string EmotionalPatterns { get; init; }

    /// <summary>
    /// Coping mechanisms.
    /// </summary>
    public List<string> CopingMechanisms { get; init; } = new();

    /// <summary>
    /// Defense mechanisms.
    /// </summary>
    public List<string> DefenseMechanisms { get; init; } = new();

    /// <summary>
    /// Motivations.
    /// </summary>
    public required string PrimaryMotivation { get; init; }

    /// <summary>
    /// Secondary motivations.
    /// </summary>
    public List<string> SecondaryMotivations { get; init; } = new();
}

/// <summary>
/// Character background and history.
/// </summary>
public sealed record CharacterBackground
{
    /// <summary>
    /// Birthplace.
    /// </summary>
    public required string Birthplace { get; init; }

    /// <summary>
    /// Family background.
    /// </summary>
    public required string FamilyBackground { get; init; }

    /// <summary>
    /// Social class.
    /// </summary>
    public required string SocialClass { get; init; }

    /// <summary>
    /// Education.
    /// </summary>
    public required string Education { get; init; }

    /// <summary>
    /// Occupation/profession.
    /// </summary>
    public required string Occupation { get; init; }

    /// <summary>
    /// Key life events.
    /// </summary>
    public List<LifeEvent> KeyLifeEvents { get; init; } = new();

    /// <summary>
    /// Formative experiences.
    /// </summary>
    public List<string> FormativeExperiences { get; init; } = new();

    /// <summary>
    /// Trauma/wounds.
    /// </summary>
    public List<string> Wounds { get; init; } = new();

    /// <summary>
    /// Skills and abilities.
    /// </summary>
    public List<string> Skills { get; init; } = new();

    /// <summary>
    /// Special abilities if any.
    /// </summary>
    public List<string> SpecialAbilities { get; init; } = new();

    /// <summary>
    /// Current living situation.
    /// </summary>
    public required string CurrentSituation { get; init; }

    /// <summary>
    /// Important possessions.
    /// </summary>
    public List<string> ImportantPossessions { get; init; } = new();

    /// <summary>
    /// Secrets.
    /// </summary>
    public List<CharacterSecret> Secrets { get; init; } = new();
}

/// <summary>
/// Key life event.
/// </summary>
public sealed record LifeEvent
{
    /// <summary>
    /// When it happened.
    /// </summary>
    public required string When { get; init; }

    /// <summary>
    /// What happened.
    /// </summary>
    public required string What { get; init; }

    /// <summary>
    /// Impact on the character.
    /// </summary>
    public required string Impact { get; init; }
}

/// <summary>
/// Character secret.
/// </summary>
public sealed record CharacterSecret
{
    /// <summary>
    /// The secret.
    /// </summary>
    public required string Secret { get; init; }

    /// <summary>
    /// Who knows about it.
    /// </summary>
    public List<Guid> KnownBy { get; init; } = new();

    /// <summary>
    /// Impact if revealed.
    /// </summary>
    public required string RevealImpact { get; init; }

    /// <summary>
    /// When/if revealed in story.
    /// </summary>
    public int? RevealChapter { get; init; }
}

/// <summary>
/// Character voice and speaking patterns.
/// </summary>
public sealed record CharacterVoice
{
    /// <summary>
    /// Speaking style description.
    /// </summary>
    public required string SpeakingStyle { get; init; }

    /// <summary>
    /// Vocabulary level.
    /// </summary>
    public required string VocabularyLevel { get; init; }

    /// <summary>
    /// Accent or dialect.
    /// </summary>
    public string? Accent { get; init; }

    /// <summary>
    /// Speech patterns.
    /// </summary>
    public List<string> SpeechPatterns { get; init; } = new();

    /// <summary>
    /// Verbal tics or habits.
    /// </summary>
    public List<string> VerbalTics { get; init; } = new();

    /// <summary>
    /// Favorite expressions or catchphrases.
    /// </summary>
    public List<string> FavoriteExpressions { get; init; } = new();

    /// <summary>
    /// Topics they talk about often.
    /// </summary>
    public List<string> CommonTopics { get; init; } = new();

    /// <summary>
    /// Topics they avoid.
    /// </summary>
    public List<string> AvoidedTopics { get; init; } = new();

    /// <summary>
    /// Non-verbal communication style.
    /// </summary>
    public required string NonVerbalStyle { get; init; }

    /// <summary>
    /// Physical mannerisms.
    /// </summary>
    public List<string> Mannerisms { get; init; } = new();

    /// <summary>
    /// Internal thought patterns (for POV).
    /// </summary>
    public required string ThoughtPatterns { get; init; }

    /// <summary>
    /// Sample dialogue lines.
    /// </summary>
    public List<string> SampleDialogue { get; init; } = new();
}

/// <summary>
/// Character arc definition.
/// </summary>
public sealed record CharacterArc
{
    /// <summary>
    /// Arc type (positive, negative, flat, etc.).
    /// </summary>
    public required string ArcType { get; init; }

    /// <summary>
    /// Starting state.
    /// </summary>
    public required string StartingState { get; init; }

    /// <summary>
    /// Ending state.
    /// </summary>
    public required string EndingState { get; init; }

    /// <summary>
    /// Internal conflict.
    /// </summary>
    public required string InternalConflict { get; init; }

    /// <summary>
    /// External conflict.
    /// </summary>
    public required string ExternalConflict { get; init; }

    /// <summary>
    /// What they want (external goal).
    /// </summary>
    public required string Want { get; init; }

    /// <summary>
    /// What they need (internal growth).
    /// </summary>
    public required string Need { get; init; }

    /// <summary>
    /// Primary flaw.
    /// </summary>
    public required string Flaw { get; init; }

    /// <summary>
    /// Emotional wound.
    /// </summary>
    public required string Wound { get; init; }

    /// <summary>
    /// The lie they believe.
    /// </summary>
    public required string LieTheyBelieve { get; init; }

    /// <summary>
    /// The truth they learn.
    /// </summary>
    public required string TruthTheyLearn { get; init; }

    /// <summary>
    /// Ghost/backstory event.
    /// </summary>
    public required string Ghost { get; init; }

    /// <summary>
    /// Arc milestones.
    /// </summary>
    public List<ArcMilestone> Milestones { get; init; } = new();

    /// <summary>
    /// Key transformation moments.
    /// </summary>
    public List<TransformationMoment> TransformationMoments { get; init; } = new();
}

/// <summary>
/// Milestone in a character arc.
/// </summary>
public sealed record ArcMilestone
{
    /// <summary>
    /// Chapter where this occurs.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Emotional state at this point.
    /// </summary>
    public required string EmotionalState { get; init; }

    /// <summary>
    /// What triggers this milestone.
    /// </summary>
    public required string Trigger { get; init; }

    /// <summary>
    /// Result of this milestone.
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Progress toward truth (0-100).
    /// </summary>
    public int ProgressTowardTruth { get; init; }
}

/// <summary>
/// Transformation moment.
/// </summary>
public sealed record TransformationMoment
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// What changes.
    /// </summary>
    public required string Change { get; init; }

    /// <summary>
    /// How it manifests.
    /// </summary>
    public required string Manifestation { get; init; }
}

/// <summary>
/// Guidelines for writing a character.
/// </summary>
public sealed record CharacterWritingGuidelines
{
    /// <summary>
    /// Do's for this character.
    /// </summary>
    public List<string> Dos { get; init; } = new();

    /// <summary>
    /// Don'ts for this character.
    /// </summary>
    public List<string> Donts { get; init; } = new();

    /// <summary>
    /// Key phrases to use.
    /// </summary>
    public List<string> KeyPhrases { get; init; } = new();

    /// <summary>
    /// Phrases to avoid.
    /// </summary>
    public List<string> AvoidPhrases { get; init; } = new();

    /// <summary>
    /// Emotional triggers.
    /// </summary>
    public List<string> EmotionalTriggers { get; init; } = new();

    /// <summary>
    /// Consistent behaviors to maintain.
    /// </summary>
    public List<string> ConsistentBehaviors { get; init; } = new();

    /// <summary>
    /// Voice sample paragraph.
    /// </summary>
    public string? VoiceSample { get; init; }
}

/// <summary>
/// Relationship between characters.
/// </summary>
public sealed record CharacterRelationship
{
    /// <summary>
    /// Other character ID.
    /// </summary>
    public required Guid OtherCharacterId { get; init; }

    /// <summary>
    /// Relationship type.
    /// </summary>
    public required string RelationshipType { get; init; }

    /// <summary>
    /// How they feel about the other.
    /// </summary>
    public required string Feelings { get; init; }

    /// <summary>
    /// History of the relationship.
    /// </summary>
    public required string History { get; init; }

    /// <summary>
    /// Current status.
    /// </summary>
    public required string CurrentStatus { get; init; }

    /// <summary>
    /// Tension/conflict in the relationship.
    /// </summary>
    public string? Tension { get; init; }

    /// <summary>
    /// How relationship evolves during story.
    /// </summary>
    public required string Evolution { get; init; }

    /// <summary>
    /// Key moments in their relationship.
    /// </summary>
    public List<RelationshipMoment> KeyMoments { get; init; } = new();
}

/// <summary>
/// Key moment in a relationship.
/// </summary>
public sealed record RelationshipMoment
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// What happens.
    /// </summary>
    public required string Event { get; init; }

    /// <summary>
    /// Impact on relationship.
    /// </summary>
    public required string Impact { get; init; }
}

/// <summary>
/// Relationship web between multiple characters.
/// </summary>
public sealed record CharacterRelationshipWeb
{
    /// <summary>
    /// Name of this web/group.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Character IDs in this web.
    /// </summary>
    public required List<Guid> CharacterIds { get; init; }

    /// <summary>
    /// Description of group dynamics.
    /// </summary>
    public required string Dynamics { get; init; }

    /// <summary>
    /// Central tension.
    /// </summary>
    public string? CentralTension { get; init; }

    /// <summary>
    /// How dynamics evolve.
    /// </summary>
    public required string Evolution { get; init; }
}

/// <summary>
/// Map of all character arcs.
/// </summary>
public sealed record CharacterArcMap
{
    /// <summary>
    /// Description of how arcs interweave.
    /// </summary>
    public required string InterweaveDescription { get; init; }

    /// <summary>
    /// Parallel arcs.
    /// </summary>
    public List<ParallelArc> ParallelArcs { get; init; } = new();

    /// <summary>
    /// Arc intersections.
    /// </summary>
    public List<ArcIntersection> Intersections { get; init; } = new();
}

/// <summary>
/// Parallel character arcs.
/// </summary>
public sealed record ParallelArc
{
    /// <summary>
    /// Character IDs with parallel arcs.
    /// </summary>
    public required List<Guid> CharacterIds { get; init; }

    /// <summary>
    /// How they parallel.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public required string ThematicPurpose { get; init; }
}

/// <summary>
/// Point where character arcs intersect.
/// </summary>
public sealed record ArcIntersection
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Characters involved.
    /// </summary>
    public required List<Guid> CharacterIds { get; init; }

    /// <summary>
    /// Nature of intersection.
    /// </summary>
    public required string Nature { get; init; }

    /// <summary>
    /// Impact on each arc.
    /// </summary>
    public required string Impact { get; init; }
}

/// <summary>
/// Character group/faction.
/// </summary>
public sealed record CharacterGroup
{
    /// <summary>
    /// Group name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Group description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Member IDs.
    /// </summary>
    public List<Guid> MemberIds { get; init; } = new();

    /// <summary>
    /// Leader ID if any.
    /// </summary>
    public Guid? LeaderId { get; init; }

    /// <summary>
    /// Group goals.
    /// </summary>
    public List<string> Goals { get; init; } = new();

    /// <summary>
    /// Internal dynamics.
    /// </summary>
    public required string InternalDynamics { get; init; }
}

/// <summary>
/// Naming conventions for the world.
/// </summary>
public sealed record NamingConventions
{
    /// <summary>
    /// Cultural naming patterns.
    /// </summary>
    public List<CulturalNamingPattern> CulturalPatterns { get; init; } = new();

    /// <summary>
    /// Naming rules to follow.
    /// </summary>
    public List<string> Rules { get; init; } = new();

    /// <summary>
    /// Names to avoid.
    /// </summary>
    public List<string> NamesToAvoid { get; init; } = new();
}

/// <summary>
/// Cultural naming pattern.
/// </summary>
public sealed record CulturalNamingPattern
{
    /// <summary>
    /// Culture/region.
    /// </summary>
    public required string Culture { get; init; }

    /// <summary>
    /// Pattern description.
    /// </summary>
    public required string Pattern { get; init; }

    /// <summary>
    /// Example names.
    /// </summary>
    public List<string> Examples { get; init; } = new();
}
