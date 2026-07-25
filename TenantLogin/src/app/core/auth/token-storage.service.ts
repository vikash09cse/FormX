import { Injectable } from '@angular/core';
import { LoginResponse } from '../models/api.models';

const ACCESS_TOKEN_KEY = 'formx_tenant_access_token';
const REFRESH_TOKEN_KEY = 'formx_tenant_refresh_token';
const USER_KEY = 'formx_tenant_user';
const REMEMBER_KEY = 'formx_tenant_remember';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  saveSession(response: LoginResponse, rememberMe = false): void {
    this.clear();
    const storage = this.pickWriteStorage(rememberMe);
    if (!storage) return;

    if (rememberMe) {
      localStorage.setItem(REMEMBER_KEY, '1');
    }

    storage.setItem(ACCESS_TOKEN_KEY, response.token);
    storage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    storage.setItem(USER_KEY, JSON.stringify(response));
  }

  updateTokens(response: LoginResponse): void {
    const storage = this.activeStorage();
    if (!storage) return;

    storage.setItem(ACCESS_TOKEN_KEY, response.token);
    storage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    const user = this.getUser();
    if (user) {
      storage.setItem(
        USER_KEY,
        JSON.stringify({
          ...user,
          token: response.token,
          refreshToken: response.refreshToken,
          expiresIn: response.expiresIn
        })
      );
    }
  }

  getAccessToken(): string | null {
    return this.get(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return this.get(REFRESH_TOKEN_KEY);
  }

  getUser(): LoginResponse | null {
    const raw = this.get(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as LoginResponse;
    } catch {
      return null;
    }
  }

  clear(): void {
    for (const storage of this.availableStorages()) {
      storage.removeItem(ACCESS_TOKEN_KEY);
      storage.removeItem(REFRESH_TOKEN_KEY);
      storage.removeItem(USER_KEY);
    }
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(REMEMBER_KEY);
    }
  }

  private get(key: string): string | null {
    if (typeof localStorage !== 'undefined') {
      const fromLocal = localStorage.getItem(key);
      if (fromLocal) return fromLocal;
    }
    if (typeof sessionStorage !== 'undefined') {
      return sessionStorage.getItem(key);
    }
    return null;
  }

  private pickWriteStorage(rememberMe: boolean): Storage | null {
    if (rememberMe) {
      return typeof localStorage !== 'undefined' ? localStorage : null;
    }
    return typeof sessionStorage !== 'undefined' ? sessionStorage : null;
  }

  private activeStorage(): Storage | null {
    if (typeof localStorage !== 'undefined' && localStorage.getItem(ACCESS_TOKEN_KEY)) {
      return localStorage;
    }
    if (typeof sessionStorage !== 'undefined' && sessionStorage.getItem(ACCESS_TOKEN_KEY)) {
      return sessionStorage;
    }
    if (typeof localStorage !== 'undefined' && localStorage.getItem(REMEMBER_KEY) === '1') {
      return localStorage;
    }
    return typeof sessionStorage !== 'undefined' ? sessionStorage : null;
  }

  private availableStorages(): Storage[] {
    const list: Storage[] = [];
    if (typeof localStorage !== 'undefined') list.push(localStorage);
    if (typeof sessionStorage !== 'undefined') list.push(sessionStorage);
    return list;
  }
}
