# Engines

## Available Engines

| Engine                 | Strategy description                                             |
| ---------------------- | ---------------------------------------------------------------- |
| `ClassicalEngine`      | Perfect minimax — both players play optimally.                   |
| `HalfDepthEngine`      | Minimax with heuristic evaluation (positional bonuses).          |
| `RandomEngine`         | Picks a random legal move every time.                            |
| `OpportunityEngine`    | Engine plays optimally; assumes opponent moves randomly.         |
| `HalftunityEngine`     | Heuristic evaluation + random-opponent assumption.               |
| `InverseEngine`        | Classical weak style that tries to maximize the opponent's odds. |
| `DisconnectedEngine`   | Maximin engine with heuristic evaluation.                        |
| `PredicamentEngine`    | Opportunity but tries to maximize opponent's chance of winning.  |
| `DisconnicamentEngine` | Predicament + heuristic evaluation.                              |

---

## `IEngine` Interface

```csharp
public interface IEngine
{
    (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null);
    int Eval(int[,] board, int player, int? depth = null);
}
```

The board is a 3×3 `int[,]` where `0` = empty, `1` = Player 1, `2` = Player 2.

**`Move(board, player, depth?)`**  
Returns the board state after the engine picks its move, together with the resulting score. `depth` limits the search; `null` searches to full resolution.

**`Eval(board, player, depth?)`**  
Scores the current board position without making a move. Same parameters and scale as `Move`.

Scores are from Player 1's perspective: `+1000` = Player 1 wins, `-1000` = Player 2 wins, `0` = draw / unknown.

---

## Creating a New Engine

### Option A — Compose from the minimax base (recommended)

Extend `MinimaxEngineBase` and supply an `IBoardEvaluator` and an `IOpponentStrategy`. The base class handles search, depth, and board validation automatically.

```csharp
public sealed class MyEngine : MinimaxEngineBase
{
    public MyEngine()
        : base(new ClassicalBoardEvaluator(), new MinimaxOpponentStrategy())
    {
    }
}
```

Swap in any evaluator or strategy — or write your own by implementing `IBoardEvaluator` / `IOpponentStrategy`.

### Option B — Implement `IEngine` from scratch

Use this when the minimax framework does not fit (e.g. a neural-network engine).

```csharp
public sealed class MyEngine : IEngine
{
    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        // pick a square, place the piece, return (newBoard, score)
        throw new NotImplementedException();
    }

    public int Eval(int[,] board, int player, int? depth = null)
    {
        // return a score in the [-1000, 1000] range
        throw new NotImplementedException();
    }
}
```

### Testing

Add your engine to the `TheoryData` in `TikTakToe.Tests/engines/EngineContractTests.cs` so the shared contract tests cover it automatically.
