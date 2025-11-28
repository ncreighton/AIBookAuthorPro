// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace AIBookAuthorPro.Infrastructure.Auth;

/// <summary>
/// Extension methods for registering auth services.
/// </summary>
public static class AuthServiceExtensions
{
    /// <summary>
    /// Adds authentication services to the service collection.
    /// </summary>
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        StackAuthConfiguration config)
    {
        services.AddSingleton(config);
        services.AddHttpClient<IAuthService, StackAuthService>();
        
        return services;
    }
}
