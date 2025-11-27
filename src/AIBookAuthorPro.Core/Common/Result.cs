// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Use this pattern instead of throwing exceptions for expected failure cases.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed record Result<T>
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; private init; }
    
    /// <summary>
    /// Gets whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the success value. Only valid when IsSuccess is true.
    /// </summary>
    public T? Value { get; private init; }
    
    /// <summary>
    /// Gets the error message. Only valid when IsFailure is true.
    /// </summary>
    public string? Error { get; private init; }
    
    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; private init; }

    private Result() { }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful Result containing the value.</returns>
    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message describing what went wrong.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A failed Result containing the error information.</returns>
    public static Result<T> Failure(string error, Exception? exception = null) => new()
    {
        IsSuccess = false,
        Error = error,
        Exception = exception
    };

    /// <summary>
    /// Pattern matches on the result, executing the appropriate function based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The return type of the match functions.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }

    /// <summary>
    /// Maps the success value to a new type using the provided mapping function.
    /// </summary>
    /// <typeparam name="TNew">The new value type.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new Result with the mapped value or the original error.</returns>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        
        return IsSuccess 
            ? Result<TNew>.Success(mapper(Value!)) 
            : Result<TNew>.Failure(Error!, Exception);
    }

    /// <summary>
    /// Chains another operation that returns a Result, only executing if this Result is successful.
    /// </summary>
    /// <typeparam name="TNew">The new value type.</typeparam>
    /// <param name="binder">The binding function that returns a new Result.</param>
    /// <returns>The Result from the binding function or the original error.</returns>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        
        return IsSuccess ? binder(Value!) : Result<TNew>.Failure(Error!, Exception);
    }

    /// <summary>
    /// Gets the value or throws an InvalidOperationException if the result is a failure.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public T GetValueOrThrow()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException(
                $"Cannot get value from failed result: {Error}",
                Exception);
        }
        return Value!;
    }

    /// <summary>
    /// Gets the value or returns the specified default value if the result is a failure.
    /// </summary>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The success value or the default value.</returns>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value! : defaultValue;
}

/// <summary>
/// Represents the result of an operation that has no return value.
/// </summary>
public sealed record Result
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; private init; }
    
    /// <summary>
    /// Gets whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the error message. Only valid when IsFailure is true.
    /// </summary>
    public string? Error { get; private init; }
    
    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; private init; }

    private Result() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful Result.</returns>
    public static Result Success() => new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message describing what went wrong.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A failed Result containing the error information.</returns>
    public static Result Failure(string error, Exception? exception = null) => new()
    {
        IsSuccess = false,
        Error = error,
        Exception = exception
    };

    /// <summary>
    /// Pattern matches on the result, executing the appropriate action based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result is a failure.</param>
    public void Match(Action onSuccess, Action<string> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error!);
    }

    /// <summary>
    /// Throws an InvalidOperationException if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public void ThrowIfFailure()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException(
                $"Operation failed: {Error}",
                Exception);
        }
    }
}
