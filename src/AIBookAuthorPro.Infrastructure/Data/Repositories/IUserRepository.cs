// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Infrastructure.Data.Entities;

namespace AIBookAuthorPro.Infrastructure.Data;

/// <summary>
/// Repository for user data access.
/// </summary>
public interface IUserRepository
{
    Task<Result<UserEntity>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserEntity>> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<UserEntity>> GetByExternalAuthIdAsync(string externalId, CancellationToken ct = default);
    Task<Result<UserEntity>> CreateAsync(UserEntity user, CancellationToken ct = default);
    Task<Result<UserEntity>> UpdateAsync(UserEntity user, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserEntity>> GetOrCreateByExternalAuthAsync(string externalId, string email, string? displayName = null, CancellationToken ct = default);
}

/// <summary>
/// Repository for book project data access.
/// </summary>
public interface IBookProjectRepository
{
    Task<Result<BookProjectEntity>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<BookProjectEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<List<BookProjectEntity>>> GetRecentAsync(Guid userId, int count = 10, CancellationToken ct = default);
    Task<Result<BookProjectEntity>> CreateAsync(BookProjectEntity project, CancellationToken ct = default);
    Task<Result<BookProjectEntity>> UpdateAsync(BookProjectEntity project, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<int>> GetTotalWordCountAsync(Guid projectId, CancellationToken ct = default);
}

/// <summary>
/// Repository for chapter data access.
/// </summary>
public interface IChapterRepository
{
    Task<Result<ChapterEntity>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ChapterEntity>> GetByNumberAsync(Guid bookProjectId, int chapterNumber, CancellationToken ct = default);
    Task<Result<List<ChapterEntity>>> GetByBookProjectIdAsync(Guid bookProjectId, CancellationToken ct = default);
    Task<Result<ChapterEntity>> CreateAsync(ChapterEntity chapter, CancellationToken ct = default);
    Task<Result<ChapterEntity>> UpdateAsync(ChapterEntity chapter, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<ChapterEntity>>> GetPendingChaptersAsync(Guid bookProjectId, CancellationToken ct = default);
}

/// <summary>
/// Repository for wizard session data access.
/// </summary>
public interface IWizardSessionRepository
{
    Task<Result<WizardSessionEntity>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<WizardSessionEntity?>> GetActiveSessionAsync(Guid userId, CancellationToken ct = default);
    Task<Result<List<WizardSessionEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<WizardSessionEntity>> CreateAsync(WizardSessionEntity session, CancellationToken ct = default);
    Task<Result<WizardSessionEntity>> UpdateAsync(WizardSessionEntity session, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
