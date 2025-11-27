// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Gets whether the validation passed.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets whether the validation failed.
    /// </summary>
    public bool IsInvalid => !IsValid;

    private ValidationResult(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new(Array.Empty<ValidationError>());

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(params ValidationError[] errors) => new(errors);

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(string propertyName, string message) =>
        new(new[] { new ValidationError(propertyName, message) });

    /// <summary>
    /// Creates a failed validation result from multiple errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors) =>
        new(errors.ToList());

    /// <summary>
    /// Combines multiple validation results.
    /// </summary>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var errors = results.SelectMany(r => r.Errors).ToList();
        return errors.Count == 0 ? Success() : new ValidationResult(errors);
    }

    /// <summary>
    /// Gets the first error message or null if valid.
    /// </summary>
    public string? GetFirstErrorMessage() => Errors.FirstOrDefault()?.Message;

    /// <summary>
    /// Gets all error messages joined by newlines.
    /// </summary>
    public string GetAllErrorMessages(string separator = "\n") =>
        string.Join(separator, Errors.Select(e => e.Message));

    /// <summary>
    /// Gets errors for a specific property.
    /// </summary>
    public IEnumerable<ValidationError> GetErrorsFor(string propertyName) =>
        Errors.Where(e => e.PropertyName == propertyName);
}

/// <summary>
/// Represents a single validation error.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Gets the property name that failed validation.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the error code (optional).
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the attempted value that failed validation.
    /// </summary>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Initializes a new instance of ValidationError.
    /// </summary>
    public ValidationError(
        string propertyName,
        string message,
        string? errorCode = null,
        object? attemptedValue = null)
    {
        PropertyName = propertyName;
        Message = message;
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue;
    }

    /// <inheritdoc />
    public override string ToString() => $"{PropertyName}: {Message}";
}
