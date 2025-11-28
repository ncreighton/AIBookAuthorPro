// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIBookAuthorPro.Infrastructure.Data;

/// <summary>
/// Application database context for Entity Framework Core.
/// Supports both SQLite (local) and PostgreSQL (Neon cloud sync).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<BookProjectEntity> BookProjects => Set<BookProjectEntity>();
    public DbSet<ChapterEntity> Chapters => Set<ChapterEntity>();
    public DbSet<GeneratedContentEntity> GeneratedContent => Set<GeneratedContentEntity>();
    public DbSet<ExportHistoryEntity> ExportHistory => Set<ExportHistoryEntity>();
    public DbSet<CharacterEntity> Characters => Set<CharacterEntity>();
    public DbSet<LocationEntity> Locations => Set<LocationEntity>();
    public DbSet<WizardSessionEntity> WizardSessions => Set<WizardSessionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Book project configuration
        modelBuilder.Entity<BookProjectEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.TargetAudience).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Draft");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.BookProjects)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Chapter configuration
        modelBuilder.Entity<ChapterEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BookProjectId, e.ChapterNumber }).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.BookProject)
                .WithMany(b => b.Chapters)
                .HasForeignKey(e => e.BookProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Generated content configuration
        modelBuilder.Entity<GeneratedContentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChapterId);
            entity.Property(e => e.ContentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ModelUsed).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.GeneratedContent)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Export history configuration
        modelBuilder.Entity<ExportHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BookProjectId);
            entity.Property(e => e.Format).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.BookProject)
                .WithMany(b => b.ExportHistory)
                .HasForeignKey(e => e.BookProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Character configuration
        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BookProjectId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.Archetype).HasMaxLength(100);

            entity.HasOne(e => e.BookProject)
                .WithMany(b => b.Characters)
                .HasForeignKey(e => e.BookProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Location configuration
        modelBuilder.Entity<LocationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BookProjectId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(e => e.BookProject)
                .WithMany(b => b.Locations)
                .HasForeignKey(e => e.BookProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Wizard session configuration
        modelBuilder.Entity<WizardSessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("InProgress");
            entity.Property(e => e.CurrentStep).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BookProject)
                .WithMany()
                .HasForeignKey(e => e.BookProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
