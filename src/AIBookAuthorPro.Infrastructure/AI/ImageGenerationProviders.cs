// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Net.Http.Json;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.AI;

/// <summary>
/// DALL-E image generation provider implementation.
/// </summary>
public sealed class DallEImageProvider : IImageGenerationProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<DallEImageProvider> _logger;

    private const string ApiUrl = "https://api.openai.com/v1/images/generations";

    public DallEImageProvider(
        HttpClient httpClient,
        ISettingsService settingsService,
        IFileSystemService fileSystemService,
        ILogger<DallEImageProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ImageGenerationResult>> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Generating image with DALL-E: {Prompt}", 
                request.Prompt.Length > 100 ? request.Prompt[..100] + "..." : request.Prompt);

            var settings = await _settingsService.GetSettingsAsync(cancellationToken);
            var apiKey = settings.OpenAIApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                return Result<ImageGenerationResult>.Failure("OpenAI API key not configured");
            }

            // Determine size string based on dimensions
            var size = GetSizeString(request.Width, request.Height);

            var requestBody = new
            {
                model = "dall-e-3",
                prompt = request.Prompt,
                n = 1,
                size = size,
                quality = "hd",
                response_format = "url"
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DALL-E API error: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);
                return Result<ImageGenerationResult>.Failure(
                    $"DALL-E API error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
            {
                return Result<ImageGenerationResult>.Failure("No image data in response");
            }

            var imageData = data[0];
            var imageUrl = imageData.GetProperty("url").GetString();

            if (string.IsNullOrEmpty(imageUrl))
            {
                return Result<ImageGenerationResult>.Failure("No image URL in response");
            }

            // Download and save the image
            var imagePath = await DownloadAndSaveImageAsync(imageUrl, cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            // Estimate cost (DALL-E 3 HD pricing)
            var cost = size switch
            {
                "1024x1024" => 0.080m,
                "1024x1792" or "1792x1024" => 0.120m,
                _ => 0.040m
            };

            _logger.LogInformation("Generated image in {Duration}ms, saved to {Path}",
                duration.TotalMilliseconds, imagePath);

            return Result<ImageGenerationResult>.Success(new ImageGenerationResult
            {
                ImagePath = imagePath,
                ImageUrl = imageUrl,
                Duration = duration,
                Cost = cost
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Image generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image with DALL-E");
            return Result<ImageGenerationResult>.Failure($"Image generation failed: {ex.Message}", ex);
        }
    }

    private static string GetSizeString(int width, int height)
    {
        // DALL-E 3 supports: 1024x1024, 1024x1792, 1792x1024
        var aspectRatio = (double)width / height;

        if (aspectRatio > 1.5)
        {
            return "1792x1024"; // Landscape
        }
        else if (aspectRatio < 0.67)
        {
            return "1024x1792"; // Portrait (book cover)
        }
        else
        {
            return "1024x1024"; // Square
        }
    }

    private async Task<string> DownloadAndSaveImageAsync(
        string imageUrl, 
        CancellationToken cancellationToken)
    {
        var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);

        var coversDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro", "Covers", "Generated");

        await _fileSystemService.EnsureDirectoryExistsAsync(coversDir, cancellationToken);

        var fileName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(coversDir, fileName);

        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

        return filePath;
    }
}

/// <summary>
/// Stability AI image generation provider implementation.
/// </summary>
public sealed class StabilityAIImageProvider : IImageGenerationProvider
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<StabilityAIImageProvider> _logger;

    private const string ApiUrl = "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image";

    public StabilityAIImageProvider(
        HttpClient httpClient,
        ISettingsService settingsService,
        IFileSystemService fileSystemService,
        ILogger<StabilityAIImageProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<ImageGenerationResult>> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Generating image with Stability AI: {Prompt}", 
                request.Prompt.Length > 100 ? request.Prompt[..100] + "..." : request.Prompt);

            var settings = await _settingsService.GetSettingsAsync(cancellationToken);
            var apiKey = settings.StabilityAIApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                return Result<ImageGenerationResult>.Failure("Stability AI API key not configured");
            }

            var requestBody = new
            {
                text_prompts = new[]
                {
                    new { text = request.Prompt, weight = 1.0 }
                },
                cfg_scale = 7,
                height = GetValidDimension(request.Height),
                width = GetValidDimension(request.Width),
                samples = 1,
                steps = 30
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stability AI API error: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);
                return Result<ImageGenerationResult>.Failure(
                    $"Stability AI API error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("artifacts", out var artifacts) || artifacts.GetArrayLength() == 0)
            {
                return Result<ImageGenerationResult>.Failure("No image data in response");
            }

            var artifact = artifacts[0];
            var base64Image = artifact.GetProperty("base64").GetString();

            if (string.IsNullOrEmpty(base64Image))
            {
                return Result<ImageGenerationResult>.Failure("No image data in response");
            }

            // Save the image
            var imageBytes = Convert.FromBase64String(base64Image);
            var imagePath = await SaveImageAsync(imageBytes, cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Generated image in {Duration}ms, saved to {Path}",
                duration.TotalMilliseconds, imagePath);

            return Result<ImageGenerationResult>.Success(new ImageGenerationResult
            {
                ImagePath = imagePath,
                Duration = duration,
                Cost = 0.04m // Approximate cost
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Image generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image with Stability AI");
            return Result<ImageGenerationResult>.Failure($"Image generation failed: {ex.Message}", ex);
        }
    }

    private static int GetValidDimension(int dimension)
    {
        // SDXL requires dimensions to be multiples of 64
        // and between 512 and 2048
        var valid = Math.Max(512, Math.Min(2048, dimension));
        return (valid / 64) * 64;
    }

    private async Task<string> SaveImageAsync(byte[] imageBytes, CancellationToken cancellationToken)
    {
        var coversDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro", "Covers", "Generated");

        await _fileSystemService.EnsureDirectoryExistsAsync(coversDir, cancellationToken);

        var fileName = $"{Guid.NewGuid()}.png";
        var filePath = Path.Combine(coversDir, fileName);

        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

        return filePath;
    }
}

/// <summary>
/// Factory for creating image generation providers.
/// </summary>
public sealed class ImageGenerationProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ImageGenerationProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets an image generation provider by type.
    /// </summary>
    public IImageGenerationProvider GetProvider(ImageGenerationProvider providerType)
    {
        return providerType switch
        {
            ImageGenerationProvider.DallE => 
                _serviceProvider.GetRequiredService<DallEImageProvider>(),
            ImageGenerationProvider.StabilityAI => 
                _serviceProvider.GetRequiredService<StabilityAIImageProvider>(),
            _ => _serviceProvider.GetRequiredService<DallEImageProvider>()
        };
    }
}

// Add this helper extension
file static class ServiceProviderExtensions
{
    public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
    {
        return (T)(provider.GetService(typeof(T)) 
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered"));
    }
}
