// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows.Controls;
using AIBookAuthorPro.Core.Enums;
using UserControl = System.Windows.Controls.UserControl;

namespace AIBookAuthorPro.UI.Views;

/// <summary>
/// Interaction logic for ChapterEditorView.xaml
/// </summary>
public partial class ChapterEditorView : UserControl
{
    /// <summary>
    /// Initializes a new instance of ChapterEditorView.
    /// </summary>
    public ChapterEditorView()
    {
        InitializeComponent();
    }
}

/// <summary>
/// Provides chapter status options for binding.
/// </summary>
public static class ChapterStatusOptions
{
    /// <summary>
    /// Gets all chapter status values.
    /// </summary>
    public static ChapterStatus[] All { get; } =
    [
        ChapterStatus.NotStarted,
        ChapterStatus.Drafting,
        ChapterStatus.FirstDraft,
        ChapterStatus.Revising,
        ChapterStatus.Complete
    ];
}
