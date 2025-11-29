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
    public CharacterArcMap ArcMap { get; init; } = new();

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
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Preferred name (how they're usually referred to).
    /// </summary>
    public string PreferredName { get; init; } = string.Empty;

    /// <summary>
    /// Aliases, nicknames, titles.
    /// </summary>
    public List<string> Aliases { get; init; } = new();

    /// <summary>
    /// Role in the story.
    /// </summary>
    public CharacterRole Role { get; init; } = CharacterRole.Supporting;

    /// <summary>
    /// Character archetype.
    /// </summary>
    public string Archetype { get; init; } = string.Empty;

    /// <summary>
    /// One-line character concept.
    /// </summary>
    public string Concept { get; init; } = string.Empty;

    // ================== PHYSICAL DESCRIPTION ==================

    /// <summary>
    /// Physical description.
    /// </summary>
    public PhysicalDescription Physical { get; init; } = new();

    // ================== PSYCHOLOGICAL PROFILE ==================

    /// <summary>
    /// Psychological profile.
    /// </summary>
    public PsychologicalProfile Psychology { get; init; } = new();

    // ================== BACKGROUND ==================

    /// <summary>
    /// Character background.
    /// </summary>
    public CharacterBackground Background { get; init; } = new();

    // ================== VOICE & MANNERISMS ==================

    /// <summary>
    /// Voice and speaking patterns.
    /// </summary>
    public CharacterVoice Voice { get; init; } = new();

    /// <summary>
    /// Speech pattern alias (from Voice).
    /// </summary>
    public string? SpeechPattern { get => Voice.SpeakingStyle; }

    /// <summary>
    /// Core traits alias (from Psychology).
    /// </summary>
    public List<string> CoreTraits { get => Psychology.CoreTraits ?? new List<string>(); }

    // ================== CHARACTER ARC ==================

    /// <summary>
    /// Character arc definition.
    /// </summary>
    public CharacterArc Arc { get; init; } = new();

    // ================== STORY FUNCTION ==================

    /// <summary>
    /// Function in the story.
    /// </summary>
    public string StoryFunction { get; init; } = string.Empty;

    /// <summary>
    /// Thematic significance.
    /// </summary>
    public string ThematicRole { get; init; } = string.Empty;

    /// <summary>
    /// Chapters this character appears in.
    /// </summary>
    public List<int> ChapterAppearances { get; init; } = new();

    /// <summary>
    /// First introduction chapter.
    /// </summary>
    public int IntroductionChapter { get; init; }

    // ================== WRITING GUIDELINES ==================

    /// <summary>
    /// Guidelines for writing this character.
    /// </summary>
    public CharacterWritingGuidelines WritingGuidelines { get; init; } = new();

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
    public string Age { get; init; } = string.Empty;

    /// <summary>
    /// Gender.
    /// </summary>
    public string Gender { get; init; } = string.Empty;

    /// <summary>
    /// Height description.
    /// </summary>
    public string Height { get; init; } = string.Empty;

    /// <summary>
    /// Build/body type.
    /// </summary>
    public string Build { get; init; } = string.Empty;

    /// <summary>
    /// Hair description.
    /// </summary>
    public string Hair { get; init; } = string.Empty;

    /// <summary>
    /// Eye description.
    /// </summary>
    public string Eyes { get; init; } = string.Empty;

    /// <summary>
    /// Skin description.
    /// </summary>
    public string Skin { get; init; } = string.Empty;

    /// <summary>
    /// Distinguishing features.
    /// </summary>
    public List<string> DistinguishingFeatures { get; init; } = new();

    /// <summary>
    /// Typical clothing/style.
    /// </summary>
    public string TypicalAttire { get; init; } = string.Empty;

    /// <summary>
    /// How they carry themselves.
    /// </summary>
    public string Bearing { get; init; } = string.Empty;

    /// <summary>
    /// First impression they give.
    /// </summary>
    public string FirstImpression { get; init; } = string.Empty;

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
    public string MoralAlignment { get; init; } = string.Empty;

    /// <summary>
    /// Emotional patterns.
    /// </summary>
    public string EmotionalPatterns { get; init; } = string.Empty;

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
    public string PrimaryMotivation { get; init; } = string.Empty;

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
    public string Birthplace { get; init; } = string.Empty;

    /// <summary>
    /// Family background.
    /// </summary>
    public string FamilyBackground { get; init; } = string.Empty;

    /// <summary>
    /// Social class.
    /// </summary>
    public string SocialClass { get; init; } = string.Empty;

    /// <summary>
    /// Education.
    /// </summary>
    public string Education { get; init; } = string.Empty;

    /// <summary>
    /// Occupation/profession.
    /// </summary>
    public string Occupation { get; init; } = string.Empty;

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
    public string CurrentSituation { get; init; } = string.Empty;

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
    public string When { get; init; } = string.Empty;

    /// <summary>
    /// What happened.
    /// </summary>
    public string What { get; init; } = string.Empty;

    /// <summary>
    /// Impact on the character.
    /// </summary>
    public string Impact { get; init; } = string.Empty;
}

/// <summary>
/// Character secret.
/// </summary>
public sealed record CharacterSecret
{
    /// <summary>
    /// The secret.
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// Who knows about it.
    /// </summary>
    public List<Guid> KnownBy { get; init; } = new();

    /// <summary>
    /// Impact if revealed.
    /// </summary>
    public string RevealImpact { get; init; } = string.Empty;

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
    public string SpeakingStyle { get; init; } = string.Empty;

    /// <summary>
    /// Vocabulary level.
    /// </summary>
    public string VocabularyLevel { get; init; } = string.Empty;

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
    public string NonVerbalStyle { get; init; } = string.Empty;

