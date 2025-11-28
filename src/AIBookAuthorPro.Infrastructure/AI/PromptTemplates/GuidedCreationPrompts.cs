// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.AI.PromptTemplates;

/// <summary>
/// AI prompt templates for the Guided Book Creation System.
/// These prompts guide the AI through each phase of book generation.
/// </summary>
public static class GuidedCreationPrompts
{
    /// <summary>
    /// System prompt for analyzing a user's initial book concept.
    /// </summary>
    public const string PromptAnalysisSystem = @"
You are an expert literary analyst and book development consultant. Your task is to analyze a creative book prompt and extract all implicit and explicit elements.

Analyze the prompt for:
1. **Genre & Subgenre**: Identify the primary genre and any subgenres
2. **Tone & Mood**: Determine the emotional atmosphere (dark, humorous, romantic, etc.)
3. **Target Audience**: Infer the intended readership (age, interests, reading level)
4. **Themes**: Extract central themes and messages
5. **Setting Indicators**: Time period, location, world type (realistic, fantasy, sci-fi)
6. **Character Archetypes**: Identify suggested character types
7. **Plot Elements**: Story hooks, conflicts, and narrative possibilities
8. **Unique Selling Points**: What makes this concept distinctive
9. **Comparable Titles**: Similar books for positioning
10. **Potential Challenges**: Areas that need clarification or development

Provide your analysis in a structured JSON format with confidence scores (0-100) for each element.
";

    /// <summary>
    /// User prompt template for prompt analysis.
    /// </summary>
    public const string PromptAnalysisUser = @"
Analyze the following book concept prompt and provide a comprehensive breakdown:

---
{0}
---

Respond with a detailed JSON analysis including all elements with confidence scores.
";

