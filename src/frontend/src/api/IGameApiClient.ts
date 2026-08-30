import type {
  CreateGameRequest,
  EngineCapabilityDto,
  EvalRequest,
  GameDto,
  HealthDto,
  MoveInput,
  VersionDto,
} from './types';

/**
 * Everything the UI needs from the backend.
 *
 * Consumers depend on this interface rather than on HttpGameApiClient, so a fake
 * implementation can be swapped in without a network.
 */
export interface IGameApiClient {
  /**
   * Registers a fresh human identity and returns its GUID.
   * The GUID must be sent as `X-Player-Id` on every move that player makes.
   */
  createHumanPlayer(signal?: AbortSignal): Promise<string>;

  /** Starts a game. The first entry in `playerIds` moves first and owns board marker 1. */
  createGame(request: CreateGameRequest, signal?: AbortSignal): Promise<GameDto>;

  /**
   * Reads the current game state. Engine moves are applied asynchronously by a
   * background worker, so this is also the polling endpoint used to observe them.
   */
  getGame(gameId: string, signal?: AbortSignal): Promise<GameDto>;

  /**
   * Submits a human move. The returned game may not yet include the engine's reply -
   * poll {@link getGame} until the turn comes back around.
   */
  makeMove(gameId: string, playerId: string, move: MoveInput, signal?: AbortSignal): Promise<GameDto>;

  /** Lists the available engine opponents. Ids are regenerated whenever the dev database resets. */
  listEngines(signal?: AbortSignal): Promise<EngineCapabilityDto[]>;

  /** Scores a position from the given player's perspective, in the range -1000..1000. */
  evaluate(request: EvalRequest, signal?: AbortSignal): Promise<number>;

  getHealth(signal?: AbortSignal): Promise<HealthDto>;

  getVersion(signal?: AbortSignal): Promise<VersionDto>;
}
