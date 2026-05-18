namespace TikTakToe.Tests.BlackBox;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BlackBoxFactAttribute : FactAttribute
{
    public BlackBoxFactAttribute()
    {
        if (BlackBoxTestSettings.ShouldSkip())
        {
            this.Skip = $"Skipped because {BlackBoxTestSettings.SkipVariableName}=true.";
        }
    }
}
