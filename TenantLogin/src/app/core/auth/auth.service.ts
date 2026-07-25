import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, map, of, shareReplay, tap, throwError, timeout } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResult, LoginResponse } from '../models/api.models';
import { unwrapApiResult } from '../api/api.util';
import { isJwtExpired } from './jwt.util';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokens = inject(TokenStorageService);

  private refreshInFlight: Observable<LoginResponse> | null = null;

  readonly currentUser = signal<LoginResponse | null>(this.tokens.getUser());

  login(email: string, password: string, rememberMe = false): Observable<LoginResponse> {
    return this.http.post<ApiResult<LoginResponse>>(`${environment.apiUrl}/auth/tenant-login`, { email, password }).pipe(
      timeout({ first: 20_000 }),
      map(unwrapApiResult),
      tap(res => this.persistSession(res, rememberMe))
    );
  }

  refreshAccessToken(): Observable<LoginResponse> {
    const token = this.getToken();
    if (!token) {
      return throwError(() => new Error('No access token'));
    }

    if (!this.refreshInFlight) {
      this.refreshInFlight = this.http.post<ApiResult<LoginResponse>>(
        `${environment.apiUrl}/auth/refresh-token`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
      ).pipe(
        timeout({ first: 10_000 }),
        map(unwrapApiResult),
        tap(res => {
          this.tokens.updateTokens(res);
          this.currentUser.set(this.tokens.getUser());
        }),
        catchError(err => {
          this.clearSession();
          return throwError(() => err);
        }),
        finalize(() => {
          this.refreshInFlight = null;
        }),
        shareReplay(1)
      );
    }

    return this.refreshInFlight;
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${environment.apiUrl}/auth/logout`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      }).pipe(catchError(() => of(null))).subscribe();
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.tokens.getAccessToken();
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !isJwtExpired(token);
  }

  isTokenExpiringSoon(): boolean {
    const token = this.getToken();
    return !!token && isJwtExpired(token, 300);
  }

  /**
   * Clears a stored session when the access token is missing or expired.
   * Used by guards so navigation never waits on a hung refresh call.
   */
  discardInvalidSession(): void {
    const token = this.getToken();
    if (!token || isJwtExpired(token)) {
      this.clearSession();
    }
  }

  ensureValidToken(): Observable<boolean> {
    const token = this.getToken();
    if (!token) return of(false);
    if (!isJwtExpired(token)) return of(true);

    // Access JWT is already expired — refresh endpoint also requires a valid JWT today,
    // so do not round-trip the API (that only delays showing login).
    this.clearSession();
    return of(false);
  }

  private persistSession(res: LoginResponse, rememberMe = false): void {
    this.tokens.saveSession(res, rememberMe);
    this.currentUser.set(res);
  }

  private clearSession(): void {
    this.tokens.clear();
    this.currentUser.set(null);
  }
}
