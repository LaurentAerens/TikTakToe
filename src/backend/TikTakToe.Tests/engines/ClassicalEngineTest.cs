using System.Reflection;
using System.Collections.Generic;
using TikTakToe.Engines.Exceptions;
using TikTakToe.Engines.Interface;
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

	[Fact]
	public void Eval_WithDepthZero_Returns0()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 0, 0 },
			{ 0, 2, 0 },
			{ 0, 0, 0 }
		};

		var score = engine.Eval(board, player: 1, depth: 0);

		Assert.Equal(0, score);
	}

	[Fact]
	public void Eval_NonStandardBoard_ThrowsBoardSizeNotSupportedException()
	{
		var engine = new ClassicalEngine();
		var board = new int[4,4]
		{
			{ 1, 2, 1, 2 },
			{ 2, 1, 0, 1 },
			{ 1, 2, 2, 1 },
			{ 2, 1, 1, 2 }
		};

		var ex = Record.Exception(() => engine.Eval(board, player: 1));

		Assert.NotNull(ex);
		Assert.IsType<BoardSizeNotSupportedException>(ex);
	}

	[Fact]
	public void ChangePlayer_PrivateMethod_TogglesBetween1And2()
	{
		var engine = new ClassicalEngine();
		var mi = typeof(ClassicalEngine).GetMethod("ChangePlayer", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(mi);

		var result1 = mi!.Invoke(engine, new object[] { 1 });
		Assert.Equal(2, result1);

		var result2 = mi.Invoke(engine, new object[] { 2 });
		Assert.Equal(1, result2);
	}

	[Fact]
	public void Move_WithFullBoard_ThrowsNoMoveAvailableException()
	{
		var engine = new ClassicalEngine();
		var fullBoard = new int[3,3]
		{
			{ 1, 2, 1 },
			{ 2, 1, 2 },
			{ 2, 1, 2 }
		};

		var ex = Record.Exception(() => engine.Move(fullBoard, player: 1));

		Assert.NotNull(ex);
		Assert.IsType<NoMoveAvailableException>(ex);
	}

	[Fact]
	public void Move_WithSingleAvailableMove_ReturnsThatMoveAndScore()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 2, 1 },
			{ 2, 1, 2 },
			{ 2, 1, 0 }
		};

		var (updatedBoard, score) = engine.Move(board, player: 2);

		// Ensure only one cell changed and it was set to player 2
		var changed = 0;
		for (var x = 0; x < 3; x++)
		{
			for (var y = 0; y < 3; y++)
			{
				if (board[x,y] != updatedBoard[x,y])
				{
					changed++;
					Assert.Equal(0, board[x,y]);
					Assert.Equal(2, updatedBoard[x,y]);
				}
			}
		}

		Assert.Equal(1, changed);
		Assert.InRange(score, -1000, 1000);
	}

	[Fact]
	public void Move_PicksImmediateWin_ForPlayer1()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 1, 1, 0 },
			{ 2, 2, 0 },
			{ 0, 0, 0 }
		};

		var (updatedBoard, score) = engine.Move(board, player: 1);

		Assert.Equal(1000, score);
		Assert.Equal(1, updatedBoard[0,2]);
	}

	[Fact]
	public void Move_PicksImmediateWin_ForPlayer2()
	{
		var engine = new ClassicalEngine();
		var board = new int[3,3]
		{
			{ 2, 2, 0 },
			{ 1, 1, 0 },
			{ 0, 0, 0 }
		};

		var (updatedBoard, score) = engine.Move(board, player: 2);

		Assert.Equal(-1000, score);
		Assert.Equal(2, updatedBoard[0,2]);
	}

	[Fact]
	public void Eval_FullBoard_WithDepth_Returns0_MinimaxNoMovesBranch()
	{
		var engine = new ClassicalEngine();
		var fullBoard = new int[3,3]
		{
			{ 1, 2, 1 },
			{ 2, 1, 2 },
			{ 2, 1, 2 }
		};

		var score = engine.Eval(fullBoard, player: 1, depth: 1);

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