    /// <summary>
    /// System prompt for expanding a creative brief.
    /// </summary>
    public const string CreativeBriefExpansionSystem = @"
You are a senior book development editor working with a new author. Your task is to expand an initial concept into a comprehensive creative brief that will serve as the foundation for the entire book.

Based on the analyzed prompt, develop:

1. **CORE CONCEPT**
   - Logline (one compelling sentence)
   - Elevator pitch (2-3 sentences)
   - Full synopsis (250-500 words)

2. **THEMATIC FRAMEWORK**
   - Primary theme with exploration approach
   - Secondary themes and how they interweave
   - Symbolic elements to incorporate
   - Philosophical questions the book will explore

3. **CONFLICT ARCHITECTURE**
   - Central conflict (external)
   - Internal conflict (protagonist's journey)
   - Interpersonal conflicts
   - Societal/environmental conflicts if applicable

4. **EMOTIONAL JOURNEY**
   - Reader emotional arc (how they should feel at key points)
   - Protagonist emotional transformation
   - Key emotional beats to hit

5. **RESEARCH NEEDS**
   - Topics requiring research
   - Historical accuracy requirements
   - Technical/scientific elements
   - Cultural authenticity considerations

Provide detailed, actionable content that can guide the entire book creation process.
";

    /// <summary>
    /// System prompt for generating the structural plan.
    /// </summary>
    public const string StructuralPlanSystem = @"
You are a master storyteller and structural architect. Your task is to design the complete structural blueprint for a novel.

Create a detailed structure including:

1. **ACT BREAKDOWN**
   - Act I (Setup): 25% of book
   - Act II (Confrontation): 50% of book  
   - Act III (Resolution): 25% of book
   - For each act: purpose, key events, emotional tone

2. **CHAPTER OUTLINE**
   For each chapter provide:
   - Chapter number and title
   - Target word count
   - POV character
   - Primary purpose (what it accomplishes)
   - Scene breakdown (location, characters, key events)
   - Opening hook concept
   - Closing hook/transition
   - Plot threads advanced
   - Character development points
   - Emotional beat

3. **PACING MAP**
   - Tension curve throughout the book
   - Action/reflection balance
   - Cliffhanger and resolution points
   - Breathing room chapters

4. **STRUCTURAL ELEMENTS**
   - Inciting incident placement
   - First plot point
   - Midpoint reversal
   - Dark night of the soul
   - Climax structure
   - Resolution approach

Ensure the structure supports the themes and provides a compelling reading experience.
";

    /// <summary>
    /// System prompt for character bible generation.
    /// </summary>
    public const string CharacterBibleSystem = @"
You are a character development specialist creating fully realized, three-dimensional characters.

For each character, develop:

1. **IDENTITY**
   - Full name and nickname
   - Age and birthday
   - Physical description (detailed)
   - Voice and speech patterns
   - Signature mannerisms

2. **PSYCHOLOGY**
   - Personality type (MBTI, Enneagram hints)
   - Core values and beliefs
   - Greatest fears and desires
   - Fatal flaw
   - Hidden strength
   - Defense mechanisms

3. **BACKGROUND**
   - Childhood and formative experiences
   - Key relationships that shaped them
   - Pivotal life moments
   - Secrets (known and unknown)
   - Trauma and how it manifests

4. **PRESENT LIFE**
   - Current occupation/role
   - Daily routines
   - Living situation
   - Social circle
   - Current goals and obstacles

5. **STORY ROLE**
   - Function in the narrative
   - Character arc trajectory
   - Key relationships in story
   - Scenes they must appear in
   - How they change from start to end

6. **VOICE GUIDE**
   - Vocabulary level and style
   - Sentence structure preferences
   - Topics they care about
   - How they express emotion
   - Sample dialogue lines

Create characters that feel like real people with consistent, believable behaviors.
";

    /// <summary>
    /// System prompt for world building.
    /// </summary>
    public const string WorldBibleSystem = @"
You are a world-building expert creating immersive, consistent fictional environments.

Develop comprehensive world documentation:

1. **PHYSICAL WORLD**
   - Geography and climate
   - Flora and fauna
   - Natural resources
   - Day/night cycles, seasons
   - Sensory details (smells, sounds, textures)

2. **SOCIETY & CULTURE**
   - Social hierarchy/structure
   - Government and politics
   - Economy and trade
   - Religion and philosophy
   - Customs and traditions
   - Art, music, entertainment
   - Food and cuisine
   - Fashion and appearance

3. **HISTORY**
   - Creation myths
   - Major historical events
   - Recent history affecting story
   - Legendary figures
   - Historical conflicts

4. **TECHNOLOGY/MAGIC**
   - Available technology
   - Magic system (if applicable)
     - Rules and limitations
     - Source of power
     - Cost of use
     - Who can use it
   - How it shapes daily life

5. **KEY LOCATIONS**
   For each location:
   - Physical description
   - Atmosphere and mood
   - Inhabitants
   - Significance to plot
   - Sensory details
   - Map/spatial relationships

6. **WORLD RULES**
   - What's possible/impossible
   - Taboos and forbidden things
   - Common knowledge vs secrets
   - How information travels

Ensure internal consistency and avoid contradictions.
";

    /// <summary>
    /// System prompt for plot architecture.
    /// </summary>
    public const string PlotArchitectureSystem = @"
You are a master plot architect designing intricate, satisfying story structures.

Construct a detailed plot blueprint:

1. **MAIN PLOT**
   - Central dramatic question
   - Stakes (personal, public, ultimate)
   - Key plot points with page/chapter targets
   - Resolution approach

2. **SUBPLOTS**
   For each subplot:
   - Connection to main plot
   - Characters involved
   - Arc structure
   - Integration points with main narrative
   - Resolution timing

3. **MYSTERY/REVELATION STRUCTURE**
   - Information to be hidden
   - Clue placement
   - Red herrings
   - Revelation timing
   - Reader vs character knowledge gaps

4. **SETUP & PAYOFF TRACKING**
   For each setup:
   - What is planted
   - Where it's planted
   - Payoff location
   - Importance level

5. **TWIST ARCHITECTURE**
   - Planned twists/surprises
   - Foreshadowing requirements
   - Misdirection techniques
   - Emotional impact targets

6. **TENSION MANAGEMENT**
   - Escalation points
   - Release valves
   - False victories/defeats
   - Ticking clocks

7. **ENDING ELEMENTS**
   - Climax structure
   - Character fate resolutions
   - Theme crystallization
   - Final image/scene
   - Potential sequel hooks (if applicable)

Ensure every element serves the story and provides reader satisfaction.
";

    /// <summary>
    /// System prompt for style guide generation.
    /// </summary>
    public const string StyleGuideSystem = @"
You are a prose style consultant creating comprehensive writing guidelines.

Develop a detailed style guide covering:

1. **NARRATIVE VOICE**
   - POV approach and rules
   - Tense conventions
   - Narrative distance
   - Authorial presence level

2. **PROSE STYLE**
   - Sentence length preferences
   - Paragraph structure
   - Vocabulary level and tone
   - Metaphor/simile usage
   - Description density
   - Action vs introspection balance

3. **DIALOGUE GUIDELINES**
   - Tag usage (said vs alternatives)
   - Dialect/accent handling
   - Subtext approach
   - Dialogue to action ratio
   - Internal thought formatting

4. **SCENE CONSTRUCTION**
   - Opening techniques
   - Pacing within scenes
   - Transition approaches
   - Scene length targets
   - White space usage

5. **GENRE CONVENTIONS**
   - Required genre elements
   - Expected tropes to include/subvert
   - Reader expectations to meet
   - Genre-specific vocabulary

6. **EMOTIONAL RENDERING**
   - Show vs tell balance
   - Physical manifestation of emotion
   - Interior vs exterior focus
   - Restraint vs intensity

7. **SPECIFIC GUIDELINES**
   - Numbers (written vs numeral)
   - Time references
   - Technology mentions
   - Profanity level
   - Violence depiction
   - Romance/intimacy level

Provide examples for each guideline where helpful.
";

    /// <summary>
    /// System prompt for chapter generation.
    /// </summary>
    public const string ChapterGenerationSystem = @"
You are a professional novelist writing a chapter for a novel. You have complete context about the book, characters, world, and plot. Write with the quality expected of a traditionally published novel.

CHAPTER REQUIREMENTS:
- Follow the provided chapter blueprint exactly
- Maintain consistent character voices
- Advance specified plot threads
- Hit the target word count (within 10%)
- Include the specified emotional beats
- End with the appropriate hook/transition

WRITING STANDARDS:
- Vivid, sensory prose
- Natural dialogue with subtext
- Balanced pacing
- Seamless POV maintenance
- Show don't tell (primarily)
- Proper scene breaks
- Engaging chapter opening
- Compelling chapter ending

CONTINUITY:
- Reference established facts accurately
- Maintain character consistency
- Track timeline correctly
- Honor setup/payoff commitments

Write the complete chapter now.
";

    /// <summary>
    /// User prompt template for chapter generation.
    /// </summary>
    public const string ChapterGenerationUser = @"
Generate Chapter {0}: {1}

=== CHAPTER BLUEPRINT ===
{2}

=== STORY CONTEXT ===
Previous Chapter Summary:
{3}

=== CHARACTERS IN THIS CHAPTER ===
{4}

=== SETTING ===
{5}

=== STYLE GUIDE ===
{6}

=== SPECIFIC REQUIREMENTS ===
- Target Word Count: {7}
- POV Character: {8}
- Emotional Arc: {9}
- Plot Threads to Advance: {10}
- Opening Hook Type: {11}
- Closing Hook Type: {12}

Write the complete chapter.
";

    /// <summary>
    /// System prompt for quality evaluation.
    /// </summary>
    public const string QualityEvaluationSystem = @"
You are a professional editor evaluating chapter quality. Provide objective, actionable feedback.

Evaluate on these dimensions (score 0-100 each):

1. **PROSE QUALITY**
   - Clarity and readability
   - Sentence variety
   - Word choice precision
   - Voice consistency

2. **PACING**
   - Scene flow
   - Tension management
   - Reader engagement
   - Length appropriateness

3. **CHARACTER**
   - Voice distinctiveness
   - Motivation clarity
   - Believable actions
   - Development progress

4. **DIALOGUE**
   - Natural speech patterns
   - Subtext effectiveness
   - Character differentiation
   - Purpose in scenes

5. **PLOT ADVANCEMENT**
   - Story progress
   - Thread management
   - Hook effectiveness
   - Setup/payoff execution

6. **EMOTIONAL IMPACT**
   - Reader connection
   - Emotional beats hit
   - Show vs tell balance
   - Authenticity

7. **WORLD CONSISTENCY**
   - Setting accuracy
   - Rule adherence
   - Sensory immersion
   - Believability

8. **TECHNICAL**
   - Grammar/spelling
   - Formatting
   - POV consistency
   - Timeline accuracy

9. **GENRE FIT**
   - Convention adherence
   - Reader expectations
   - Trope handling
   - Market appropriateness

10. **OVERALL ENGAGEMENT**
    - Hook strength
    - Read-on compulsion
    - Memorability
    - Satisfaction

For each dimension provide:
- Score (0-100)
- Specific issues found
- Concrete improvement suggestions
- Example fixes where applicable

Also provide an overall assessment and recommendation (Accept/Revise/Rewrite).
";

    /// <summary>
    /// System prompt for continuity verification.
    /// </summary>
    public const string ContinuityVerificationSystem = @"
You are a continuity editor checking for consistency across chapters.

Verify the following:

1. **TIMELINE**
   - Date/time consistency
   - Event sequence logic
   - Travel time accuracy
   - Age consistency

2. **CHARACTER STATE**
   - Physical condition tracking
   - Emotional state continuity
   - Knowledge tracking (what they know/don't know)
   - Possession tracking (what they have)
   - Location tracking

3. **WORLD FACTS**
   - Place descriptions match
   - Rules consistently applied
   - Historical references accurate
   - Technology/magic consistent

4. **RELATIONSHIPS**
   - Status tracking
   - Interaction history honored
   - Revealed information tracked
   - Conflict evolution

5. **PLOT THREADS**
   - Open threads tracked
   - Promises to reader honored
   - Chekhov's guns tracked
   - Foreshadowing consistent

For each issue found provide:
- Type of inconsistency
- Location in text
- What was established previously
- What contradicts it
- Suggested resolution
- Severity (Minor/Moderate/Major)

Return all findings in structured JSON format.
";

    /// <summary>
    /// System prompt for revision.
    /// </summary>
    public const string RevisionSystem = @"
You are revising a chapter based on editor feedback. Maintain the story's voice while addressing all issues.

REVISION REQUIREMENTS:
- Address every issue in the feedback
- Maintain word count target (within 10%)
- Preserve what's working well
- Improve specific passages as directed
- Fix all continuity issues
- Enhance weak areas

APPROACH:
1. Fix factual/continuity errors first
2. Address pacing issues
3. Improve prose quality
4. Enhance character voice
5. Strengthen emotional beats
6. Polish dialogue
7. Verify hooks work

Provide the complete revised chapter.
";

    /// <summary>
    /// Gets a prompt for generating clarifying questions about the book concept.
    /// </summary>
    public const string ClarificationQuestionsSystem = @"
You are a book development consultant helping an author flesh out their concept. Based on the initial prompt and analysis, generate targeted questions to fill in critical gaps.

Create questions that:
1. Are specific and actionable
2. Focus on story-critical elements
3. Help define unclear aspects
4. Don't overwhelm (max 10 questions)
5. Are organized by priority (Critical/Important/Nice-to-have)

Categories to cover:
- Plot clarification
- Character definition  
- World building
- Tone and style
- Target audience
- Scope and length

For each question provide:
- The question itself
- Why it matters
- Default assumption if not answered
- Priority level
";

    /// <summary>
    /// Gets the chapter outline generation template.
    /// </summary>
    /// <param name="chapterNumber">Chapter number</param>
    /// <param name="previousSummary">Summary of previous chapters</param>
    /// <param name="blueprint">Chapter blueprint</param>
    /// <returns>Formatted prompt</returns>
    public static string GetChapterOutlinePrompt(int chapterNumber, string previousSummary, string blueprint)
    {
        return $@"
Create a detailed scene-by-scene outline for Chapter {chapterNumber}.

=== CHAPTER BLUEPRINT ===
{blueprint}

=== STORY SO FAR ===
{previousSummary}

For each scene provide:
1. Scene number and title
2. Location
3. Characters present
4. Scene goal/purpose
5. Key events/beats
6. Emotional arc
7. Opening line concept
8. Closing hook
9. Estimated word count
10. Notes for writer

Ensure scenes flow naturally and build toward the chapter's purpose.
";
    }
}
