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
/// Book project repository implementation.
/// </summary>
public class BookProjectRepository : IBookProjectRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookProjectRepository> _logger;

    public BookProjectRepository(AppDbContext context, ILogger<BookProjectRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<BookProjectEntity>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var project = await _context.BookProjects
                .Include(p => p.Chapters.OrderBy(c => c.ChapterNumber))
                .Include(p => p.Characters.OrderBy(c => c.SortOrder))
                .Include(p => p.Locations.OrderBy(l => l.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            return project != null
                ? Result<BookProjectEntity>.Success(project)
                : Result<BookProjectEntity>.Failure("Project not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return Result<BookProjectEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<List<BookProjectEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var projects = await _context.BookProjects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync(ct);

            return Result<List<BookProjectEntity>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for user {UserId}", userId);
            return Result<List<BookProjectEntity>>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<List<BookProjectEntity>>> GetRecentAsync(Guid userId, int count = 10, CancellationToken ct = default)
    {
        try
        {
            var projects = await _context.BookProjects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .Take(count)
                .ToListAsync(ct);

            return Result<List<BookProjectEntity>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent projects for user {UserId}", userId);
            return Result<List<BookProjectEntity>>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<BookProjectEntity>> CreateAsync(BookProjectEntity project, CancellationToken ct = default)
    {
        try
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _context.BookProjects.Add(project);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Created book project {ProjectId}: {Title}", project.Id, project.Title);
            return Result<BookProjectEntity>.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return Result<BookProjectEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<BookProjectEntity>> UpdateAsync(BookProjectEntity project, CancellationToken ct = default)
    {
        try
        {
            project.UpdatedAt = DateTime.UtcNow;
            _context.BookProjects.Update(project);
            await _context.SaveChangesAsync(ct);
            return Result<BookProjectEntity>.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", project.Id);
            return Result<BookProjectEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var project = await _context.BookProjects.FindAsync(new object[] { id }, ct);
            if (project == null)
                return Result.Failure("Project not found");

            _context.BookProjects.Remove(project);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted book project {ProjectId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return Result.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetTotalWordCountAsync(Guid projectId, CancellationToken ct = default)
    {
        try
        {
            var totalWords = await _context.Chapters
                .Where(c => c.BookProjectId == projectId)
                .SumAsync(c => c.WordCount, ct);

            return Result<int>.Success(totalWords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating word count for project {ProjectId}", projectId);
            return Result<int>.Failure($"Database error: {ex.Message}", ex);
        }
    }
}
