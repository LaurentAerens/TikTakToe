using System.Collections.Generic;
using TikTakToe.Engines;

namespace TikTakToe.Tests.Engines;

public class ClassicalEngineTest
{
	[Fact]
	public void Eval_Player1RowWin_Returns1000()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 1, 1 },
			{ 0, 2, 0 },
			{ 0, 0, 2 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(1000, score);
	}

	[Fact]
	public void Eval_Player2DiagonalWin_ReturnsMinus1000()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 2, 1, 1 },
			{ 0, 2, 0 },
			{ 0, 0, 2 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(-1000, score);
	}

	[Fact]
	public void Eval_CenterAndOppositeCorners_Player1_Returns1000()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 0, 0 },
			{ 0, 1, 0 },
			{ 0, 0, 1 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(1000, score);
	}

	[Fact]
	public void Eval_CenterAndVerticalSides_Player1_Returns1000()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 0, 1, 0 },
			{ 0, 1, 0 },
			{ 0, 1, 0 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(1000, score);
	}

	[Fact]
	public void Eval_CenterAndOppositeCorners_Player2_ReturnsMinus1000()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 2, 0, 0 },
			{ 0, 2, 0 },
			{ 0, 0, 2 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(-1000, score);
	}

	[Fact]
	public void Eval_FullDrawBoard_Returns0()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 2, 1 },
			{ 2, 1, 2 },
			{ 2, 1, 2 }
		};

		var score = engine.Eval(board, player: 1);

		Assert.Equal(0, score);
	}

	public static IEnumerable<object[]> WinningBoards()
	{
		// Player 1 winning lines (expect 1000)
		yield return new object[] { new int[3,3] { {1,1,1}, {0,0,0}, {0,0,0} }, 1000 };
		yield return new object[] { new int[3,3] { {0,0,0}, {1,1,1}, {0,0,0} }, 1000 };
		yield return new object[] { new int[3,3] { {0,0,0}, {0,0,0}, {1,1,1} }, 1000 };
		yield return new object[] { new int[3,3] { {1,0,0}, {1,0,0}, {1,0,0} }, 1000 };
		yield return new object[] { new int[3,3] { {0,1,0}, {0,1,0}, {0,1,0} }, 1000 };
		yield return new object[] { new int[3,3] { {0,0,1}, {0,0,1}, {0,0,1} }, 1000 };
		yield return new object[] { new int[3,3] { {1,0,0}, {0,1,0}, {0,0,1} }, 1000 };
		yield return new object[] { new int[3,3] { {0,0,1}, {0,1,0}, {1,0,0} }, 1000 };

		// Player 2 winning lines (expect -1000)
		yield return new object[] { new int[3,3] { {2,2,2}, {0,0,0}, {0,0,0} }, -1000 };
		yield return new object[] { new int[3,3] { {0,0,0}, {2,2,2}, {0,0,0} }, -1000 };
		yield return new object[] { new int[3,3] { {0,0,0}, {0,0,0}, {2,2,2} }, -1000 };
		yield return new object[] { new int[3,3] { {2,0,0}, {2,0,0}, {2,0,0} }, -1000 };
		yield return new object[] { new int[3,3] { {0,2,0}, {0,2,0}, {0,2,0} }, -1000 };
		yield return new object[] { new int[3,3] { {0,0,2}, {0,0,2}, {0,0,2} }, -1000 };
		yield return new object[] { new int[3,3] { {2,0,0}, {0,2,0}, {0,0,2} }, -1000 };
		yield return new object[] { new int[3,3] { {0,0,2}, {0,2,0}, {2,0,0} }, -1000 };
	}

	[Theory]
	[MemberData(nameof(WinningBoards))]
	public void Eval_WinningBoards_ReturnsExpectedTerminalScore(int[,] board, int expected)
	{
		var engine = new ClassicalEngine();

		// Terminal detection should be independent of depth value.
		var scoreDefault = engine.Eval(board, player: 1);
		var scoreWithDepth = engine.Eval(board, player: 1, depth: 3);

		Assert.Equal(expected, scoreDefault);
		Assert.Equal(expected, scoreWithDepth);
	}
}

