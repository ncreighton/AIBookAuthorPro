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
/// Wizard session repository implementation.
/// </summary>
public class WizardSessionRepository : IWizardSessionRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WizardSessionRepository> _logger;

    public WizardSessionRepository(AppDbContext context, ILogger<WizardSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<WizardSessionEntity>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var session = await _context.WizardSessions
                .Include(s => s.BookProject)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return session != null
                ? Result<WizardSessionEntity>.Success(session)
                : Result<WizardSessionEntity>.Failure("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wizard session {SessionId}", id);
            return Result<WizardSessionEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<WizardSessionEntity?>> GetActiveSessionAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var session = await _context.WizardSessions
                .Where(s => s.UserId == userId && s.Status == "InProgress")
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync(ct);

            return Result<WizardSessionEntity?>.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active session for user {UserId}", userId);
            return Result<WizardSessionEntity?>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<List<WizardSessionEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var sessions = await _context.WizardSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync(ct);

            return Result<List<WizardSessionEntity>>.Success(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            return Result<List<WizardSessionEntity>>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<WizardSessionEntity>> CreateAsync(WizardSessionEntity session, CancellationToken ct = default)
    {
        try
        {
            session.CreatedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            _context.WizardSessions.Add(session);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Created wizard session {SessionId} for user {UserId}", session.Id, session.UserId);
            return Result<WizardSessionEntity>.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wizard session");
            return Result<WizardSessionEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<WizardSessionEntity>> UpdateAsync(WizardSessionEntity session, CancellationToken ct = default)
    {
        try
        {
            session.UpdatedAt = DateTime.UtcNow;
            _context.WizardSessions.Update(session);
            await _context.SaveChangesAsync(ct);
            return Result<WizardSessionEntity>.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wizard session {SessionId}", session.Id);
            return Result<WizardSessionEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var session = await _context.WizardSessions.FindAsync(new object[] { id }, ct);
            if (session == null)
                return Result.Failure("Session not found");

            _context.WizardSessions.Remove(session);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wizard session {SessionId}", id);
            return Result.Failure($"Database error: {ex.Message}");
        }
    }
}
