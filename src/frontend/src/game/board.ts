import type { GameDto } from '@/api';
import { sortMoves } from './state';

export const EMPTY_CELL = 0;

export interface Cell {
  x: number;
  y: number;
}

/** Board markers in turn order; anything beyond falls back to the raw number. */
const MARKS = ['X', 'O'] as const;

/** Directions scanned for a winning run: horizontal, vertical, and both diagonals. */
const DIRECTIONS: readonly Cell[] = [
  { x: 0, y: 1 },
  { x: 1, y: 0 },
  { x: 1, y: 1 },
  { x: 1, y: -1 },
];

export function glyphForValue(value: number): string {
  if (value === EMPTY_CELL) {
    return '';
  }

  return MARKS[value - 1] ?? String(value);
}

export function createEmptyBoard(rows: number, cols: number): number[][] {
  return Array.from({ length: rows }, () => Array.from({ length: cols }, () => EMPTY_CELL));
}

export function boardSize(board: number[][]): { rows: number; cols: number } {
  return { rows: board.length, cols: board[0]?.length ?? 0 };
}

/**
 * Rebuilds the position after the first `moveCount` moves.
 * Backs the history scrubber; passing `moves.length` reproduces `game.board`.
 */
export function boardAfter(game: GameDto, moveCount: number): number[][] {
  const { rows, cols } = boardSize(game.board);
  const board = createEmptyBoard(rows, cols);

  for (const move of sortMoves(game.moves).slice(0, moveCount)) {
    if (move.x >= 0 && move.x < rows && move.y >= 0 && move.y < cols) {
      board[move.x][move.y] = move.value;
    }
  }

  return board;
}

/**
 * Locates the winning run for display only - the API reports that someone won but not
 * which cells did it. Mirrors GameRules.GetWinner: the target is 3 whenever both
 * dimensions allow it, otherwise the smaller dimension.
 */
export function findWinningLine(board: number[][], winnerValue: number | null): Cell[] {
  if (winnerValue === null || winnerValue <= 0) {
    return [];
  }

  const { rows, cols } = boardSize(board);
  const target = rows >= 3 && cols >= 3 ? 3 : Math.min(rows, cols);

  if (target <= 0) {
    return [];
  }

  for (let x = 0; x < rows; x += 1) {
    for (let y = 0; y < cols; y += 1) {
      if (board[x][y] !== winnerValue) {
        continue;
      }

      for (const direction of DIRECTIONS) {
        const line: Cell[] = [{ x, y }];

        for (let step = 1; step < target; step += 1) {
          const nextX = x + direction.x * step;
          const nextY = y + direction.y * step;
          const inBounds = nextX >= 0 && nextX < rows && nextY >= 0 && nextY < cols;

          if (!inBounds || board[nextX][nextY] !== winnerValue) {
            break;
          }

          line.push({ x: nextX, y: nextY });
        }

        if (line.length === target) {
          return line;
        }
      }
    }
  }

  return [];
}
