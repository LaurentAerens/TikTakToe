namespace TikTakToe.Services;

using TikTakToe.Engines.Interface;

internal sealed record EngineRegistration(
    string DisplayName,
    int MaxBoardSizeX,
    int MaxBoardSizeY,
    bool Depth,
    Func<IEngine> Factory);
