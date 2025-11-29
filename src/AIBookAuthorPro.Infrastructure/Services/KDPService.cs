// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

using System.Text.Json;
using AIBookAuthorPro.Core.Common;
using AIBookAuthorPro.Core.Enums;
using AIBookAuthorPro.Core.Interfaces;
using AIBookAuthorPro.Core.Models.AI;
using AIBookAuthorPro.Core.Models.KDP;
using Microsoft.Extensions.Logging;

namespace AIBookAuthorPro.Infrastructure.Services;

/// <summary>
/// Implementation of KDP publishing service.
/// </summary>
public sealed class KDPService : IKDPService
{
    private readonly IAIProvider _aiProvider;
    private readonly IProjectService _projectService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<KDPService> _logger;

    // KDP category database (simplified - in production, load from JSON file)
    private static readonly List<KDPCategory> _categoryDatabase = InitializeCategoryDatabase();

    public KDPService(
        IAIProvider aiProvider,
        IProjectService projectService,
        IFileSystemService fileSystemService,
        ILogger<KDPService> logger)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<KDPMetadata>> GetMetadataAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting KDP metadata for project {ProjectId}", projectId);

            var metadataPath = GetMetadataPath(projectId);
            if (!await _fileSystemService.FileExistsAsync(metadataPath, cancellationToken))
            {
                // Return new metadata if none exists
                return Result<KDPMetadata>.Success(new KDPMetadata());
            }

            var json = await _fileSystemService.ReadAllTextAsync(metadataPath, cancellationToken);
            var metadata = JsonSerializer.Deserialize<KDPMetadata>(json, JsonOptions);

