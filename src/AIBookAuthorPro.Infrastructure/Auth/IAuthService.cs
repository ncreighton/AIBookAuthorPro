// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Infrastructure.Data.Entities;

namespace AIBookAuthorPro.Infrastructure.Auth;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    Task<Result<UserEntity>> GetCurrentUserAsync(CancellationToken ct = default);

    /// <summary>
    /// Signs in a user with external auth provider (Stack Auth).
    /// </summary>
    Task<Result<UserEntity>> SignInWithExternalAuthAsync(string accessToken, CancellationToken ct = default);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task<Result> SignOutAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current user ID if authenticated.
    /// </summary>
    Guid? CurrentUserId { get; }

    /// <summary>
    /// Event raised when authentication state changes.
    /// </summary>
    event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;
}

/// <summary>
/// Auth state changed event args.
/// </summary>
public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; init; }
    public UserEntity? User { get; init; }
}
