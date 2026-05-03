namespace TikTakToe.Engines.Interface;

public interface IEngine
{
	(int[,] Board, int Score) Move(int[,] board, int player, int? depth = null);

	int Eval(int[,] board, int player, int? depth = null);

	IReadOnlyCollection<int> SupportedPlayers => [1, 2];
}
