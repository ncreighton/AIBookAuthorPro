// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Net.Http.Json;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Infrastructure.Data;
using AIBookAuthorPro.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Auth;

/// <summary>
/// Stack Auth implementation of authentication service.
/// </summary>
public class StackAuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IUserRepository _userRepository;
    private readonly StackAuthConfiguration _config;
    private readonly ILogger<StackAuthService> _logger;
    
    private UserEntity? _currentUser;
    private string? _accessToken;

    public event EventHandler<AuthStateChangedEventArgs>? AuthStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public Guid? CurrentUserId => _currentUser?.Id;

    public StackAuthService(
        HttpClient httpClient,
        IUserRepository userRepository,
        StackAuthConfiguration config,
        ILogger<StackAuthService> logger)
    {
        _httpClient = httpClient;
        _userRepository = userRepository;
        _config = config;
        _logger = logger;
    }

    public Task<Result<UserEntity>> GetCurrentUserAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_currentUser != null
            ? Result<UserEntity>.Success(_currentUser)
            : Result<UserEntity>.Failure("No user is currently authenticated"));
    }

    public async Task<Result<UserEntity>> SignInWithExternalAuthAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Attempting Stack Auth sign in");

            // Verify token with Stack Auth
            var userInfo = await VerifyTokenAsync(accessToken, ct);
            if (userInfo == null)
            {
                return Result<UserEntity>.Failure("Invalid access token");
            }

            // Get or create local user
            var userResult = await _userRepository.GetOrCreateByExternalAuthAsync(
                userInfo.UserId,
                userInfo.Email,
                userInfo.DisplayName,
                ct);

            if (userResult.IsFailure)
            {
                return userResult;
            }

            _currentUser = userResult.Value;
            _accessToken = accessToken;

            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
            {
                IsAuthenticated = true,
                User = _currentUser
            });

            _logger.LogInformation("User {UserId} signed in successfully", _currentUser!.Id);
            return Result<UserEntity>.Success(_currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in");
            return Result<UserEntity>.Failure($"Sign in failed: {ex.Message}", ex);
        }
    }

    public Task<Result> SignOutAsync(CancellationToken ct = default)
    {
        _currentUser = null;
        _accessToken = null;

        AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
        {
            IsAuthenticated = false,
            User = null
        });

        _logger.LogInformation("User signed out");
        return Task.FromResult(Result.Success());
    }

    private async Task<StackAuthUserInfo?> VerifyTokenAsync(string accessToken, CancellationToken ct)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_config.BaseUrl}/api/v1/users/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("x-stack-project-id", _config.ProjectId);
            request.Headers.Add("x-stack-publishable-client-key", _config.PublishableClientKey);

            var response = await _httpClient.SendAsync(request, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stack Auth token verification failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var userData = JsonSerializer.Deserialize<JsonElement>(json);

            return new StackAuthUserInfo
            {
                UserId = userData.GetProperty("id").GetString() ?? "",
                Email = userData.GetProperty("primary_email").GetString() ?? "",
                DisplayName = userData.TryGetProperty("display_name", out var name) ? name.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Stack Auth token");
            return null;
        }
    }

    private class StackAuthUserInfo
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string? DisplayName { get; set; }
    }
}

/// <summary>
/// Stack Auth configuration.
/// </summary>
public class StackAuthConfiguration
{
    public string ProjectId { get; set; } = "";
    public string PublishableClientKey { get; set; } = "";
    public string SecretServerKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.stack-auth.com";
}
