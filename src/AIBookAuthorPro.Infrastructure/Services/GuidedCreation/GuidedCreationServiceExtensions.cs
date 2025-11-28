// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Extension methods for registering Guided Creation services.
/// </summary>
public static class GuidedCreationServiceExtensions
{
    /// <summary>
    /// Adds all Guided Creation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGuidedCreationServices(this IServiceCollection services)
    {
        // Core Guided Creation Services
        services.AddTransient<IPromptAnalysisService, PromptAnalysisService>();
        services.AddTransient<IBlueprintGeneratorService, BlueprintGeneratorService>();
        services.AddTransient<IBookGenerationOrchestrator, BookGenerationOrchestrator>();
        services.AddTransient<IQualityEvaluationService, QualityEvaluationService>();
        services.AddTransient<IContinuityVerificationService, ContinuityVerificationService>();
        services.AddTransient<IGenerationContextBuilder, GenerationContextBuilder>();
        services.AddTransient<IGuidedCreationWizardService, GuidedCreationWizardService>();
        services.AddTransient<IChapterGenerationPipeline, ChapterGenerationPipeline>();

        return services;
    }
}
