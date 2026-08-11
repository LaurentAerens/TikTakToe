namespace TikTakToe.Services;

using TikTakToe.Engines.Interface;

internal sealed record EngineRegistration(
    string DisplayName,
    int MaxBoardSizeX,
    int MaxBoardSizeY,
    bool Depth,
    Func<IEngine> Factory,
    IReadOnlyList<int>? CustomDepths = null)
{
    public IReadOnlyList<int?> GetSupportedDepths()
    {
        if (this.CustomDepths is { Count: > 0 })
        {
            return this.CustomDepths.Select(d => (int?)d).ToArray();
        }

        if (this.Depth)
        {
            return [1, 2, 3];
        }

        return [null];
    }
}
