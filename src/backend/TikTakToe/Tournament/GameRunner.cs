namespace TikTakToe.Tournament;

using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;

/// <summary>
/// Runs games between two engines without human interaction.
/// Portable implementation suitable for both console and backend use.
/// </summary>
public static class GameRunner
{
    /// <summary>
    /// Runs a single game between two engines.
    /// </summary>
    /// <param name="engine1">First engine (plays as player 1).</param>
    /// <param name="engine2">Second engine (plays as player 2).</param>
    /// <param name="searchDepth">Optional search depth for engines that support it.</param>
    /// <returns>Match result with the result from engine1's perspective.</returns>
    public static MatchResult RunGame(IEngine engine1, IEngine engine2, int? searchDepth = null)
    {
        var board = BoardRules.CreateEmpty();
        var currentPlayer = 1;
        var moveCount = 0;

        while (true)
        {
            try
            {
                var currentEngine = currentPlayer == 1 ? engine1 : engine2;
                (board, _) = MakeMove(currentEngine, board, currentPlayer, searchDepth);
                moveCount++;
            }
            catch (NoMoveAvailableException)
            {
                return new MatchResult(
                    EngineRegistry.GetId(engine1),
                    EngineRegistry.GetId(engine2),
                    GameResult.Draw,
                    moveCount);
            }
            catch (BoardSizeNotSupportedException)
            {
                var resultFromPerspective = currentPlayer == 1 ? GameResult.Loss : GameResult.Win;
                return new MatchResult(
                    EngineRegistry.GetId(engine1),
                    EngineRegistry.GetId(engine2),
                    resultFromPerspective,
                    moveCount);
            }

            // engine1 is always player 1, so results are from engine1's perspective.
            if (BoardRules.PlayerHasWon(board, 1))
            {
                return new MatchResult(
                    EngineRegistry.GetId(engine1),
                    EngineRegistry.GetId(engine2),
                    GameResult.Win,
                    moveCount);
            }

            if (BoardRules.PlayerHasWon(board, 2))
            {
                return new MatchResult(
                    EngineRegistry.GetId(engine1),
                    EngineRegistry.GetId(engine2),
                    GameResult.Loss,
                    moveCount);
            }

            if (BoardRules.IsFull(board))
            {
                return new MatchResult(
                    EngineRegistry.GetId(engine1),
                    EngineRegistry.GetId(engine2),
                    GameResult.Draw,
                    moveCount);
            }

            currentPlayer = currentPlayer == 1 ? 2 : 1;
        }
    }

    /// <summary>
    /// Invokes an engine move, falling back when the engine does not support depth.
    /// </summary>
    /// <returns></returns>
    public static (int[,] Board, int Score) MakeMove(IEngine engine, int[,] board, int player, int? searchDepth)
    {
        try
        {
            return engine.Move(board, player, searchDepth);
        }
        catch (UnsupportedDepthException) when (searchDepth.HasValue)
        {
            return engine.Move(board, player, depth: null);
        }
    }
}
