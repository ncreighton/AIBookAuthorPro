// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Infrastructure.Data.Entities;

/// <summary>
/// User entity for database persistence.
/// </summary>
public class UserEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ExternalAuthId { get; set; }  // Stack Auth user ID
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Preferences stored as JSON
    public string? PreferencesJson { get; set; }

    // Navigation properties
    public virtual ICollection<BookProjectEntity> BookProjects { get; set; } = new List<BookProjectEntity>();
}
