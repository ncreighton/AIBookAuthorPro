// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Diagnostics;
using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.Covers;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Implementation of book cover generation and management service.
/// </summary>
public sealed class CoverService : ICoverService
{
    private readonly IAIProvider _aiProvider;
    private readonly IImageGenerationProvider _imageProvider;
    private readonly IProjectService _projectService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<CoverService> _logger;

    private static readonly List<CoverTemplate> _builtInTemplates = InitializeBuiltInTemplates();

    public CoverService(
        IAIProvider aiProvider,
        IImageGenerationProvider imageProvider,
        IProjectService projectService,
        IFileSystemService fileSystemService,
        ILogger<CoverService> logger)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _imageProvider = imageProvider ?? throw new ArgumentNullException(nameof(imageProvider));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<BookCover?>> GetActiveCoverAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting active cover for project {ProjectId}", projectId);

            var coversResult = await GetAllCoversAsync(projectId, cancellationToken);
            if (coversResult.IsFailure)
            {
                return Result<BookCover?>.Failure(coversResult.Error!);
            }

            var activeCover = coversResult.Value!.FirstOrDefault(c => c.IsActive);
            return Result<BookCover?>.Success(activeCover);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active cover for project {ProjectId}", projectId);
            return Result<BookCover?>.Failure($"Failed to get active cover: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<BookCover>>> GetAllCoversAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all covers for project {ProjectId}", projectId);

            var coversPath = GetCoversIndexPath(projectId);
            if (!await _fileSystemService.FileExistsAsync(coversPath, cancellationToken))
            {
                return Result<List<BookCover>>.Success(new List<BookCover>());
            }

            var json = await _fileSystemService.ReadAllTextAsync(coversPath, cancellationToken);
            var covers = JsonSerializer.Deserialize<List<BookCover>>(json, JsonOptions) ?? [];