            return Result<KDPMetadata>.Success(metadata ?? new KDPMetadata());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KDP metadata for project {ProjectId}", projectId);
            return Result<KDPMetadata>.Failure($"Failed to get metadata: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveMetadataAsync(
        Guid projectId,
        KDPMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving KDP metadata for project {ProjectId}", projectId);

            var metadataPath = GetMetadataPath(projectId);
            var json = JsonSerializer.Serialize(metadata, JsonOptions);
            await _fileSystemService.WriteAllTextAsync(metadataPath, json, cancellationToken);

            _logger.LogInformation("Saved KDP metadata for project {ProjectId}", projectId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving KDP metadata for project {ProjectId}", projectId);
            return Result.Failure($"Failed to save metadata: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<Result<List<KDPCategory>>> SearchCategoriesAsync(
        string query,
        BookCategory? bookCategory = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching KDP categories for: {Query}", query);

            var results = _categoryDatabase
                .Where(c => c.CategoryPath.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();

            return Task.FromResult(Result<List<KDPCategory>>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories for: {Query}", query);
            return Task.FromResult(Result<List<KDPCategory>>.Failure($"Search failed: {ex.Message}", ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<KDPCategory>>> SuggestCategoriesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Suggesting KDP categories for project {ProjectId}", projectId);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<List<KDPCategory>>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var prompt = $@"Based on this book information, suggest the 5 most appropriate Amazon KDP browse categories:

Title: {project.Metadata.Title}
Genre: {project.Metadata.Genre}
Description: {project.Description}
Tags: {string.Join(", ", project.Metadata.Tags)}
Target Audience: {project.Metadata.TargetAudience}

Return the categories in order of relevance as a JSON array:
[
  {{
    ""categoryPath"": ""Fiction > Fantasy > Epic"",
    ""name"": ""Epic Fantasy"",
    ""bisacCode"": ""FIC009020""
  }}
]

Use real Amazon KDP category paths.
Return ONLY the JSON array.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 1000,
                Temperature = 0.3
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<KDPCategory>>.Failure($"AI suggestion failed: {result.Error}");
            }

            var categories = ParseCategories(result.Value!.Content);
            
            _logger.LogInformation("Suggested {Count} categories for project {ProjectId}", 
                categories.Count, projectId);
            return Result<List<KDPCategory>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting categories for project {ProjectId}", projectId);
            return Result<List<KDPCategory>>.Failure($"Suggestion failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<KeywordSuggestion>>> GenerateKeywordsAsync(
        Guid projectId,
        int count = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating {Count} keywords for project {ProjectId}", count, projectId);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<List<KeywordSuggestion>>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var prompt = $@"Generate {count} highly effective Amazon KDP search keywords for this book:

Title: {project.Metadata.Title}
Genre: {project.Metadata.Genre}
Description: {project.Description}
Target Audience: {project.Metadata.TargetAudience}

Requirements for keywords:
- Mix of broad and long-tail keywords
- Keywords readers would actually search for
- Avoid author name, title, or category names
- Focus on themes, tropes, comparisons, reader emotions
- Include ""books like [similar popular book]"" style keywords

Format as JSON array:
[
  {{
    ""keyword"": ""the keyword phrase"",
    ""relevanceScore"": 0.95,
    ""competitionLevel"": ""Low/Medium/High"",
    ""relatedCategory"": ""Related category"",
    ""isLongTail"": true
  }}
]

Return ONLY the JSON array.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 1500,
                Temperature = 0.7
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<List<KeywordSuggestion>>.Failure($"Keyword generation failed: {result.Error}");
            }

            var keywords = ParseKeywords(result.Value!.Content);
            
            _logger.LogInformation("Generated {Count} keywords for project {ProjectId}", 
                keywords.Count, projectId);
            return Result<List<KeywordSuggestion>>.Success(keywords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating keywords for project {ProjectId}", projectId);
            return Result<List<KeywordSuggestion>>.Failure($"Keyword generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<BookDescriptionSuggestion>> GenerateDescriptionAsync(
        Guid projectId,
        string? style = null,
        int maxLength = 4000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating book description for project {ProjectId}, style: {Style}", 
                projectId, style);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<BookDescriptionSuggestion>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var styleInstruction = style ?? "compelling hook-based";

            var prompt = $@"Write a {styleInstruction} Amazon book description for this book:

Title: {project.Metadata.Title}
Genre: {project.Metadata.Genre}
Synopsis: {project.Description}
Target Audience: {project.Metadata.TargetAudience}

Requirements:
- Maximum {maxLength} characters
- Start with a powerful hook
- Use HTML formatting (bold, italic, line breaks)
- Include emotional appeal
- End with a call to action
- Make readers NEED to buy this book

Format the response as JSON:
{{
  ""description"": ""The full HTML-formatted description"",
  ""style"": ""{styleInstruction}"",
  ""hasHtmlFormatting"": true,
  ""hook"": ""The opening hook line"",
  ""callToAction"": ""The closing call to action""
}}

Return ONLY the JSON object.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 2000,
                Temperature = 0.8
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<BookDescriptionSuggestion>.Failure($"Description generation failed: {result.Error}");
            }

            var description = ParseDescription(result.Value!.Content);
            
            _logger.LogInformation("Generated book description for project {ProjectId}, {Length} characters", 
                projectId, description.CharacterCount);
            return Result<BookDescriptionSuggestion>.Success(description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating description for project {ProjectId}", projectId);
            return Result<BookDescriptionSuggestion>.Failure($"Description generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<BookDescriptionSuggestion>>> GenerateDescriptionVariationsAsync(
        Guid projectId,
        int count = 3,
        CancellationToken cancellationToken = default)
    {
        var styles = new[] { "hook-based thriller style", "emotional literary style", "mystery teaser style" };
        var descriptions = new List<BookDescriptionSuggestion>();

        for (int i = 0; i < Math.Min(count, styles.Length); i++)
        {
            var result = await GenerateDescriptionAsync(projectId, styles[i], 4000, cancellationToken);
            if (result.IsSuccess)
            {
                descriptions.Add(result.Value!);
            }
        }

        return Result<List<BookDescriptionSuggestion>>.Success(descriptions);
    }

    /// <inheritdoc />
    public Task<Result<PrintSpecifications>> CalculatePrintSpecsAsync(
        int pageCount,
        TrimSize trimSize = TrimSize.Size6x9,
        PaperType paperType = PaperType.Cream,
        bool hasBleed = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating print specs: {PageCount} pages, {TrimSize}, {PaperType}",
                pageCount, trimSize, paperType);

            var specs = new PrintSpecifications
            {
                FormatType = BookFormatType.Paperback,
                TrimSize = trimSize,
                PaperType = paperType,
                HasBleed = hasBleed,
                PageCount = pageCount
            };

            // Calculate printing cost (simplified KDP formula)
            // Actual formula: Fixed cost + (page count × per-page cost)
            var fixedCost = 0.85m; // Base cost
            var perPageCost = paperType == PaperType.Cream ? 0.012m : 0.010m;
            specs.PrintingCost = fixedCost + (pageCount * perPageCost);

            // Minimum list price = printing cost / 0.6 (for 60% royalty rate to break even)
            specs.MinimumListPrice = Math.Ceiling(specs.PrintingCost / 0.6m * 100) / 100;

            return Task.FromResult(Result<PrintSpecifications>.Success(specs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating print specs");
            return Task.FromResult(Result<PrintSpecifications>.Failure($"Calculation failed: {ex.Message}", ex));
        }
    }

    /// <inheritdoc />
    public Task<Result<PricingInfo>> CalculateRoyaltiesAsync(
        decimal listPrice,
        KDPMarketplace marketplace = KDPMarketplace.US,
        BookFormatType formatType = BookFormatType.Ebook,
        PrintSpecifications? printSpecs = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating royalties: ${ListPrice}, {Marketplace}, {FormatType}",
                listPrice, marketplace, formatType);

            var pricing = new PricingInfo
            {
                ListPrice = listPrice,
                Currency = GetMarketplaceCurrency(marketplace)
            };

            if (formatType == BookFormatType.Ebook)
            {
                // eBook royalty calculation
                if (listPrice >= 2.99m && listPrice <= 9.99m)
                {
                    pricing.RoyaltyPercentage = 70;
                    // Delivery cost ~$0.15 per MB (assume 1MB average)
                    pricing.DeliveryCost = 0.15m;
                    pricing.EstimatedRoyalty = (listPrice * 0.70m) - pricing.DeliveryCost;
                }
                else
                {
                    pricing.RoyaltyPercentage = 35;
                    pricing.DeliveryCost = 0;
                    pricing.EstimatedRoyalty = listPrice * 0.35m;
                }
            }
            else if (formatType == BookFormatType.Paperback && printSpecs != null)
            {
                // Paperback royalty: 60% of list price minus printing cost
                pricing.RoyaltyPercentage = 60;
                pricing.DeliveryCost = printSpecs.PrintingCost;
                pricing.EstimatedRoyalty = (listPrice * 0.60m) - printSpecs.PrintingCost;
            }

            return Task.FromResult(Result<PricingInfo>.Success(pricing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating royalties");
            return Task.FromResult(Result<PricingInfo>.Failure($"Calculation failed: {ex.Message}", ex));
        }
    }

    /// <inheritdoc />
    public async Task<Result<SeriesInfo>> GetOrCreateSeriesAsync(
        string seriesName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting or creating series: {SeriesName}", seriesName);

            var seriesPath = GetSeriesPath();
            var allSeries = new List<SeriesInfo>();

            if (await _fileSystemService.FileExistsAsync(seriesPath, cancellationToken))
            {
                var json = await _fileSystemService.ReadAllTextAsync(seriesPath, cancellationToken);
                allSeries = JsonSerializer.Deserialize<List<SeriesInfo>>(json, JsonOptions) ?? [];
            }

            var existing = allSeries.FirstOrDefault(s => 
                s.Name.Equals(seriesName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return Result<SeriesInfo>.Success(existing);
            }

            var newSeries = new SeriesInfo { Name = seriesName };
            allSeries.Add(newSeries);

            var serialized = JsonSerializer.Serialize(allSeries, JsonOptions);
            await _fileSystemService.WriteAllTextAsync(seriesPath, serialized, cancellationToken);

            return Result<SeriesInfo>.Success(newSeries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting/creating series: {SeriesName}", seriesName);
            return Result<SeriesInfo>.Failure($"Series operation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<List<SeriesInfo>>> GetAllSeriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var seriesPath = GetSeriesPath();
            if (!await _fileSystemService.FileExistsAsync(seriesPath, cancellationToken))
            {
                return Result<List<SeriesInfo>>.Success(new List<SeriesInfo>());
            }

            var json = await _fileSystemService.ReadAllTextAsync(seriesPath, cancellationToken);
            var series = JsonSerializer.Deserialize<List<SeriesInfo>>(json, JsonOptions) ?? [];

            return Result<List<SeriesInfo>>.Success(series);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all series");
            return Result<List<SeriesInfo>>.Failure($"Failed to get series: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<APlusContent>> GenerateAPlusContentAsync(
        Guid projectId,
        List<APlusModuleType>? moduleTypes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating A+ content for project {ProjectId}", projectId);

            var projectResult = await _projectService.GetProjectAsync(projectId, cancellationToken);
            if (projectResult.IsFailure)
            {
                return Result<APlusContent>.Failure($"Project not found: {projectResult.Error}");
            }

            var project = projectResult.Value!;
            var types = moduleTypes ?? new List<APlusModuleType> 
            { 
                APlusModuleType.StandardText, 
                APlusModuleType.StandardImageText,
                APlusModuleType.StandardFourImages 
            };

            var prompt = $@"Generate Amazon A+ Content (Enhanced Brand Content) for this book:

Title: {project.Metadata.Title}
Author: {project.Metadata.Author}
Genre: {project.Metadata.Genre}
Description: {project.Description}

Create content for these module types: {string.Join(", ", types)}

Format as JSON:
{{
  ""name"": ""A+ Content for {project.Metadata.Title}"",
  ""modules"": [
    {{
      ""type"": ""StandardText"",
      ""headline"": ""Compelling headline"",
      ""bodyText"": ""Engaging body text (max 400 chars)"",
      ""order"": 1
    }},
    {{
      ""type"": ""StandardImageText"",
      ""headline"": ""Image section headline"",
      ""bodyText"": ""Description text"",
      ""imageUrls"": [],
      ""imageAltTexts"": [""Alt text for image""],
      ""order"": 2
    }}
  ]
}}

Make the content compelling, on-brand, and designed to convert browsers to buyers.
Return ONLY the JSON object.";

            var request = new GenerationRequest
            {
                Prompt = prompt,
                MaxTokens = 3000,
                Temperature = 0.7
            };

            var result = await _aiProvider.GenerateAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Result<APlusContent>.Failure($"A+ content generation failed: {result.Error}");
            }

            var content = ParseAPlusContent(result.Value!.Content);
            
            _logger.LogInformation("Generated A+ content with {Count} modules for project {ProjectId}", 
                content.Modules.Count, projectId);
            return Result<APlusContent>.Success(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating A+ content for project {ProjectId}", projectId);
            return Result<APlusContent>.Failure($"A+ content generation failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public Task<Result<List<ValidationIssue>>> ValidateMetadataAsync(
        KDPMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ValidationIssue>();

        // Validate description
        if (string.IsNullOrWhiteSpace(metadata.Description))
        {
            issues.Add(new ValidationIssue
            {
                Field = "Description",
                Message = "Book description is required",
                Severity = "Error",
                SuggestedFix = "Add a compelling book description (up to 4000 characters)"
            });
        }
        else if (metadata.Description.Length > 4000)
        {
            issues.Add(new ValidationIssue
            {
                Field = "Description",
                Message = $"Description exceeds maximum length ({metadata.Description.Length}/4000)",
                Severity = "Error",
                SuggestedFix = "Shorten the description to 4000 characters or less"
            });
        }

        // Validate keywords
        if (metadata.Keywords.Count == 0)
        {
            issues.Add(new ValidationIssue
            {
                Field = "Keywords",
                Message = "No keywords specified",
                Severity = "Warning",
                SuggestedFix = "Add up to 7 search keywords for discoverability"
            });
        }
        else if (metadata.Keywords.Count > 7)
        {
            issues.Add(new ValidationIssue
            {
                Field = "Keywords",
                Message = $"Too many keywords ({metadata.Keywords.Count}/7)",
                Severity = "Error",
                SuggestedFix = "Remove keywords to have maximum 7"
            });
        }

        // Validate categories
        if (metadata.PrimaryCategory == null)
        {
            issues.Add(new ValidationIssue
            {
                Field = "PrimaryCategory",
                Message = "Primary category is required",
                Severity = "Error",
                SuggestedFix = "Select a primary browse category"
            });
        }

        // Validate ISBN if not using KDP free
        if (!metadata.UseKDPFreeISBN && string.IsNullOrWhiteSpace(metadata.ISBN13))
        {
            issues.Add(new ValidationIssue
            {
                Field = "ISBN",
                Message = "ISBN-13 required when not using KDP free ISBN",
                Severity = "Error",
                SuggestedFix = "Provide your own ISBN-13 or enable KDP free ISBN"
            });
        }

        return Task.FromResult(Result<List<ValidationIssue>>.Success(issues));
    }

    #region Private Methods

    private static string GetMetadataPath(Guid projectId) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro", "KDP", $"{projectId}.json");

    private static string GetSeriesPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AIBookAuthorPro", "KDP", "series.json");

    private static string GetMarketplaceCurrency(KDPMarketplace marketplace) => marketplace switch
    {
        KDPMarketplace.US => "USD",
        KDPMarketplace.UK => "GBP",
        KDPMarketplace.DE or KDPMarketplace.FR or KDPMarketplace.ES or 
        KDPMarketplace.IT or KDPMarketplace.NL => "EUR",
        KDPMarketplace.JP => "JPY",
        KDPMarketplace.BR => "BRL",
        KDPMarketplace.CA => "CAD",
        KDPMarketplace.MX => "MXN",
        KDPMarketplace.AU => "AUD",
        KDPMarketplace.IN => "INR",
        _ => "USD"
    };

    private static List<KDPCategory> ParseCategories(string content)
    {
        var categories = new List<KDPCategory>();
        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                categories.Add(new KDPCategory
                {
                    CategoryPath = item.GetProperty("categoryPath").GetString() ?? "",
                    Name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                    BISACCode = item.TryGetProperty("bisacCode", out var b) ? b.GetString() : null
                });
            }
        }
        catch { }
        return categories;
    }

    private static List<KeywordSuggestion> ParseKeywords(string content)
    {
        var keywords = new List<KeywordSuggestion>();
        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                keywords.Add(new KeywordSuggestion
                {
                    Keyword = item.GetProperty("keyword").GetString() ?? "",
                    RelevanceScore = item.TryGetProperty("relevanceScore", out var r) ? r.GetDouble() : 0.5,
                    CompetitionLevel = item.TryGetProperty("competitionLevel", out var c) ? c.GetString() : null,
                    RelatedCategory = item.TryGetProperty("relatedCategory", out var rc) ? rc.GetString() : null,
                    IsLongTail = item.TryGetProperty("isLongTail", out var lt) && lt.GetBoolean()
                });
            }
        }
        catch { }
        return keywords;
    }

    private static BookDescriptionSuggestion ParseDescription(string content)
    {
        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new BookDescriptionSuggestion
            {
                Description = root.GetProperty("description").GetString() ?? "",
                Style = root.TryGetProperty("style", out var s) ? s.GetString() ?? "" : "",
                HasHtmlFormatting = root.TryGetProperty("hasHtmlFormatting", out var h) && h.GetBoolean(),
                Hook = root.TryGetProperty("hook", out var ho) ? ho.GetString() : null,
                CallToAction = root.TryGetProperty("callToAction", out var c) ? c.GetString() : null
            };
        }
        catch
        {
            return new BookDescriptionSuggestion { Description = content };
        }
    }

    private static APlusContent ParseAPlusContent(string content)
    {
        var aplus = new APlusContent();
        try
        {
            var json = ExtractJson(content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            aplus.Name = root.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";

            if (root.TryGetProperty("modules", out var modules))
            {
                foreach (var m in modules.EnumerateArray())
                {
                    var module = new APlusModule
                    {
                        Type = Enum.TryParse<APlusModuleType>(
                            m.TryGetProperty("type", out var t) ? t.GetString() : "StandardText",
                            out var mt) ? mt : APlusModuleType.StandardText,
                        Headline = m.TryGetProperty("headline", out var h) ? h.GetString() : null,
                        BodyText = m.TryGetProperty("bodyText", out var b) ? b.GetString() : null,
                        Order = m.TryGetProperty("order", out var o) ? o.GetInt32() : 0
                    };

                    if (m.TryGetProperty("imageAltTexts", out var alts))
                    {
                        foreach (var alt in alts.EnumerateArray())
                        {
                            module.ImageAltTexts.Add(alt.GetString() ?? "");
                        }
                    }

                    aplus.Modules.Add(module);
                }
            }
        }
        catch { }
        return aplus;
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

    private static List<KDPCategory> InitializeCategoryDatabase()
    {
        // Simplified category database - in production, load from comprehensive JSON file
        return new List<KDPCategory>
        {
            new() { CategoryId = "1", CategoryPath = "Fiction > Fantasy > Epic", Name = "Epic Fantasy", BISACCode = "FIC009020" },
            new() { CategoryId = "2", CategoryPath = "Fiction > Fantasy > Contemporary", Name = "Contemporary Fantasy", BISACCode = "FIC009010" },
            new() { CategoryId = "3", CategoryPath = "Fiction > Science Fiction > Space Opera", Name = "Space Opera", BISACCode = "FIC028030" },
            new() { CategoryId = "4", CategoryPath = "Fiction > Romance > Contemporary", Name = "Contemporary Romance", BISACCode = "FIC027020" },
            new() { CategoryId = "5", CategoryPath = "Fiction > Romance > Historical", Name = "Historical Romance", BISACCode = "FIC027050" },
            new() { CategoryId = "6", CategoryPath = "Fiction > Mystery > Cozy", Name = "Cozy Mystery", BISACCode = "FIC022060" },
            new() { CategoryId = "7", CategoryPath = "Fiction > Thriller > Psychological", Name = "Psychological Thriller", BISACCode = "FIC031080" },
            new() { CategoryId = "8", CategoryPath = "Fiction > Horror > Supernatural", Name = "Supernatural Horror", BISACCode = "FIC015000" },
            new() { CategoryId = "9", CategoryPath = "Fiction > Literary", Name = "Literary Fiction", BISACCode = "FIC019000" },
            new() { CategoryId = "10", CategoryPath = "Fiction > Coming of Age", Name = "Coming of Age", BISACCode = "FIC043000" },
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    #endregion
}
