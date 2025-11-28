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
/// User repository implementation.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<UserEntity>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users.FindAsync(new object[] { id }, ct);
            return user != null
                ? Result<UserEntity>.Success(user)
                : Result<UserEntity>.Failure("User not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<UserEntity>> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
            return user != null
                ? Result<UserEntity>.Success(user)
                : Result<UserEntity>.Failure("User not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email");
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<UserEntity>> GetByExternalAuthIdAsync(string externalId, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ExternalAuthId == externalId, ct);
            return user != null
                ? Result<UserEntity>.Success(user)
                : Result<UserEntity>.Failure("User not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by external auth ID");
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<UserEntity>> CreateAsync(UserEntity user, CancellationToken ct = default)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Created user {UserId}", user.Id);
            return Result<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result<UserEntity>> UpdateAsync(UserEntity user, CancellationToken ct = default)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
            return Result<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", user.Id);
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users.FindAsync(new object[] { id }, ct);
            if (user == null)
                return Result.Failure("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return Result.Failure($"Database error: {ex.Message}");
        }
    }

    public async Task<Result<UserEntity>> GetOrCreateByExternalAuthAsync(
        string externalId, string email, string? displayName = null, CancellationToken ct = default)
    {
        try
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.ExternalAuthId == externalId, ct);

            if (existing != null)
            {
                existing.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return Result<UserEntity>.Success(existing);
            }

            var user = new UserEntity
            {
                Email = email,
                ExternalAuthId = externalId,
                DisplayName = displayName,
                LastLoginAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Created new user from external auth: {UserId}", user.Id);
            return Result<UserEntity>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateByExternalAuth");
            return Result<UserEntity>.Failure($"Database error: {ex.Message}", ex);
        }
    }
}
