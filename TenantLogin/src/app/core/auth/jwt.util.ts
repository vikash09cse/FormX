export interface JwtPayload {
  exp?: number;
  tenant_id?: string;
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const json = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}

export function isJwtExpired(token: string, bufferSeconds = 60): boolean {
  const payload = decodeJwtPayload(token);
  if (!payload?.exp) return true;
  const expiresAtMs = payload.exp * 1000;
  return Date.now() >= expiresAtMs - bufferSeconds * 1000;
}
