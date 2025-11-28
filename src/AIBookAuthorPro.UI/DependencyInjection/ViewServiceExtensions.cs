// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.UI.Views;
using AIBookAuthorPro.UI.Views.GuidedCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.UI.DependencyInjection;

/// <summary>
/// Extension methods for registering View services.
/// </summary>
public static class ViewServiceExtensions
{
    /// <summary>
    /// Adds all Views to the service collection.
    /// </summary>
    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        // Main window
        services.AddSingleton<MainWindow>();

        // Main views
        services.AddTransient<QuickStartView>();
        services.AddTransient<ProjectDashboardView>();
        services.AddTransient<ChapterEditorView>();
        services.AddTransient<CharacterListView>();
        services.AddTransient<LocationListView>();
        services.AddTransient<OutlineEditorView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<ExportDialogView>();

        // Guided Creation views
        services.AddTransient<GuidedCreationWizardView>();
        services.AddTransient<PromptEntryView>();
        services.AddTransient<PromptAnalysisView>();
        services.AddTransient<BlueprintReviewView>();
        services.AddTransient<GenerationDashboardView>();

        return services;
    }
}
