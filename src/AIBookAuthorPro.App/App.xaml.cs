// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure.AI;
using AIBookAuthorPro.Infrastructure.Services;
using AIBookAuthorPro.UI.ViewModels;
using AIBookAuthorPro.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.App;

/// &lt;summary&gt;
/// Application entry point.
/// &lt;/summary&gt;
public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;
    private ILogger&lt;App&gt;? _logger;

    /// &lt;inheritdoc /&gt;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService&lt;ILogger&lt;App&gt;&gt;();
        _logger.LogInformation("AI Book Author Pro starting up...");

        // Set up global exception handling
        SetupExceptionHandling();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService&lt;MainWindow&gt;();
        mainWindow.Show();

        _logger.LogInformation("Application started successfully");
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =&gt;
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug();
        });

        // Core Services
        services.AddSingleton&lt;IFileSystemService, FileSystemService&gt;();
        services.AddSingleton&lt;ISettingsService, SettingsService&gt;();
        services.AddSingleton&lt;IProjectService, ProjectService&gt;();
        services.AddSingleton&lt;IExportService, ExportService&gt;();
        services.AddSingleton&lt;INotificationService, NotificationService&gt;();
        services.AddSingleton&lt;NotificationService&gt;(); // Concrete type for NotificationHost
        services.AddSingleton&lt;IErrorHandler, ErrorHandler&gt;();
        services.AddSingleton&lt;IFlowDocumentService, FlowDocumentService&gt;();

        // AI Services
        services.AddSingleton&lt;ITokenCounter, TokenCounter&gt;();
        services.AddSingleton&lt;IAIProviderFactory, AIProviderFactory&gt;();
        services.AddTransient&lt;IContextBuilderService, ContextBuilderService&gt;();
        services.AddTransient&lt;IChapterGeneratorService, ChapterGeneratorService&gt;();
        services.AddTransient&lt;IGenerationPipelineService, GenerationPipelineService&gt;();
        
        // Register AI providers
        services.AddTransient&lt;AnthropicProvider&gt;();
        services.AddTransient&lt;OpenAIProvider&gt;();
        services.AddTransient&lt;GeminiProvider&gt;();

        // ViewModels
        services.AddSingleton&lt;MainViewModel&gt;();
        services.AddTransient&lt;ProjectDashboardViewModel&gt;();
        services.AddTransient&lt;ChapterEditorViewModel&gt;();
        services.AddTransient&lt;AIGenerationViewModel&gt;();
        services.AddTransient&lt;SettingsViewModel&gt;();
        services.AddTransient&lt;CharacterListViewModel&gt;();
        services.AddTransient&lt;CharacterEditorViewModel&gt;();
        services.AddTransient&lt;LocationListViewModel&gt;();
        services.AddTransient&lt;LocationEditorViewModel&gt;();
        services.AddTransient&lt;OutlineEditorViewModel&gt;();
        services.AddTransient&lt;ExportViewModel&gt;();

        // Views
        services.AddTransient&lt;MainWindow&gt;();
        services.AddTransient&lt;ProjectDashboardView&gt;();
        services.AddTransient&lt;ChapterEditorView&gt;();
        services.AddTransient&lt;AIGenerationDialog&gt;();
        services.AddTransient&lt;SettingsView&gt;();
        services.AddTransient&lt;CharacterListView&gt;();
        services.AddTransient&lt;CharacterEditorView&gt;();
        services.AddTransient&lt;LocationListView&gt;();
        services.AddTransient&lt;LocationEditorView&gt;();
        services.AddTransient&lt;OutlineEditorView&gt;();
        services.AddTransient&lt;ExportDialogView&gt;();
    }

    private void SetupExceptionHandling()
    {
        // Handle unhandled exceptions on the UI thread
        DispatcherUnhandledException += (sender, args) =&gt;
        {
            _logger?.LogError(args.Exception, "Unhandled UI exception");

            var errorHandler = _serviceProvider.GetService&lt;IErrorHandler&gt;();
            errorHandler?.Handle(args.Exception, "Unhandled UI Exception");

            args.Handled = true;
        };

        // Handle unhandled exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =&gt;
        {
            var exception = args.ExceptionObject as Exception;
            _logger?.LogCritical(exception, "Unhandled domain exception");
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (sender, args) =&gt;
        {
            _logger?.LogError(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }

    /// &lt;inheritdoc /&gt;
    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("AI Book Author Pro shutting down...");

        // Dispose service provider if it implements IDisposable
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
