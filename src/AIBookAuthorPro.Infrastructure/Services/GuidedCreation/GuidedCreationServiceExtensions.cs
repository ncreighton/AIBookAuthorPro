// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.Infrastructure.Services.GuidedCreation;

/// <summary>
/// Extension methods for registering guided creation services.
/// </summary>
public static class GuidedCreationServiceExtensions
{
    /// <summary>
    /// Adds all guided creation services to the service collection.
    /// </summary>
    public static IServiceCollection AddGuidedCreationServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IPromptAnalysisService, PromptAnalysisService>();
        services.AddScoped<IBlueprintGeneratorService, BlueprintGeneratorService>();
        services.AddScoped<IQualityEvaluationService, QualityEvaluationService>();
        services.AddScoped<IContinuityVerificationService, ContinuityVerificationService>();
        services.AddScoped<IGenerationContextBuilder, GenerationContextBuilder>();
        services.AddScoped<IBookGenerationOrchestrator, BookGenerationOrchestrator>();
        services.AddScoped<IGuidedCreationWizardService, GuidedCreationWizardService>();
        
        // Pipeline
        services.AddScoped<IChapterGenerationPipeline, ChapterGenerationPipeline>();

        return services;
    }
}
