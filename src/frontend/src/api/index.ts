import { HttpGameApiClient } from './HttpGameApiClient';
import type { IGameApiClient } from './IGameApiClient';

export { ApiError } from './ApiError';
export { HttpGameApiClient } from './HttpGameApiClient';
export { DEFAULT_BASE_URL, HttpTransport } from './http';
export type { FetchLike, HttpTransportOptions } from './http';
export type { IGameApiClient } from './IGameApiClient';
export * from './types';

/** Shared client used by the hooks. Typed as the interface so call sites stay swappable. */
export const gameApi: IGameApiClient = new HttpGameApiClient();
