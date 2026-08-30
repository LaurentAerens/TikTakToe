/** Raised for any non-2xx response, or a 2xx whose envelope reports `success: false`. */
export class ApiError extends Error {
  readonly status: number;

  /** Raw response body, kept for diagnosis when it was not valid JSON. */
  readonly body: string;

  constructor(message: string, status: number, body = '') {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.body = body;
  }
}
