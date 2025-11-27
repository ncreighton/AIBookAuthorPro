// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Enums;

/// <summary>
/// Represents the available AI providers for content generation.
/// </summary>
public enum AIProviderType
{
    /// <summary>
    /// Anthropic's Claude models.
    /// </summary>
    Claude = 0,
    
    /// <summary>
    /// OpenAI's GPT models.
    /// </summary>
    OpenAI = 1,
    
    /// <summary>
    /// Local models via Ollama (future support).
    /// </summary>
    Ollama = 2
}

/// <summary>
/// Represents the available Claude model variants.
/// </summary>
public enum ClaudeModel
{
    /// <summary>
    /// Claude 3.5 Sonnet - Balanced quality and speed.
    /// </summary>
    Claude35Sonnet = 0,
    
    /// <summary>
    /// Claude 3 Opus - Highest quality, slower.
    /// </summary>
    Claude3Opus = 1,
    
    /// <summary>
    /// Claude 3.5 Haiku - Fast and cost-effective.
    /// </summary>
    Claude35Haiku = 2
}

/// <summary>
/// Represents the available OpenAI model variants.
/// </summary>
public enum OpenAIModel
{
    /// <summary>
    /// GPT-4o - Latest multimodal model.
    /// </summary>
    GPT4o = 0,
    
    /// <summary>
    /// GPT-4 Turbo - High quality with longer context.
    /// </summary>
    GPT4Turbo = 1,
    
    /// <summary>
    /// GPT-3.5 Turbo - Fast and economical.
    /// </summary>
    GPT35Turbo = 2
}

/// <summary>
/// Represents point of view options for writing.
/// </summary>
public enum PointOfView
{
    /// <summary>
    /// First person narrative (I, me, my).
    /// </summary>
    FirstPerson = 0,
    
    /// <summary>
    /// Second person narrative (you, your).
    /// </summary>
    SecondPerson = 1,
    
    /// <summary>
    /// Third person limited - follows one character's perspective.
    /// </summary>
    ThirdPersonLimited = 2,
    
    /// <summary>
    /// Third person omniscient - knows all characters' thoughts.
    /// </summary>
    ThirdPersonOmniscient = 3
}

/// <summary>
/// Represents tense options for writing.
/// </summary>
public enum Tense
{
    /// <summary>
    /// Past tense (walked, said, thought).
    /// </summary>
    Past = 0,
    
    /// <summary>
    /// Present tense (walks, says, thinks).
    /// </summary>
    Present = 1
}

/// <summary>
/// Represents export format options.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Microsoft Word document (.docx).
    /// </summary>
    Docx = 0,
    
    /// <summary>
    /// Portable Document Format (.pdf).
    /// </summary>
    Pdf = 1,
    
    /// <summary>
    /// Electronic Publication format (.epub).
    /// </summary>
    Epub = 2,
    
    /// <summary>
    /// Markdown format (.md).
    /// </summary>
    Markdown = 3,
    
    /// <summary>
    /// Plain text format (.txt).
    /// </summary>
    PlainText = 4,
    
    /// <summary>
    /// HTML format (.html).
    /// </summary>
    Html = 5
}

/// <summary>
/// Represents book template categories.
/// </summary>
public enum BookCategory
{
    /// <summary>
    /// Fiction - novels, short stories, etc.
    /// </summary>
    Fiction = 0,
    
    /// <summary>
    /// Non-fiction - guides, self-help, business, etc.
    /// </summary>
    NonFiction = 1,
    
    /// <summary>
    /// Special formats - children's books, poetry, etc.
    /// </summary>
    Special = 2
}

/// <summary>
/// Represents the status of a chapter.
/// </summary>
public enum ChapterStatus
{
    /// <summary>
    /// Chapter has not been started.
    /// </summary>
    NotStarted = 0,
    
    /// <summary>
    /// Chapter outline is complete.
    /// </summary>
    Outlined = 1,
    
