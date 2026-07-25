import { ApiResult } from '../models/api.models';

export function unwrapApiResult<T>(result: ApiResult<T>): T {
  if (!result.success || result.data == null) {
    throw { error: result };
  }
  return result.data;
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  const body = (error as { error?: ApiResult<unknown> })?.error;
  if (!body) return fallback;
  return body.message || body.errors?.[0]?.error || fallback;
}
