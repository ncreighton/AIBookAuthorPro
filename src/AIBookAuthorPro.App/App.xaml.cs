// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.IO;
using System.Windows;
using System.Windows.Threading;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure;
using AIBookAuthorPro.Infrastructure.AI;
using AIBookAuthorPro.UI.ViewModels;
using AIBookAuthorPro.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AIBookAuthorPro.App;

/// <summary>
/// Application entry point with dependency injection and configuration setup.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public App()
    {
        // Build configuration
        _configuration = BuildConfiguration();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AIBookAuthorPro",
                    "logs",
                    "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("AI Book Author Pro starting...");

        // Build service provider
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Configures application services.
    /// </summary>
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

        // AI Provider settings
        services.Configure<AnthropicSettings>(
            _configuration.GetSection("AIProviders:Anthropic"));
        services.Configure<OpenAISettings>(
            _configuration.GetSection("AIProviders:OpenAI"));
        services.Configure<GeminiSettings>(
            _configuration.GetSection("AIProviders:Gemini"));

        // Infrastructure services
        services.AddInfrastructure();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    /// <summary>
    /// Builds the configuration from appsettings files and environment variables.
    /// </summary>
    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables("AIBOOKAUTHOR_");

        // Check for user secrets in development
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (env == "Development")
        {
            // User secrets would be added here for development
        }

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

        try
        {
            // Create and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("AI Book Author Pro started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show(
                $"Failed to start application:\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
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
        DispatcherUnhandledExceptionEventArgs e)
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