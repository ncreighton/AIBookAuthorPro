// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Data;

/// <summary>
/// Extension methods for database service registration.
/// </summary>
public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Adds database services to the service collection.
    /// </summary>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        DatabaseConfiguration config)
    {
        services.AddSingleton(config);

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (config.Provider.ToLowerInvariant())
            {
                case "postgresql":
                case "postgres":
                case "neon":
                    options.UseNpgsql(config.ConnectionString, npgsql =>
                    {
                        npgsql.EnableRetryOnFailure(3);
                        npgsql.CommandTimeout(30);
                    });
                    break;

                case "sqlite":
                default:
                    options.UseSqlite(config.ConnectionString);
                    break;
            }

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookProjectRepository, BookProjectRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<IWizardSessionRepository, WizardSessionRepository>();

        return services;
    }

    /// <summary>
    /// Initializes the database (creates if not exists, runs migrations).
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        ILogger? logger = null)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<DatabaseConfiguration>();

        try
        {
            if (config.AutoMigrate)
            {
                logger?.LogInformation("Applying database migrations...");
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                logger?.LogInformation("Ensuring database is created...");
                await dbContext.Database.EnsureCreatedAsync();
            }

            logger?.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to initialize database");
            throw;
        }
    }
}