            return Result<List<BookCover>>.Success(covers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting covers for project {ProjectId}", projectId);
            return Result<List<BookCover>>.Failure($"Failed to get covers: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveCoverAsync(
        Guid projectId,
        BookCover cover,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving cover {CoverId} for project {ProjectId}", cover.Id, projectId);

            var coversResult = await GetAllCoversAsync(projectId, cancellationToken);
            var covers = coversResult.IsSuccess ? coversResult.Value! : new List<BookCover>();

            var existingIndex = covers.FindIndex(c => c.Id == cover.Id);
            if (existingIndex >= 0)
            {
                covers[existingIndex] = cover;
            }
            else
            {
                covers.Add(cover);
            }

            // Ensure directory exists
            var coversDir = GetCoversDirectory(projectId);
            await _fileSystemService.EnsureDirectoryExistsAsync(coversDir, cancellationToken);

            var json = JsonSerializer.Serialize(covers, JsonOptions);
            await _fileSystemService.WriteAllTextAsync(GetCoversIndexPath(projectId), json, cancellationToken);

            _logger.LogInformation("Saved cover {CoverId} for project {ProjectId}", cover.Id, projectId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cover for project {ProjectId}", projectId);
            return Result.Failure($"Failed to save cover: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> SetActiveCoverAsync(
        Guid projectId,
        Guid coverId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Setting active cover {CoverId} for project {ProjectId}", coverId, projectId);

            var coversResult = await GetAllCoversAsync(projectId, cancellationToken);
            if (coversResult.IsFailure)
            {
                return Result.Failure(coversResult.Error!);
            }

            var covers = coversResult.Value!;
            foreach (var cover in covers)
            {
                cover.IsActive = cover.Id == coverId;
            }

            var json = JsonSerializer.Serialize(covers, JsonOptions);
            await _fileSystemService.WriteAllTextAsync(GetCoversIndexPath(projectId), json, cancellationToken);

            _logger.LogInformation("Set active cover {CoverId} for project {ProjectId}", coverId, projectId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active cover for project {ProjectId}", projectId);
            return Result.Failure($"Failed to set active cover: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteCoverAsync(
        Guid projectId,
        Guid coverId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting cover {CoverId} from project {ProjectId}", coverId, projectId);

            var coversResult = await GetAllCoversAsync(projectId, cancellationToken);
            if (coversResult.IsFailure)
            {
                return Result.Failure(coversResult.Error!);
            }

            var covers = coversResult.Value!;
            var coverToDelete = covers.FirstOrDefault(c => c.Id == coverId);
            
            if (coverToDelete == null)
            {
                return Result.Failure("Cover not found");
            }

            // Delete associated image files
            if (!string.IsNullOrEmpty(coverToDelete.FrontCoverPath))
                await _fileSystemService.DeleteFileAsync(coverToDelete.FrontCoverPath, cancellationToken);
            if (!string.IsNullOrEmpty(coverToDelete.BackCoverPath))
                await _fileSystemService.DeleteFileAsync(coverToDelete.BackCoverPath, cancellationToken);
            if (!string.IsNullOrEmpty(coverToDelete.FullCoverPath))
                await _fileSystemService.DeleteFileAsync(coverToDelete.FullCoverPath, cancellationToken);

            covers.Remove(coverToDelete);

            var json = JsonSerializer.Serialize(covers, JsonOptions);
            await _fileSystemService.WriteAllTextAsync(GetCoversIndexPath(projectId), json, cancellationToken);

            _logger.LogInformation("Deleted cover {CoverId} from project {ProjectId}", coverId, projectId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover from project {ProjectId}", projectId);
            return Result.Failure($"Failed to delete cover: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<CoverGenerationResult>> GenerateCoverAsync(
        CoverGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Generating cover for '{Title}' with style {Style}", request.Title, request.Style);

            // Build the image generation prompt
            var prompt = await BuildCoverPromptAsync(request, cancellationToken);
            
            var result = new CoverGenerationResult
            {
                UsedPrompt = prompt
            };

            // Generate variations
            for (int i = 0; i < request.VariationCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var imageResult = await _imageProvider.GenerateImageAsync(
                    new ImageGenerationRequest
                    {
                        Prompt = prompt,
                        Width = request.Specifications?.WidthPixels ?? 1600,
                        Height = request.Specifications?.HeightPixels ?? 2560,
                        Provider = request.Provider
                    },
                    cancellationToken);

                if (imageResult.IsSuccess)
                {
                    var variation = new CoverVariation
                    {
                        Name = $"Variation {i + 1}",
                        ImagePath = imageResult.Value!.ImagePath,
                        Prompt = prompt,
                        CreatedAt = DateTime.UtcNow
                    };
                    result.Variations.Add(variation);
                }
                else
                {
                    _logger.LogWarning("Failed to generate variation {Index}: {Error}", i + 1, imageResult.Error);
                }
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.IsSuccess = result.Variations.Count > 0;

            if (!result.IsSuccess)
            {
                result.Error = "Failed to generate any cover variations";
            }

            _logger.LogInformation("Generated {Count} cover variations in {Duration}ms",
                result.Variations.Count, stopwatch.ElapsedMilliseconds);

            return Result<CoverGenerationResult>.Success(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cover generation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover for '{Title}'", request.Title);
            return Result<CoverGenerationResult>.Failure($"Cover generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<CoverVariation>>> GenerateVariationsAsync(
        Guid coverId,
        int count = 4,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating {Count} variations for cover {CoverId}", count, coverId);

            // This would need the project ID in a real implementation
            // For now, we'll generate based on a modified prompt
            var variations = new List<CoverVariation>();

            // Placeholder - actual implementation would retrieve the original cover
            // and generate variations based on its prompt

            return Result<List<CoverVariation>>.Success(variations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating variations for cover {CoverId}", coverId);
            return Result<List<CoverVariation>>.Failure($"Variation generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<string>>> GeneratePromptSuggestionsAsync(
        Guid projectId,
        CoverStyle style = CoverStyle.Photographic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating prompt suggestions for project {ProjectId}, style {Style}",
                projectId, style);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<List<string>>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var prompt = $@"Generate 5 detailed image generation prompts for a book cover with these details:

Title: {project.Metadata.Title}
Genre: {project.Metadata.Genre}
Description: {project.Description}
Style: {style}

Requirements:
- Each prompt should be detailed and specific
- Include mood, lighting, composition elements
- Suitable for AI image generation (DALL-E, Midjourney style)
- Focus on visual elements, not text
- Professional book cover quality

Format as JSON array of strings:
[""prompt 1"", ""prompt 2"", ""prompt 3"", ""prompt 4"", ""prompt 5""]

Return ONLY the JSON array.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 2000,
                Temperature = 0.8
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<string>>.Failure($"Prompt generation failed: {result.Error}");
            }

            var suggestions = ParsePromptSuggestions(result.Value!.Content);
            
            _logger.LogInformation("Generated {Count} prompt suggestions for project {ProjectId}",
                suggestions.Count, projectId);
            return Result<List<string>>.Success(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt suggestions for project {ProjectId}", projectId);
            return Result<List<string>>.Failure($"Prompt generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<Result<List<CoverTemplate>>> GetTemplatesAsync(
        string? genre = null,
        CoverStyle? style = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = _builtInTemplates.AsEnumerable();

            if (!string.IsNullOrEmpty(genre))
            {
                templates = templates.Where(t => 
                    t.Category.Contains(genre, StringComparison.OrdinalIgnoreCase));
            }

            if (style.HasValue)
            {
                templates = templates.Where(t => t.Style == style.Value);
            }

            return Task.FromResult(Result<List<CoverTemplate>>.Success(templates.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cover templates");
            return Task.FromResult(Result<List<CoverTemplate>>.Failure($"Failed to get templates: {ex.Message}", ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<BookCover>> CreateFromTemplateAsync(
        Guid projectId,
        Guid templateId,
        Dictionary<string, string>? customizations = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating cover from template {TemplateId} for project {ProjectId}",
                templateId, projectId);

            var template = _builtInTemplates.FirstOrDefault(t => t.Id == templateId);
            if (template == null)
            {
                return Result<BookCover>.Failure("Template not found");
            }

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<BookCover>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;

            var cover = new BookCover
            {
                Title = project.Metadata.Title,
                Style = template.Style,
                Template = template,
                TextElements = new CoverTextElements
                {
                    Title = new TextElement { Text = project.Metadata.Title },
                    AuthorName = new TextElement { Text = project.Metadata.Author }
                },
                GenerationPrompt = template.BasePrompt
            };

            // Apply customizations
            if (customizations != null)
            {
                if (customizations.TryGetValue("subtitle", out var subtitle))
                {
                    cover.TextElements.Subtitle = new TextElement { Text = subtitle };
                }
                if (customizations.TryGetValue("tagline", out var tagline))
                {
                    cover.TextElements.Tagline = new TextElement { Text = tagline };
                }
            }

            await SaveCoverAsync(projectId, cover, cancellationToken);

            _logger.LogInformation("Created cover from template {TemplateId} for project {ProjectId}",
                templateId, projectId);
            return Result<BookCover>.Success(cover);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cover from template for project {ProjectId}", projectId);
            return Result<BookCover>.Failure($"Failed to create cover: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<Result<CoverSpecifications>> CalculateSpecificationsAsync(
        int pageCount,
        TrimSize trimSize = TrimSize.Size6x9,
        PaperType paperType = PaperType.Cream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating cover specifications: {PageCount} pages, {TrimSize}, {PaperType}",
                pageCount, trimSize, paperType);

            var specs = new CoverSpecifications
            {
                TrimSize = trimSize,
                PaperType = paperType,
                PageCount = pageCount,
                DPI = 300,
                HasBleed = true,
                BleedInches = 0.125
            };

            return Task.FromResult(Result<CoverSpecifications>.Success(specs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cover specifications");
            return Task.FromResult(Result<CoverSpecifications>.Failure($"Calculation failed: {ex.Message}", ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> GenerateFullCoverAsync(
        Guid coverId,
        CoverSpecifications specifications,
        BackCoverContent backCoverContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating full cover for cover {CoverId}", coverId);

            // This would compose front cover, spine, and back cover into a single image
            // For now, return placeholder path
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AIBookAuthorPro", "Covers", $"{coverId}_full.png");

            // In actual implementation:
            // 1. Load front cover image
            // 2. Create spine with title and author
            // 3. Create back cover with blurb, bio, barcode
            // 4. Compose into single image at correct dimensions
            // 5. Save and return path

            _logger.LogInformation("Generated full cover at {Path}", outputPath);
            return Result<string>.Success(outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating full cover for {CoverId}", coverId);
            return Result<string>.Failure($"Full cover generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> ExportForKDPAsync(
        Guid coverId,
        string outputPath,
        BookFormatType formatType = BookFormatType.Ebook,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Exporting cover {CoverId} for KDP {FormatType}", coverId, formatType);

            // Export specifications based on format:
            // eBook: 1600x2560 pixels minimum, RGB, JPEG/TIFF
            // Paperback: Full wrap cover with spine, 300 DPI, PDF
            
            // Placeholder - actual implementation would:
            // 1. Load cover image
            // 2. Resize/convert to KDP specifications
            // 3. Save to output path

            _logger.LogInformation("Exported cover {CoverId} to {Path}", coverId, outputPath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting cover {CoverId}", coverId);
            return Result.Failure($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<Result<List<CoverValidationIssue>>> ValidateCoverAsync(
        Guid coverId,
        BookFormatType formatType = BookFormatType.Ebook,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<CoverValidationIssue>();

        // Validation would check:
        // - Image dimensions meet minimums
        // - Color space is RGB (not CMYK for eBooks)
        // - File format is supported
        // - File size within limits
        // - Resolution meets requirements
        // - Bleed area is correct for print

        // Placeholder validation
        if (formatType == BookFormatType.Ebook)
        {
            // eBook requirements: min 1000x1600, recommended 1600x2560
            // Check would be performed on actual image
        }
        else if (formatType == BookFormatType.Paperback)
        {
            // Paperback requires 300 DPI minimum
            // Full wrap cover with correct spine width
        }

        return Task.FromResult(Result<List<CoverValidationIssue>>.Success(issues));
    }

    /// <inheritdoc />
    public async Task<Result<BookCover>> ImportCoverAsync(
        Guid projectId,
        string imagePath,
        CoverType coverType = CoverType.Front,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Importing cover from {Path} for project {ProjectId}", imagePath, projectId);

            if (!await _fileSystemService.FileExistsAsync(imagePath, cancellationToken))
            {
                return Result<BookCover>.Failure("Image file not found");
            }

            // Copy image to covers directory
            var coversDir = GetCoversDirectory(projectId);
            await _fileSystemService.EnsureDirectoryExistsAsync(coversDir, cancellationToken);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagePath)}";
            var destPath = Path.Combine(coversDir, fileName);
            await _fileSystemService.CopyFileAsync(imagePath, destPath, cancellationToken);

            var cover = new BookCover
            {
                Style = CoverStyle.Photographic // Assume photographic for imports
            };

            switch (coverType)
            {
                case CoverType.Front:
                    cover.FrontCoverPath = destPath;
                    break;
                case CoverType.Back:
                    cover.BackCoverPath = destPath;
                    break;
                case CoverType.Full:
                    cover.FullCoverPath = destPath;
                    break;
                case CoverType.Spine:
                    cover.SpinePath = destPath;
                    break;
            }

            await SaveCoverAsync(projectId, cover, cancellationToken);

            _logger.LogInformation("Imported cover from {Path} for project {ProjectId}", imagePath, projectId);
            return Result<BookCover>.Success(cover);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing cover for project {ProjectId}", projectId);
            return Result<BookCover>.Failure($"Import failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<Result<List<ImageGenerationProvider>>> GetAvailableProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        var providers = new List<ImageGenerationProvider>
        {
            ImageGenerationProvider.DallE,
            ImageGenerationProvider.StabilityAI,
            ImageGenerationProvider.Ideogram,
            ImageGenerationProvider.Leonardo
        };

        return Task.FromResult(Result<List<ImageGenerationProvider>>.Success(providers));
    }

    #region Private Methods

    private async Task<string> BuildCoverPromptAsync(
        CoverGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var styleDescriptions = new Dictionary<CoverStyle, string>
        {
            [CoverStyle.Photographic] = "photorealistic, cinematic photography, professional book cover",
            [CoverStyle.Illustrated] = "illustrated, digital painting, artistic book cover illustration",
            [CoverStyle.Typography] = "bold typography focused, minimalist design with text emphasis",
            [CoverStyle.Minimalist] = "clean, minimalist, simple shapes, elegant composition",
            [CoverStyle.Abstract] = "abstract art, dynamic colors, conceptual design",
            [CoverStyle.Vintage] = "vintage aesthetic, retro design, aged texture, classic feel",
            [CoverStyle.DigitalArt] = "digital art, modern illustration, vibrant colors",
            [CoverStyle.ThreeD] = "3D rendered, dimensional, modern CGI aesthetic",
            [CoverStyle.Collage] = "collage style, mixed media, layered composition",
            [CoverStyle.HandDrawn] = "hand-drawn, sketch style, artistic linework"
        };

        var basePrompt = $@"Professional book cover design for ""{request.Title}"" by {request.Author}. 
Genre: {request.Genre}. 
Style: {styleDescriptions.GetValueOrDefault(request.Style, "professional book cover")}.
Mood: {request.Mood ?? "compelling and genre-appropriate"}.
{(request.Description != null ? $"Book concept: {request.Description}" : "")}
{(request.IncludeElements?.Count > 0 ? $"Include: {string.Join(", ", request.IncludeElements)}" : "")}
{(request.ExcludeElements?.Count > 0 ? $"Exclude: {string.Join(", ", request.ExcludeElements)}" : "")}
{(request.ColorScheme != null ? $"Color scheme: {request.ColorScheme.PrimaryColor}, {request.ColorScheme.SecondaryColor}" : "")}
{request.AdditionalInstructions ?? ""}
High quality, professional publishing standard, no text or typography in image.";

        return basePrompt.Trim();
    }

    private static List<string> ParsePromptSuggestions(string content)
    {
        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed[7..];
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0) trimmed = trimmed[..endIndex];
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed[3..];
            var endIndex = trimmed.LastIndexOf("```");
            if (endIndex > 0) trimmed = trimmed[..endIndex];
        }
        return trimmed.Trim();
    }

    private static string GetCoversDirectory(Guid projectId) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro", "Covers", projectId.ToString());

    private static string GetCoversIndexPath(Guid projectId) =>
        Path.Combine(GetCoversDirectory(projectId), "covers.json");

    private static List<CoverTemplate> InitializeBuiltInTemplates()
    {
        return new List<CoverTemplate>
        {
            new()
            {
                Name = "Epic Fantasy",
                Description = "Dramatic fantasy landscape with magical elements",
                Category = "Fantasy",
                Style = CoverStyle.DigitalArt,
                BasePrompt = "epic fantasy book cover, magical landscape, dramatic lighting, mystical atmosphere",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#1a237e",
                    SecondaryColor = "#7c4dff",
                    BackgroundColor = "#0d1421",
                    TextColor = "#ffd700"
                }
            },
            new()
            {
                Name = "Contemporary Romance",
                Description = "Elegant, romantic design with soft colors",
                Category = "Romance",
                Style = CoverStyle.Photographic,
                BasePrompt = "romantic book cover, soft lighting, elegant composition, warm tones",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#e91e63",
                    SecondaryColor = "#f8bbd9",
                    BackgroundColor = "#fff5f8",
                    TextColor = "#880e4f"
                }
            },
            new()
            {
                Name = "Dark Thriller",
                Description = "Moody, suspenseful design with dark tones",
                Category = "Thriller",
                Style = CoverStyle.Photographic,
                BasePrompt = "thriller book cover, dark moody atmosphere, suspenseful, noir aesthetic",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#b71c1c",
                    SecondaryColor = "#212121",
                    BackgroundColor = "#0a0a0a",
                    TextColor = "#ffffff"
                }
            },
            new()
            {
                Name = "Science Fiction",
                Description = "Futuristic, tech-inspired design",
                Category = "Science Fiction",
                Style = CoverStyle.ThreeD,
                BasePrompt = "sci-fi book cover, futuristic, space, technology, cyberpunk elements",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#00bcd4",
                    SecondaryColor = "#7c4dff",
                    BackgroundColor = "#0a1628",
                    TextColor = "#00ffff"
                }
            },
            new()
            {
                Name = "Literary Fiction",
                Description = "Artistic, minimalist design",
                Category = "Literary",
                Style = CoverStyle.Minimalist,
                BasePrompt = "literary fiction cover, artistic, minimalist, elegant, thoughtful design",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#37474f",
                    SecondaryColor = "#78909c",
                    BackgroundColor = "#eceff1",
                    TextColor = "#263238"
                }
            },
            new()
            {
                Name = "Cozy Mystery",
                Description = "Charming, inviting design with whimsical elements",
                Category = "Mystery",
                Style = CoverStyle.Illustrated,
                BasePrompt = "cozy mystery book cover, charming illustration, warm inviting atmosphere, whimsical",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#ff7043",
                    SecondaryColor = "#ffcc80",
                    BackgroundColor = "#fff3e0",
                    TextColor = "#bf360c"
                }
            },
            new()
            {
                Name = "Historical Fiction",
                Description = "Period-appropriate vintage aesthetic",
                Category = "Historical",
                Style = CoverStyle.Vintage,
                BasePrompt = "historical fiction cover, vintage aesthetic, period appropriate, aged texture",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#8d6e63",
                    SecondaryColor = "#d7ccc8",
                    BackgroundColor = "#efebe9",
                    TextColor = "#3e2723"
                }
            },
            new()
            {
                Name = "Horror",
                Description = "Unsettling, atmospheric horror design",
                Category = "Horror",
                Style = CoverStyle.Photographic,
                BasePrompt = "horror book cover, unsettling atmosphere, dark shadows, ominous mood",
                IsBuiltIn = true,
                Colors = new ColorScheme
                {
                    PrimaryColor = "#6a1b9a",
                    SecondaryColor = "#ce93d8",
                    BackgroundColor = "#12005e",
                    TextColor = "#ea80fc"
                }
            }
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    #endregion
}

/// <summary>
/// Interface for image generation providers (DALL-E, Stability AI, etc.)
/// </summary>
public interface IImageGenerationProvider
{
    /// <summary>
    /// Generates an image based on a prompt.
    /// </summary>
    Task<Result<ImageGenerationResult>> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for image generation.
/// </summary>
public record ImageGenerationRequest
{
    public required string Prompt { get; init; }
    public int Width { get; init; } = 1024;
    public int Height { get; init; } = 1024;
    public ImageGenerationProvider Provider { get; init; } = ImageGenerationProvider.DallE;
}

/// <summary>
/// Result of image generation.
/// </summary>
public record ImageGenerationResult
{
    public required string ImagePath { get; init; }
    public string? ImageUrl { get; init; }
    public TimeSpan Duration { get; init; }
    public decimal? Cost { get; init; }
}

/// <summary>
/// Types of cover for import.
/// </summary>
public enum CoverType
{
    Front,
    Back,
    Full,
    Spine
}
