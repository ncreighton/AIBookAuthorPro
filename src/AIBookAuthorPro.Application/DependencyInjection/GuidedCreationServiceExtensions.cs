// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Application.Services.GuidedCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering guided creation services.
/// </summary>
public static class GuidedCreationServiceExtensions
{
    /// <summary>
    /// Adds guided creation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGuidedCreationServices(this IServiceCollection services)
    {
        // Core services
        // services.AddScoped<IPromptAnalysisService, PromptAnalysisService>();
        // services.AddScoped<IBlueprintGeneratorService, BlueprintGeneratorService>();
        // services.AddScoped<IBookGenerationOrchestrator, BookGenerationOrchestrator>();
        // services.AddScoped<IQualityEvaluationService, QualityEvaluationService>();
        // services.AddScoped<IContinuityVerificationService, ContinuityVerificationService>();
        // services.AddScoped<IGenerationContextBuilder, GenerationContextBuilder>();
        // services.AddScoped<IGuidedCreationWizardService, GuidedCreationWizardService>();

        // Pipeline
        // services.AddScoped<IChapterGenerationPipeline, ChapterGenerationPipeline>();
        
        // Pipeline steps
        // services.AddScoped<IBuildContextStep, BuildContextStep>();
        // services.AddScoped<IGenerateOutlineStep, GenerateOutlineStep>();
        // services.AddScoped<IGenerateScenesStep, GenerateScenesStep>();
        // services.AddScoped<IAssembleChapterStep, AssembleChapterStep>();
        // services.AddScoped<IContinuityCheckStep, ContinuityCheckStep>();
        // services.AddScoped<IStyleConsistencyStep, StyleConsistencyStep>();
        // services.AddScoped<IQualityEvaluationStep, QualityEvaluationStep>();
        // services.AddScoped<IRevisionStep, RevisionStep>();
        // services.AddScoped<IFinalizeStep, FinalizeStep>();

        return services;
    }
}
