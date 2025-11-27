// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure.AI;
using AIBookAuthorPro.Infrastructure.Export;
using AIBookAuthorPro.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // HTTP clients for AI providers
        services.AddHttpClient<AnthropicProvider>();
        services.AddHttpClient<OpenAIProvider>();
        services.AddHttpClient<GeminiProvider>();

        // AI Providers
        services.AddSingleton<AnthropicProvider>();
        services.AddSingleton<OpenAIProvider>();
        services.AddSingleton<GeminiProvider>();
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

        // Core services
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // FlowDocument service (WPF-specific, but lives in Infrastructure)
        services.AddSingleton<IFlowDocumentService, FlowDocumentService>();

        // Context building service
        services.AddSingleton<IContextBuilderService, ContextBuilderService>();

        // Generation pipeline service
        services.AddSingleton<IGenerationPipelineService, GenerationPipelineService>();

        // Export services
        services.AddSingleton<IDocxExporter, DocxExporter>();
        services.AddSingleton<IMarkdownExporter, MarkdownExporter>();
        services.AddSingleton<IHtmlExporter, HtmlExporter>();
        services.AddSingleton<IExportService, ExportService>();

        // Legacy exporters (for backward compatibility)
        services.AddSingleton<IDocumentExporter, Export.DocxExporter>();
        services.AddSingleton<DocumentExporterFactory>();

        return services;
    }
}

/// <summary>
/// Factory for getting document exporters by format.
/// </summary>
public sealed class DocumentExporterFactory
{
    private readonly IEnumerable<IDocumentExporter> _exporters;

    public DocumentExporterFactory(IEnumerable<IDocumentExporter> exporters)
    {
        _exporters = exporters;
    }

    /// <summary>
    /// Gets an exporter for the specified format.
    /// </summary>
    public IDocumentExporter? GetExporter(ExportFormat format)
    {
        return _exporters.FirstOrDefault(e => e.Format == format);
    }

    /// <summary>
    /// Gets all available exporters.
    /// </summary>
    public IReadOnlyList<IDocumentExporter> GetAllExporters()
    {
        return _exporters.ToList();
    }
}
