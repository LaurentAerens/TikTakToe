namespace TikTakToe.Services;

/// <summary>
/// Defines the contract for the example service.
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Returns a greeting message.
    /// </summary>
    /// <param name="name">The name to greet.</param>
    /// <returns>A greeting string.</returns>
    string Greet(string name);
}
