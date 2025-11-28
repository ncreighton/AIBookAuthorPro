// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.UI.ViewModels;
using AIBookAuthorPro.UI.ViewModels.GuidedCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.UI.DependencyInjection;

/// <summary>
/// Extension methods for registering ViewModel services.
/// </summary>
public static class ViewModelServiceExtensions
{
    /// <summary>
    /// Adds all ViewModels to the service collection.
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Main application ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EditorViewModel>();
        services.AddTransient<CharacterViewModel>();
        services.AddTransient<LocationViewModel>();
        services.AddTransient<OutlineViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ExportDialogViewModel>();

        // Guided Creation ViewModels
        services.AddTransient<GuidedCreationWizardViewModel>();
        services.AddTransient<PromptEntryViewModel>();
        services.AddTransient<PromptAnalysisViewModel>();
        services.AddTransient<BlueprintReviewViewModel>();
        services.AddTransient<GenerationDashboardViewModel>();

        return services;
    }
}
