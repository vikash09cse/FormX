import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { ApiService } from '../api/api.service';
import { unwrapApiResult } from '../api/api.util';
import { ApiResult, LoginResponse } from '../models/api.models';

const TOKEN_KEY = 'formx_token';
const USER_KEY = 'formx_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  readonly currentUser = signal<LoginResponse | null>(this.loadUser());

  login(email: string, password: string) {
    return this.api.post<ApiResult<LoginResponse>>('/auth/platform-login', { email, password }).pipe(
      map(unwrapApiResult),
      tap(res => this.persistSession(res))
    );
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private persistSession(res: LoginResponse) {
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, JSON.stringify(res));
    this.currentUser.set(res);
  }

  private loadUser(): LoginResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as LoginResponse;
    } catch {
      return null;
    }
  }
}

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly api = inject(ApiService);

  getDashboard() {
    return this.api.get<ApiResult<import('../models/api.models').PlatformDashboard>>('/platform/tenants/dashboard');
  }

  getTenants() {
    return this.api.get<ApiResult<import('../models/api.models').TenantSummary[]>>('/platform/tenants');
  }

  getTenant(id: string) {
    return this.api.get<ApiResult<import('../models/api.models').TenantDetail>>(`/platform/tenants/${id}`);
  }

  createTenant(body: import('../models/api.models').CreateTenantRequest) {
    return this.api.post<ApiResult<import('../models/api.models').TenantDetail>>('/platform/tenants', body);
  }

  updateTenant(id: string, body: import('../models/api.models').UpdateTenantRequest) {
    return this.api.put<ApiResult<import('../models/api.models').TenantDetail>>(`/platform/tenants/${id}`, body);
  }

  suspendTenant(id: string) {
    return this.api.patch<ApiResult<boolean>>(`/platform/tenants/${id}/suspend`);
  }

  reactivateTenant(id: string) {
    return this.api.patch<ApiResult<boolean>>(`/platform/tenants/${id}/reactivate`);
  }
}

@Injectable({ providedIn: 'root' })
export class PlatformUserService {
  private readonly api = inject(ApiService);

  getUsers(page = 1, pageSize = 10) {
    return this.api.get<ApiResult<import('../models/api.models').BackOfficeUserList>>(
      `/platform/backoffice-users?page=${page}&pageSize=${pageSize}`
    );
  }

  createUser(body: import('../models/api.models').CreateBackOfficeUserRequest) {
    return this.api.post<ApiResult<import('../models/api.models').BackOfficeUser>>('/platform/backoffice-users', body);
  }

  setStatus(id: string, status: number) {
    return this.api.patch<ApiResult<boolean>>(`/platform/backoffice-users/${id}/status?status=${status}`);
  }

  deleteUser(id: string) {
    return this.api.delete<ApiResult<boolean>>(`/platform/backoffice-users/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class DocumentTemplateService {
  private readonly api = inject(ApiService);

  getTemplates(templateType?: number) {
    const q = templateType != null ? `?templateType=${templateType}` : '';
    return this.api.get<ApiResult<import('../models/api.models').GlobalDocumentTemplate[]>>(
      `/platform/document-templates${q}`);
  }

  createTemplate(body: import('../models/api.models').SaveDocumentTemplateRequest) {
    return this.api.post<ApiResult<import('../models/api.models').GlobalDocumentTemplate>>(
      '/platform/document-templates', body);
  }

  updateTemplate(id: string, body: import('../models/api.models').SaveDocumentTemplateRequest) {
    return this.api.put<ApiResult<import('../models/api.models').GlobalDocumentTemplate>>(
      `/platform/document-templates/${id}`, body);
  }

  setDefault(id: string) {
    return this.api.patch<ApiResult<boolean>>(`/platform/document-templates/${id}/default`);
  }

  deleteTemplate(id: string) {
    return this.api.delete<ApiResult<boolean>>(`/platform/document-templates/${id}`);
  }
}
