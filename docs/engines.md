# Engine Architecture

This document explains the engine system used by TikTakToe: what the interfaces are, the available implementations, how the pieces fit together, and how to create your own engine.

---

## Table of Contents

- [Board Representation](#board-representation)
- [Core Interface – `IEngine`](#core-interface--iengine)
- [Available Engines](#available-engines)
- [Board Evaluators – `IBoardEvaluator`](#board-evaluators--iboardevaluator)
- [Opponent Strategies – `IOpponentStrategy`](#opponent-strategies--iopponentstrategy)
- [Minimax Base Class – `MinimaxEngineBase`](#minimax-base-class--minimaxenginebase)
- [Exceptions](#exceptions)
- [Creating a New Engine](#creating-a-new-engine)

---

## Board Representation

The board is a 3×3 `int[,]` array where:

| Value | Meaning         |
|-------|-----------------|
| `0`   | Empty square    |
| `1`   | Player 1 (X)    |
| `2`   | Player 2 (O)    |

Indices follow `[row, col]` with `[0,0]` at the top-left and `[2,2]` at the bottom-right.

All engines currently support **3×3 boards only**. Passing a board of any other size throws `BoardSizeNotSupportedException`.

---

## Core Interface – `IEngine`

**Namespace:** `TikTakToe.Engines.Interface`  
**File:** `src/backend/TikTakToe/Engines/Interface/IEngine.cs`

```csharp
public interface IEngine
{
    (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null);
    int Eval(int[,] board, int player, int? depth = null);
}
```

### `Move`

Selects the best move for `player` and returns the resulting board together with its score.

| Parameter | Type      | Description                                                                                                   |
|-----------|-----------|---------------------------------------------------------------------------------------------------------------|
| `board`   | `int[,]`  | Current board state (not mutated).                                                                            |
| `player`  | `int`     | The player whose turn it is (`1` or `2`).                                                                     |
| `depth`   | `int?`    | Optional search depth. `null` means "search to full resolution". Engines that ignore depth throw `UnsupportedDepthException` if a value is supplied. |

**Returns:** `(int[,] Board, int Score)` — the board after the move and the evaluated score from `player`'s perspective.

### `Eval`

Evaluates a board position without making a move. Useful for scoring a position before committing to it.

Parameters and return type are the same as `Move`, except no board state change occurs.

---

## Available Engines

| Engine             | Evaluator            | Opponent Strategy     | Opponent Model         |
|--------------------|----------------------|-----------------------|------------------------|
| `ClassicalEngine`  | Classical (terminal) | Minimax               | Perfect play           |
| `HalfDepthEngine`  | HalfDepth (heuristic)| Minimax               | Perfect play           |
| `RandomEngine`     | Random               | –                     | Random moves           |
| `OppertunityEngine`| Classical (terminal) | Oppertunity           | Random/suboptimal play |
| `HalftunityEngine` | HalfDepth (heuristic)| Oppertunity           | Random/suboptimal play |

### `ClassicalEngine`

Uses a terminal-only evaluator combined with the standard minimax strategy. Both players are assumed to play perfectly, producing the game-theoretically optimal move. Because Tic-Tac-Toe is solved, the engine never loses when it goes first, and always forces at least a draw otherwise.

### `HalfDepthEngine`

Extends the classical engine with a heuristic board evaluator that awards bonus points for positional threats (two-in-a-row, centre control) even in non-terminal positions. This lets the engine "prefer" stronger intermediate positions when the search is depth-limited.

### `RandomEngine`

Picks a random legal move. Does **not** accept a `depth` parameter — passing one throws `UnsupportedDepthException`. Useful as a baseline opponent or for testing.

### `OppertunityEngine`

Plays optimally itself but models the opponent as playing randomly (scores are averaged over all opponent replies instead of minimised). This makes the engine more aggressive: it takes risks that would be punished against a perfect opponent but pay off against weaker or unpredictable play.

### `HalftunityEngine`

Combines the heuristic evaluator of `HalfDepthEngine` with the random-opponent assumption of `OppertunityEngine`. A good choice when you want the engine to chase positional advantages while still being exploitative against non-perfect opponents.

---

## Board Evaluators – `IBoardEvaluator`

**Namespace:** `TikTakToe.Engines.Evaluation`  
**File:** `src/backend/TikTakToe/Engines/Evaluation/IBoardEvaluator.cs`

```csharp
public interface IBoardEvaluator
{
    int Evaluate(int[,] board);
}
```

The evaluator converts a board snapshot into a numeric score from **Player 1's perspective**:

| Score     | Meaning                    |
|-----------|----------------------------|
| `+1000`   | Player 1 wins              |
| `-1000`   | Player 2 wins              |
| `0`       | Draw or non-terminal       |
| other     | Heuristic strength estimate|

### `ClassicalBoardEvaluator`

Returns `+1000`, `-1000`, or `0`. Non-terminal, non-drawn positions score `0`, relying entirely on the search depth to discover winning lines.

### `HalfDepthBoardEvaluator`

Adds heuristic scoring on top of the terminal check:

- **Centre control:** when the centre square `[1,1]` is empty, each pair of opposite corners belonging to the same player awards ±500.
- **Two-in-a-row:** any row, column, or diagonal that contains two pieces of the same player and one empty square awards ±500.

Final scores are clamped to `[-1000, 1000]`.

### `BoardEvaluationPrimitives` (static utility)

Shared helper used by both evaluators. Provides a fast `EvaluateTerminalState(int[,] board)` method that checks for wins and draws, checking the centre piece first for speed.

---

## Opponent Strategies – `IOpponentStrategy`

**Namespace:** `TikTakToe.Engines.Search`  
**File:** `src/backend/TikTakToe/Engines/Search/IOpponentStrategy.cs`

```csharp
public interface IOpponentStrategy
{
    /// <summary>
    /// Aggregates child move scores for the current search node.
    /// </summary>
    int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer);
}
```

At each node in the search tree the strategy decides how to combine the child scores into a single value for the parent.

| Parameter       | Description                                            |
|-----------------|--------------------------------------------------------|
| `scores`        | Scores of all legal continuations from this position.  |
| `currentPlayer` | The player whose turn it is at this node.              |
| `enginePlayer`  | The player the engine is playing for.                  |

### `MinimaxOpponentStrategy`

Standard minimax: the engine's player maximises, the opponent minimises.

```
enginePlayer's turn  → scores.Max()
opponent's turn      → scores.Min()
```

### `OppertunityOpponentStrategy`

The engine still maximises/minimises on its own turn, but when it is the opponent's turn the scores are **averaged**, modelling an opponent who moves randomly.

```
enginePlayer's turn  → scores.Max() / scores.Min()  (same as minimax)
opponent's turn      → (int)Math.Round(scores.Average())
```

---

## Minimax Base Class – `MinimaxEngineBase`

**File:** `src/backend/TikTakToe/Engines/MinimaxEngineBase.cs`

All search-based engines inherit from this abstract class. It implements `IEngine` and takes an `IBoardEvaluator` and an optional `IOpponentStrategy` via constructor injection.

```csharp
protected MinimaxEngineBase(
    IBoardEvaluator boardEvaluator,
    IOpponentStrategy? opponentStrategy = null)
```

If `opponentStrategy` is `null`, `MinimaxOpponentStrategy` is used by default.

**Key behaviours:**

- **3×3 only** – throws `BoardSizeNotSupportedException` for any other size.
- **Default depth** – when `depth` is `null`, searches to the number of remaining empty squares (full resolution).
- **Terminal positions** – immediately return the evaluator's score without recursing further.
- **Parallel root expansion** – the immediate children of the root position are evaluated in parallel using `Parallel.ForEach`, making full use of available CPU cores.
- **Sequential recursion** – deeper levels search sequentially to avoid excessive thread contention.

---

## Exceptions

All exceptions live in `TikTakToe.Engines.Exceptions` and inherit from `InvalidOperationException`.

| Exception                      | Thrown when                                                         |
|-------------------------------|---------------------------------------------------------------------|
| `NoMoveAvailableException`    | `Move` is called on a full board or a board with no legal squares.  |
| `BoardSizeNotSupportedException` | The supplied board is not 3×3.                                  |
| `UnsupportedDepthException`   | A `depth` value is passed to an engine that does not support it (e.g. `RandomEngine`). |

---

## Creating a New Engine

There are three levels at which you can customise engine behaviour.

### Option A – Compose a new `MinimaxEngineBase` subclass

This is the quickest path. Pick (or create) a board evaluator and an opponent strategy, then wire them up:

```csharp
public sealed class MyCustomEngine : MinimaxEngineBase
{
    public MyCustomEngine()
        : base(new MyBoardEvaluator(), new MyOpponentStrategy())
    {
    }
}
```

`MinimaxEngineBase` provides `Move`, `Eval`, board validation, depth handling, and parallel root search automatically.

### Option B – Implement a new `IBoardEvaluator`

Create a class that implements `IBoardEvaluator.Evaluate(int[,] board)` and returns a score in the `[-1000, 1000]` range from Player 1's perspective. You can reuse `BoardEvaluationPrimitives.EvaluateTerminalState` for the win/loss/draw check and layer additional heuristics on top:

```csharp
public sealed class MyBoardEvaluator : IBoardEvaluator
{
    public int Evaluate(int[,] board)
    {
        int terminal = BoardEvaluationPrimitives.EvaluateTerminalState(board);
        if (terminal != 0) return terminal;

        // Add heuristic scoring here…
        return 0;
    }
}
```

### Option C – Implement a new `IOpponentStrategy`

Create a class that implements `IOpponentStrategy.AggregateScores` to change how child-node scores are combined. For example, a "pessimistic" strategy could assume the opponent always plays the move that is worst for the engine:

```csharp
public sealed class PessimisticOpponentStrategy : IOpponentStrategy
{
    public int AggregateScores(IReadOnlyList<int> scores, int currentPlayer, int enginePlayer)
    {
        // Always assume the worst possible outcome for the engine
        return currentPlayer == enginePlayer ? scores.Max() : scores.Min();
    }
}
```

### Option D – Implement `IEngine` from scratch

If the minimax framework does not suit your needs (e.g. you want a neural-network-based engine), implement `IEngine` directly:

```csharp
public sealed class MyAlternativeEngine : IEngine
{
    public (int[,] Board, int Score) Move(int[,] board, int player, int? depth = null)
    {
        // Your custom logic here
        throw new NotImplementedException();
    }

    public int Eval(int[,] board, int player, int? depth = null)
    {
        // Your custom logic here
        throw new NotImplementedException();
    }
}
```

### Testing your engine

All engines should satisfy the shared contract tests in `TikTakToe.Tests/engines/EngineContractTests.cs`. Add your engine to the `TheoryData` in that file so it is automatically exercised by the existing suite.
