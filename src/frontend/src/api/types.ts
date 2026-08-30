/**
 * TypeScript mirrors of the backend DTOs in `src/backend/TikTakToe/Models`.
 * Minimal APIs serialise with JsonSerializerDefaults.Web, so every property is camelCase
 * and nulls are written rather than omitted.
 */

/** Envelope wrapping every game, player, engine and eval response. */
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}

export interface PlayerDto {
  id: string;
  isEngine: boolean;
  /** For engine players this is the engine capability id; always null for humans. */
  externalId: string | null;
}

export interface MoveDto {
  id: string;
  /** Row coordinate, 0-indexed. */
  x: number;
  /** Column coordinate, 0-indexed. */
  y: number;
  /** Board marker of the player who made the move (1-based, matching turn order). */
  value: number;
  moveNumber: number;
}

export interface GameStateDto {
  isGameOver: boolean;
  /** null while in progress, -1 for a draw, otherwise the winner's board marker. */
  winnerValue: number | null;
  winnerPlayerId: string | null;
}

export interface GameDto {
  id: string;
  /** 0 is empty; any other value is the board marker of its owner. */
  board: number[][];
  /** Ordered by turn order, so the player at index i owns board marker i + 1. */
  players: PlayerDto[];
  /** Returned in unspecified order - sort by moveNumber before use. */
  moves: MoveDto[];
  /** null once the game is over. */
  waitingForPlayerId: string | null;
  state: GameStateDto;
}

export interface EngineCapabilityDto {
  /** Identifies the engine to POST /eval. */
  id: string;
  /** Identifies the engine as a participant in POST /games. */
  playerId: string;
  displayName: string;
  maxBoardSizeX: number;
  maxBoardSizeY: number;
  /** Whether the engine accepts a custom search depth. */
  depth: boolean;
  supportedPlayers: number[];
}

export interface EvalResponseDto {
  score: number;
}

export interface HealthDto {
  status: string;
}

export interface VersionDto {
  version: string;
}

export interface CreateGameRequest {
  rows: number;
  cols: number;
  /** Turn order follows this array; between 2 and 1000 ids. */
  playerIds: string[];
}

export interface MoveInput {
  x: number;
  y: number;
}

export interface EvalRequest {
  engineId: string;
  board: number[][];
  player: number;
  depth?: number | null;
}

/** `GameStateDto.winnerValue` uses this sentinel for a drawn game. */
export const DRAW_WINNER_VALUE = -1;

/** Magnitude the engines report for a decisive position; used to normalise eval scores. */
export const EVAL_WIN_SCORE = 1000;
