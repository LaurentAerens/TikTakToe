import { ApiError } from './ApiError';
import type { ApiResponse } from './types';

/**
 * Defaults to the Vite proxy prefix, which forwards to the backend without CORS.
 * Set VITE_API_BASE_URL to point at an already-CORS-enabled backend instead.
 */
export const DEFAULT_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? '/api';

export type FetchLike = typeof fetch;

export interface HttpTransportOptions {
  baseUrl?: string;
  /** Injectable for tests; defaults to the global fetch. */
  fetchImpl?: FetchLike;
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  headers?: Record<string, string>;
  signal?: AbortSignal;
}

/** Parses a body that may legitimately be empty, without throwing. */
function tryParseJson(text: string): unknown {
  if (text.length === 0) {
    return undefined;
  }

  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}

/**
 * Route-constraint misses and malformed bodies produce a 4xx with an empty body,
 * so the envelope's `error` is only sometimes available.
 */
function errorMessageFor(parsed: unknown, status: number, statusText: string): string {
  if (typeof parsed === 'object' && parsed !== null) {
    const envelopeError = (parsed as ApiResponse<unknown>).error;
    if (typeof envelopeError === 'string' && envelopeError.length > 0) {
      return envelopeError;
    }
  }

  return statusText.length > 0
    ? `The API responded ${status} ${statusText}.`
    : `The API responded ${status}.`;
}

/** Thin wrapper over fetch that understands the backend's response envelope. */
export class HttpTransport {
  private readonly baseUrl: string;

  private readonly fetchImpl: FetchLike;

  constructor(options: HttpTransportOptions = {}) {
    this.baseUrl = (options.baseUrl ?? DEFAULT_BASE_URL).replace(/\/+$/, '');
    this.fetchImpl = options.fetchImpl ?? globalThis.fetch.bind(globalThis);
  }

  /** Sends a request and returns the parsed body as-is. Use for unenveloped endpoints. */
  async requestRaw<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const { value } = await this.send<T>(path, options);
    return value;
  }

  /** Sends a request and unwraps the `{ success, data, error }` envelope. */
  async requestEnveloped<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const { value: envelope, status } = await this.send<ApiResponse<T>>(path, options);

    if (!envelope.success || envelope.data === null || envelope.data === undefined) {
      throw new ApiError(envelope.error ?? `The API reported a failure for ${path}.`, status);
    }

    return envelope.data;
  }

  private async send<T>(path: string, options: RequestOptions): Promise<{ value: T; status: number }> {
    const { method = 'GET', body, headers = {}, signal } = options;
    const hasBody = body !== undefined;

    const response = await this.fetchImpl(`${this.baseUrl}${path}`, {
      method,
      signal,
      headers: hasBody ? { 'Content-Type': 'application/json', ...headers } : headers,
      body: hasBody ? JSON.stringify(body) : undefined,
    });

    const text = await response.text();
    const parsed = tryParseJson(text);

    if (!response.ok) {
      throw new ApiError(errorMessageFor(parsed, response.status, response.statusText), response.status, text);
    }

    if (parsed === undefined) {
      throw new ApiError(`The API returned an unreadable body for ${method} ${path}.`, response.status, text);
    }

    return { value: parsed as T, status: response.status };
  }
}
