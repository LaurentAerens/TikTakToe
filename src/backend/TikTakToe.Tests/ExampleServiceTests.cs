using TikTakToe.Services;

namespace TikTakToe.Tests;

public class ExampleServiceTests
{
    [Fact]
    public void Greet_ReturnsExpectedMessage()
    {
        var service = new ExampleService();
        var result = service.Greet("World");
        Assert.Equal("Hello, World!", result);
    }
}
