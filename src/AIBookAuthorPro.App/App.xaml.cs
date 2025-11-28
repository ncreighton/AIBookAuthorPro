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
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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

        try
        {
            System.Diagnostics.Debug.WriteLine("APP: Starting OnStartup");
            Console.WriteLine("APP: Starting OnStartup");

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            System.Diagnostics.Debug.WriteLine("APP: Building service provider");
            Console.WriteLine("APP: Building service provider");
            
            _serviceProvider = services.BuildServiceProvider();

            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("AI Book Author Pro starting up...");
            Console.WriteLine("APP: Logger created");

            // Set up global exception handling
            SetupExceptionHandling();

            System.Diagnostics.Debug.WriteLine("APP: Creating MainWindow");
            Console.WriteLine("APP: Creating MainWindow");

            // Show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            
            System.Diagnostics.Debug.WriteLine("APP: MainWindow created, showing...");
            Console.WriteLine("APP: MainWindow created, showing...");
            
            mainWindow.Show();

            _logger.LogInformation("Application started successfully");
            Console.WriteLine("APP: Application started successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"APP ERROR: {ex}");
            Console.WriteLine($"APP ERROR: {ex}");
            MessageBox.Show($"Failed to start application:\n\n{ex.Message}\n\n{ex.StackTrace}", 
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        Console.WriteLine("APP: ConfigureServices starting");
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddDebug();
        });

        Console.WriteLine("APP: Registering core services");
        
        // Core Services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<NotificationService>(); // Concrete type for NotificationHost
        services.AddSingleton<IErrorHandler, ErrorHandler>();
        services.AddSingleton<IFlowDocumentService, FlowDocumentService>();
        
        // Export services (must be registered before IExportService)
        services.AddSingleton<IDocxExporter, DocxExporter>();
        services.AddSingleton<IMarkdownExporter, MarkdownExporter>();
        services.AddSingleton<IHtmlExporter, HtmlExporter>();
        services.AddSingleton<IExportService, ExportService>();

        Console.WriteLine("APP: Registering AI services");
        
        // AI Services
        services.AddSingleton<ITokenCounter, TokenCounter>();
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();
        services.AddTransient<IContextBuilderService, ContextBuilderService>();
        services.AddTransient<IChapterGeneratorService, ChapterGeneratorService>();
        services.AddTransient<IGenerationPipelineService, GenerationPipelineService>();
        
        // Register AI providers
        services.AddTransient<AnthropicProvider>();
        services.AddTransient<OpenAIProvider>();
        services.AddTransient<GeminiProvider>();

        Console.WriteLine("APP: Registering ViewModels");
        
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

        Console.WriteLine("APP: Registering Views");
        
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
        
        Console.WriteLine("APP: ConfigureServices complete");
    }

    private void SetupExceptionHandling()
    {
        // Handle unhandled exceptions on the UI thread
        DispatcherUnhandledException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "Unhandled UI exception");
            Console.WriteLine($"UI EXCEPTION: {args.Exception}");

            var errorHandler = _serviceProvider.GetService<IErrorHandler>();
            errorHandler?.Handle(args.Exception, "Unhandled UI Exception");

            args.Handled = true;
        };

        // Handle unhandled exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            _logger?.LogCritical(exception, "Unhandled domain exception");
            Console.WriteLine($"DOMAIN EXCEPTION: {exception}");
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            _logger?.LogError(args.Exception, "Unobserved task exception");
            Console.WriteLine($"TASK EXCEPTION: {args.Exception}");
            args.SetObserved();
        };
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("AI Book Author Pro shutting down...");
        Console.WriteLine("APP: Shutting down");

        // Dispose service provider if it implements IDisposable
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
