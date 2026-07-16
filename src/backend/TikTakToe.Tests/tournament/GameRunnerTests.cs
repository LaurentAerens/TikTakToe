namespace TikTakToe.Tests.Tournament;

using TikTakToe.Engines.Interface;
using TikTakToe.Tournament;

public class GameRunnerTests
{
    [Fact]
    public void RunGame_WhenPlayer1Wins_ReportsWinForEngine1()
    {
        // Player 1 plays top row; player 2 plays elsewhere and does not block.
        var engine1 = new ScriptedEngine([(0, 0), (0, 1), (0, 2)]);
        var engine2 = new ScriptedEngine([(1, 0), (1, 1), (1, 2)]);

        var result = GameRunner.RunGame(engine1, engine2);

        Assert.Equal(GameResult.Win, result.ResultFromEngine1Perspective);
    }

    [Fact]
    public void RunGame_WhenPlayer2Wins_ReportsLossForEngine1()
    {
        // Player 2 completes a column while player 1 never blocks it.
        // Moves: P1(0,1), P2(0,0), P1(0,2), P2(1,0), P1(1,1), P2(2,0) wins.
        var engine1 = new ScriptedEngine([(0, 1), (0, 2), (1, 1)]);
        var engine2 = new ScriptedEngine([(0, 0), (1, 0), (2, 0)]);

        var result = GameRunner.RunGame(engine1, engine2);

        Assert.Equal(GameResult.Loss, result.ResultFromEngine1Perspective);
    }

    [Fact]
    public void RunGame_WhenBoardFillsWithoutWinner_ReportsDraw()
    {
        // Classic draw line.
        var engine1 = new ScriptedEngine([(0, 0), (0, 1), (1, 2), (2, 0), (2, 2)]);
        var engine2 = new ScriptedEngine([(1, 1), (0, 2), (1, 0), (2, 1)]);

        var result = GameRunner.RunGame(engine1, engine2);

        Assert.Equal(GameResult.Draw, result.ResultFromEngine1Perspective);
    }

    private sealed class ScriptedEngine((int X, int Y)[] moves) : IEngine
    {
        private int _moveIndex;

        public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
        {
            if (this._moveIndex >= moves.Length)
            {
                throw new InvalidOperationException("No scripted move left.");
            }

            var (x, y) = moves[this._moveIndex++];
            var updated = (int[,])board.Clone();
            if (updated[x, y] != 0)
            {
                throw new InvalidOperationException($"Scripted square ({x},{y}) already occupied.");
            }

            updated[x, y] = player;
            return (updated, 0);
        }

        public int Eval(int[,] board, int player, int? depth = null) => 0;
    }
}
