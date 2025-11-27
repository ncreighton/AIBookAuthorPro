// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure.Services;
using AIBookAuthorPro.UI.ViewModels;
using AIBookAuthorPro.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.App;

/// <summary>
/// Application entry point.
/// </summary>
public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;
    private ILogger<App>? _logger;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        _logger.LogInformation("AI Book Author Pro starting up...");

        // Set up global exception handling
        SetupExceptionHandling();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        _logger.LogInformation("Application started successfully");
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug();
        });

        // Core Services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<IErrorHandler, ErrorHandler>();

        // AI Services
        services.AddSingleton<ITokenCounter, TokenCounter>();
        services.AddTransient<IAIProvider, ClaudeProvider>();
        services.AddTransient<IAIProviderFactory, AIProviderFactory>();
        services.AddTransient<IChapterGeneratorService, ChapterGeneratorService>();
        services.AddTransient<IContextBuilder, ContextBuilder>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ProjectDashboardViewModel>();
        services.AddTransient<ChapterEditorViewModel>();
        services.AddTransient<AIGenerationViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<CharacterListViewModel>();
        services.AddTransient<CharacterEditorViewModel>();
        services.AddTransient<LocationListViewModel>();
        services.AddTransient<LocationEditorViewModel>();
        services.AddTransient<OutlineEditorViewModel>();
        services.AddTransient<ExportViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<ProjectDashboardView>();
        services.AddTransient<ChapterEditorView>();
        services.AddTransient<AIGenerationDialog>();
        services.AddTransient<SettingsView>();
        services.AddTransient<CharacterListView>();
        services.AddTransient<CharacterEditorView>();
        services.AddTransient<LocationListView>();
        services.AddTransient<LocationEditorView>();
        services.AddTransient<OutlineEditorView>();
        services.AddTransient<ExportDialogView>();
    }

    private void SetupExceptionHandling()
    {
        // Handle unhandled exceptions on the UI thread
        DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "Unhandled UI exception");

            var errorHandler = _serviceProvider.GetService<IErrorHandler>();
            errorHandler?.Handle(args.Exception, "Unhandled UI Exception");

            args.Handled = true;
        };

        // Handle unhandled exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            _logger?.LogCritical(exception, "Unhandled domain exception");
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }

    /// <inheritdoc />
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