    /// <summary>
    /// First draft is in progress.
    /// </summary>
    Drafting = 2,
    
    /// <summary>
    /// First draft is complete.
    /// </summary>
    FirstDraft = 3,
    
    /// <summary>
    /// Chapter is being revised.
    /// </summary>
    Revising = 4,
    
    /// <summary>
    /// Chapter is complete and polished.
    /// </summary>
    Complete = 5
}

/// <summary>
/// Represents the type of a chapter.
/// </summary>
public enum ChapterType
{
    /// <summary>
    /// Standard chapter.
    /// </summary>
    Standard = 0,
    
    /// <summary>
    /// Prologue chapter.
    /// </summary>
    Prologue = 1,
    
    /// <summary>
    /// Epilogue chapter.
    /// </summary>
    Epilogue = 2,
    
    /// <summary>
    /// Interlude chapter.
    /// </summary>
    Interlude = 3,
    
    /// <summary>
    /// Flashback chapter.
    /// </summary>
    Flashback = 4
}

/// <summary>
/// Represents the role of a character in the story.
/// </summary>
public enum CharacterRole
{
    /// <summary>
    /// Main character, hero of the story.
    /// </summary>
    Protagonist = 0,
    
    /// <summary>
    /// Main opposing force.
    /// </summary>
    Antagonist = 1,
    
    /// <summary>
    /// Important secondary character.
    /// </summary>
    Supporting = 2,
    
    /// <summary>
    /// Less important character.
    /// </summary>
    Minor = 3,
    
    /// <summary>
    /// Guide or teacher character.
    /// </summary>
    Mentor = 4,
    
    /// <summary>
    /// Romantic interest.
    /// </summary>
    LoveInterest = 5,
    
    /// <summary>
    /// Loyal companion.
    /// </summary>
    Sidekick = 6,
    
    /// <summary>
    /// Story narrator.
    /// </summary>
    Narrator = 7
}

/// <summary>
/// Represents the type of outline item.
/// </summary>
public enum OutlineItemType
{
    /// <summary>
    /// Major section/act of the book.
    /// </summary>
    Act = 0,
    
    /// <summary>
    /// Part/section within an act.
    /// </summary>
    Part = 1,
    
    /// <summary>
    /// A chapter.
    /// </summary>
    Chapter = 2,
    
    /// <summary>
    /// A scene within a chapter.
    /// </summary>
    Scene = 3,
    
    /// <summary>
    /// A story beat within a scene.
    /// </summary>
    Beat = 4,
    
    /// <summary>
    /// A note/comment in the outline.
    /// </summary>
    Note = 5
}

/// <summary>
/// Represents the status of a project.
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Project is in initial planning phase.
    /// </summary>
    Planning = 0,
    
    /// <summary>
    /// Outline is being developed.
    /// </summary>
    Outlining = 1,
    
    /// <summary>
    /// Writing is in progress.
    /// </summary>
    Writing = 2,
    
    /// <summary>
    /// Project is being edited/revised.
    /// </summary>
    Editing = 3,
    
    /// <summary>
    /// Project is complete.
    /// </summary>
    Complete = 4,
    
    /// <summary>
    /// Project has been published.
    /// </summary>
    Published = 5
}

/// <summary>
/// Represents the type of scene break.
/// </summary>
public enum SceneBreakStyle
{
    /// <summary>
    /// Three asterisks (***).
    /// </summary>
    Asterisks = 0,
    
    /// <summary>
    /// Three pound signs (###).
    /// </summary>
    HashMarks = 1,
    
    /// <summary>
    /// Horizontal rule line.
    /// </summary>
    HorizontalRule = 2,
    
    /// <summary>
    /// Extra blank line.
    /// </summary>
    BlankLine = 3,
    
    /// <summary>
    /// Custom symbol defined by user.
    /// </summary>
    Custom = 4
}
