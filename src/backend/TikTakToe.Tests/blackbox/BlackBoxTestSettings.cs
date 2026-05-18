namespace TikTakToe.Tests.BlackBox;

public static class BlackBoxTestSettings
{
    public const string SkipVariableName = "SKIP_BLACKBOX_TESTS";

    public static bool ShouldSkip()
    {
        var value = Environment.GetEnvironmentVariable(SkipVariableName);
        return value is not null
            && (value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }
}
