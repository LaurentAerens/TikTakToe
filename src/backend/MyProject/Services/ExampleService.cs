namespace MyProject.Services;

/// <summary>
/// A simple example service implementation.
/// </summary>
public class ExampleService : IExampleService
{
    /// <inheritdoc />
    public string Greet(string name) => $"Hello, {name}!";
}
