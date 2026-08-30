import { HttpTransport, type HttpTransportOptions } from './http';
import type { IGameApiClient } from './IGameApiClient';
import type {
  CreateGameRequest,
  EngineCapabilityDto,
  EvalRequest,
  EvalResponseDto,
  GameDto,
  HealthDto,
  MoveInput,
  VersionDto,
} from './types';

/** Header the backend reads to identify the player submitting a move. */
const PLAYER_ID_HEADER = 'X-Player-Id';

/** fetch-based implementation of {@link IGameApiClient}. */
export class HttpGameApiClient implements IGameApiClient {
  private readonly transport: HttpTransport;

  constructor(options: HttpTransportOptions = {}) {
    this.transport = new HttpTransport(options);
  }

  createHumanPlayer(signal?: AbortSignal): Promise<string> {
    // isEngine must stay false: the backend throws an unhandled 500 when an engine
    // player is registered without a valid engine GUID in externalId.
    return this.transport.requestEnveloped<string>('/players', {
      method: 'POST',
      body: { isEngine: false, externalId: null },
      signal,
    });
  }

  createGame(request: CreateGameRequest, signal?: AbortSignal): Promise<GameDto> {
    return this.transport.requestEnveloped<GameDto>('/games', {
      method: 'POST',
      body: request,
      signal,
    });
  }

  getGame(gameId: string, signal?: AbortSignal): Promise<GameDto> {
    return this.transport.requestEnveloped<GameDto>(`/games/${gameId}`, { signal });
  }

  makeMove(gameId: string, playerId: string, move: MoveInput, signal?: AbortSignal): Promise<GameDto> {
    return this.transport.requestEnveloped<GameDto>(`/games/${gameId}/moves`, {
      method: 'POST',
      body: move,
      headers: { [PLAYER_ID_HEADER]: playerId },
      signal,
    });
  }

  listEngines(signal?: AbortSignal): Promise<EngineCapabilityDto[]> {
    return this.transport.requestEnveloped<EngineCapabilityDto[]>('/engines', { signal });
  }

  async evaluate(request: EvalRequest, signal?: AbortSignal): Promise<number> {
    const result = await this.transport.requestEnveloped<EvalResponseDto>('/eval', {
      method: 'POST',
      body: request,
      signal,
    });

    return result.score;
  }

  getHealth(signal?: AbortSignal): Promise<HealthDto> {
    return this.transport.requestRaw<HealthDto>('/healthz', { signal });
  }

  getVersion(signal?: AbortSignal): Promise<VersionDto> {
    return this.transport.requestRaw<VersionDto>('/version', { signal });
  }
}
