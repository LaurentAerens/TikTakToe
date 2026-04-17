using TikTakToe.Engines.Interface;

namespace TikTakToe.Services;

internal sealed record EngineRegistration(
    string DisplayName,
    int MaxBoardSizeX,
    int MaxBoardSizeY,
    bool Depth,
    Func<IEngine> Factory);