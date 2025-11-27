// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.UI.ViewModels;
using AIBookAuthorPro.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.UI;

/// <summary>
/// Extension methods for registering UI services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds UI services to the service collection.
    /// </summary>
    public static IServiceCollection AddUI(this IServiceCollection services)
    {
        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<ChapterEditorViewModel>();
        services.AddTransient<AIGenerationViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<ChapterEditorView>();
        services.AddTransient<AIGenerationDialog>();

        return services;
    }
}
