// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data;

/// <summary>
/// Database configuration settings.
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Database provider: "SQLite" or "PostgreSQL"
    /// </summary>
    public string Provider { get; set; } = "SQLite";

    /// <summary>
    /// Connection string for the database.
    /// For SQLite: "Data Source=aibook.db"
    /// For PostgreSQL/Neon: Full connection string from environment
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=aibook.db";

    /// <summary>
    /// Enable cloud sync with Neon PostgreSQL.
    /// </summary>
    public bool EnableCloudSync { get; set; } = false;

    /// <summary>
    /// Neon PostgreSQL connection string for cloud sync.
    /// </summary>
    public string? NeonConnectionString { get; set; }

    /// <summary>
    /// Sync interval in seconds (default: 5 minutes).
    /// </summary>
    public int SyncIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Enable automatic migrations on startup.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;
}
