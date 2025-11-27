// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Windows;
using AIBookAuthorPro.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AIBookAuthorPro;

/// <summary>
/// Application entry point and DI container configuration.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services => _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the App class.
    /// </summary>
    public App()
    {
        // Build configuration
        _configuration = BuildConfiguration();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AIBookAuthorPro", "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .WriteTo.Console()
            .CreateLogger();

        // Build DI container
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        Log.Information("AI Book Author Pro starting...");
    }

    /// <summary>
    /// Configures the application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        services.AddSingleton(_configuration);

        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Register ViewModels
        RegisterViewModels(services);

        // Register Services
        RegisterServices(services);

        // Register Views
        RegisterViews(services);
    }

    /// <summary>
    /// Registers ViewModels with the DI container.
    /// </summary>
    private static void RegisterViewModels(IServiceCollection services)
    {
        // Main ViewModels
        services.AddTransient<UI.ViewModels.MainWindowViewModel>();
        services.AddTransient<UI.ViewModels.DashboardViewModel>();
        services.AddTransient<UI.ViewModels.ProjectEditorViewModel>();
        services.AddTransient<UI.ViewModels.ChapterEditorViewModel>();
        services.AddTransient<UI.ViewModels.SettingsViewModel>();
    }

    /// <summary>
    /// Registers application services with the DI container.
    /// </summary>
    private static void RegisterServices(IServiceCollection services)
    {
        // TODO: Register services as they are implemented
        // services.AddSingleton<IProjectService, ProjectService>();
        // services.AddSingleton<IAIProviderFactory, AIProviderFactory>();
        // services.AddHttpClient<ClaudeProvider>();
        // services.AddHttpClient<OpenAIProvider>();
    }

    /// <summary>
    /// Registers Views with the DI container.
    /// </summary>
    private static void RegisterViews(IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
    }

    /// <summary>
    /// Builds the application configuration.
    /// </summary>
    /// <returns>The configuration root.</returns>
    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", 
                optional: true, reloadOnChange: true)
            .AddEnvironmentVariables("AIBOOKAUTHOR_");

        return builder.Build();
    }

    /// <summary>
    /// Handles application startup.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up global exception handling
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        // Create and show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Log.Information("AI Book Author Pro started successfully");
    }

    /// <summary>
    /// Handles application exit.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("AI Book Author Pro shutting down...");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    /// <summary>
    /// Handles unhandled exceptions on the dispatcher thread.
    /// </summary>
    private void App_DispatcherUnhandledException(object sender, 
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception on dispatcher thread");
        
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will attempt to continue.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    /// <summary>
    /// Handles unhandled exceptions from non-UI threads.
    /// </summary>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Fatal unhandled exception");
        }
    }

    /// <summary>
    /// Handles unobserved task exceptions.
    /// </summary>
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }
}
