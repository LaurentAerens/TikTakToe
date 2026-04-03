using TikTakToe.Engines.Interface;
using TikTakToe.Engines.Exceptions;

namespace TikTakToe.Engines;

public sealed class RandomEngine : IEngine
{
	private static readonly Random Rng = Random.Shared;

	public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
	{
		if (depth.HasValue)
		{
			throw new UnsupportedDepthException(nameof(RandomEngine));
		}

		var rows = board.GetLength(0);
		var cols = board.GetLength(1);
		var updatedBoard = (int[,])board.Clone();

		var emptySquares = new List<(int X, int Y)>();
		for (var x = 0; x < rows; x++)
		{
			for (var y = 0; y < cols; y++)
			{
				if (updatedBoard[x, y] == 0)
				{
					emptySquares.Add((x, y));
				}
			}
		}

		if (emptySquares.Count == 0)
		{
			throw new NoMoveAvailableException();
		}

		var pick = emptySquares[Rng.Next(emptySquares.Count)];
		updatedBoard[pick.X, pick.Y] = player;

		return (updatedBoard, Eval(updatedBoard, player, depth));
	}

	public int Eval(int[,] board, int player, int? depth = null)
	{
		if (depth.HasValue)
		{
			throw new UnsupportedDepthException(nameof(RandomEngine));
		}

		return Rng.Next(-1000, 1001);
	}
}