    /// <summary>
    /// Physical mannerisms.
    /// </summary>
    public List<string> Mannerisms { get; init; } = new();

    /// <summary>
    /// Internal thought patterns (for POV).
    /// </summary>
    public string ThoughtPatterns { get; init; } = string.Empty;

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
    public string ArcType { get; init; } = string.Empty;

    /// <summary>
    /// Type alias for ArcType.
    /// </summary>
    public string Type { get => ArcType; init => ArcType = value; }

    /// <summary>
    /// Current phase in the arc.
    /// </summary>
    public string CurrentPhase { get; init; } = string.Empty;

    /// <summary>
    /// Starting state.
    /// </summary>
    public string StartingState { get; init; } = string.Empty;

    /// <summary>
    /// Ending state.
    /// </summary>
    public string EndingState { get; init; } = string.Empty;

    /// <summary>
    /// Internal conflict.
    /// </summary>
    public string InternalConflict { get; init; } = string.Empty;

    /// <summary>
    /// External conflict.
    /// </summary>
    public string ExternalConflict { get; init; } = string.Empty;

    /// <summary>
    /// What they want (external goal).
    /// </summary>
    public string Want { get; init; } = string.Empty;

    /// <summary>
    /// What they need (internal growth).
    /// </summary>
    public string Need { get; init; } = string.Empty;

    /// <summary>
    /// Primary flaw.
    /// </summary>
    public string Flaw { get; init; } = string.Empty;

    /// <summary>
    /// Emotional wound.
    /// </summary>
    public string Wound { get; init; } = string.Empty;

    /// <summary>
    /// The lie they believe.
    /// </summary>
    public string LieTheyBelieve { get; init; } = string.Empty;

    /// <summary>
    /// The truth they learn.
    /// </summary>
    public string TruthTheyLearn { get; init; } = string.Empty;

    /// <summary>
    /// Ghost/backstory event.
    /// </summary>
    public string Ghost { get; init; } = string.Empty;

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
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Emotional state at this point.
    /// </summary>
    public string EmotionalState { get; init; } = string.Empty;

    /// <summary>
    /// What triggers this milestone.
    /// </summary>
    public string Trigger { get; init; } = string.Empty;

    /// <summary>
    /// Result of this milestone.
    /// </summary>
    public string Result { get; init; } = string.Empty;

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
    public int ChapterNumber { get; init; }

    /// <summary>
    /// What changes.
    /// </summary>
    public string Change { get; init; } = string.Empty;

    /// <summary>
    /// How it manifests.
    /// </summary>
    public string Manifestation { get; init; } = string.Empty;
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
    public Guid OtherCharacterId { get; init; }

    /// <summary>
    /// Relationship type.
    /// </summary>
    public string RelationshipType { get; init; } = string.Empty;

    /// <summary>
    /// How they feel about the other.
    /// </summary>
    public string Feelings { get; init; } = string.Empty;

    /// <summary>
    /// History of the relationship.
    /// </summary>
    public string History { get; init; } = string.Empty;

    /// <summary>
    /// Current status.
    /// </summary>
    public string CurrentStatus { get; init; } = string.Empty;

    /// <summary>
    /// Tension/conflict in the relationship.
    /// </summary>
    public string? Tension { get; init; }

    /// <summary>
    /// How relationship evolves during story.
    /// </summary>
    public string Evolution { get; init; } = string.Empty;

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
    public int ChapterNumber { get; init; }

    /// <summary>
    /// What happens.
    /// </summary>
    public string Event { get; init; } = string.Empty;

    /// <summary>
    /// Impact on relationship.
    /// </summary>
    public string Impact { get; init; } = string.Empty;
}

/// <summary>
/// Relationship web between multiple characters.
/// </summary>
public sealed record CharacterRelationshipWeb
{
    /// <summary>
    /// Name of this web/group.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Character IDs in this web.
    /// </summary>
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// Description of group dynamics.
    /// </summary>
    public string Dynamics { get; init; } = string.Empty;

    /// <summary>
    /// Central tension.
    /// </summary>
    public string? CentralTension { get; init; }

    /// <summary>
    /// How dynamics evolve.
    /// </summary>
    public string Evolution { get; init; } = string.Empty;
}

/// <summary>
/// Map of all character arcs.
/// </summary>
public sealed record CharacterArcMap
{
    /// <summary>
    /// Description of how arcs interweave.
    /// </summary>
    public string InterweaveDescription { get; init; } = string.Empty;

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
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// How they parallel.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Thematic purpose.
    /// </summary>
    public string ThematicPurpose { get; init; } = string.Empty;
}

/// <summary>
/// Point where character arcs intersect.
/// </summary>
public sealed record ArcIntersection
{
    /// <summary>
    /// Chapter.
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Characters involved.
    /// </summary>
    public List<Guid> CharacterIds { get; init; } = new();

    /// <summary>
    /// Nature of intersection.
    /// </summary>
    public string Nature { get; init; } = string.Empty;

    /// <summary>
    /// Impact on each arc.
    /// </summary>
    public string Impact { get; init; } = string.Empty;
}

/// <summary>
/// Character group/faction.
/// </summary>
public sealed record CharacterGroup
{
    /// <summary>
    /// Group name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Group description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

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
    public string InternalDynamics { get; init; } = string.Empty;
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
    public string Culture { get; init; } = string.Empty;

    /// <summary>
    /// Pattern description.
    /// </summary>
    public string Pattern { get; init; } = string.Empty;

    /// <summary>
    /// Example names.
    /// </summary>
    public List<string> Examples { get; init; } = new();
}
