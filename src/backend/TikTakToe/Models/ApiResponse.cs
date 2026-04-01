namespace TikTakToe.Models;

/// <summary>
/// Represents a standard API response envelope.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets an optional error message.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}
