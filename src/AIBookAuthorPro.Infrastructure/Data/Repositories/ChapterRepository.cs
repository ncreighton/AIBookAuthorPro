// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Data;

/// <summary>
/// Chapter repository implementation.
/// </summary>
public class ChapterRepository : IChapterRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChapterRepository> _logger;

    public ChapterRepository(AppDbContext context, ILogger<ChapterRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ChapterEntity>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var chapter = await _context.Chapters
                .Include(c => c.GeneratedContent.OrderByDescending(gc => gc.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            return chapter != null
                ? Result<ChapterEntity>.Success(chapter)
                : Result<ChapterEntity>.Failure("Chapter not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapter {ChapterId}", id);
            return Result<ChapterEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<ChapterEntity>> GetByNumberAsync(Guid bookProjectId, int chapterNumber, CancellationToken ct = default)
    {
        try
        {
            var chapter = await _context.Chapters
                .Include(c => c.GeneratedContent.OrderByDescending(gc => gc.CreatedAt))
                .FirstOrDefaultAsync(c => c.BookProjectId == bookProjectId && c.ChapterNumber == chapterNumber, ct);

            return chapter != null
                ? Result<ChapterEntity>.Success(chapter)
                : Result<ChapterEntity>.Failure("Chapter not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapter {Number} for project {ProjectId}", chapterNumber, bookProjectId);
            return Result<ChapterEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<List<ChapterEntity>>> GetByBookProjectIdAsync(Guid bookProjectId, CancellationToken ct = default)
    {
        try
        {
            var chapters = await _context.Chapters
                .Where(c => c.BookProjectId == bookProjectId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync(ct);

            return Result<List<ChapterEntity>>.Success(chapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapters for project {ProjectId}", bookProjectId);
            return Result<List<ChapterEntity>>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<ChapterEntity>> CreateAsync(ChapterEntity chapter, CancellationToken ct = default)
    {
        try
        {
            chapter.CreatedAt = DateTime.UtcNow;
            chapter.UpdatedAt = DateTime.UtcNow;

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Created chapter {ChapterNumber} for project {ProjectId}", chapter.ChapterNumber, chapter.BookProjectId);
            return Result<ChapterEntity>.Success(chapter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chapter");
            return Result<ChapterEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<ChapterEntity>> UpdateAsync(ChapterEntity chapter, CancellationToken ct = default)
    {
        try
        {
            chapter.UpdatedAt = DateTime.UtcNow;
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync(ct);
            return Result<ChapterEntity>.Success(chapter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chapter {ChapterId}", chapter.Id);
            return Result<ChapterEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var chapter = await _context.Chapters.FindAsync(new object[] { id }, ct);
            if (chapter == null)
                return Result.Failure("Chapter not found");

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chapter {ChapterId}", id);
            return Result.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<List<ChapterEntity>>> GetPendingChaptersAsync(Guid bookProjectId, CancellationToken ct = default)
    {
        try
        {
            var chapters = await _context.Chapters
                .Where(c => c.BookProjectId == bookProjectId && c.Status == "Pending")
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync(ct);

            return Result<List<ChapterEntity>>.Success(chapters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending chapters for project {ProjectId}", bookProjectId);
            return Result<List<ChapterEntity>>.Failure($"Database error: {ex.Message}", ex);
        }
    }
}
