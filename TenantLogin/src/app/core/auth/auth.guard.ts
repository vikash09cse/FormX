import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { LoginResponse } from '../models/api.models';
import { AuthService } from './auth.service';

function menuFallbackPath(user: LoginResponse): string {
  const menus = user.menus ?? [];
  if (menus.includes('dashboard')) return '/dashboard';
  if (menus.includes('my-forms')) return '/my-forms';
  return '/profile';
}

/** Default post-login landing path for the current user. */
export function tenantHomePath(user?: LoginResponse | null): string {
  if (!user) return '/dashboard';
  if (user.role === 'TenantSuperAdmin') return '/dashboard';
  return menuFallbackPath(user);
}

/**
 * Protects authenticated routes.
 * Never waits on a network refresh before painting — expired sessions go straight to login
 * so the UI does not sit on a blank router-outlet when the API is slow or unreachable.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) return true;

  // Stale/expired token in sessionStorage — clear immediately (do not block on refresh).
  auth.discardInvalidSession();
  return router.createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return router.createUrlTree([tenantHomePath(auth.currentUser())]);
  }

  // Avoid guest↔auth loops when a dead JWT is still stored.
  auth.discardInvalidSession();
  return true;
};

export const tenantSuperAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.currentUser()?.role;
  if (role === 'TenantSuperAdmin') return true;
  const user = auth.currentUser();
  return router.createUrlTree([user ? menuFallbackPath(user) : '/my-forms']);
};

/** Allow TenantSuperAdmin or staff whose role menus include the key. */
export function menuGuard(menuKey: string): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const user = auth.currentUser();
    if (!user) {
      auth.discardInvalidSession();
      return router.createUrlTree(['/login']);
    }
    if (user.role === 'TenantSuperAdmin') return true;
    if ((user.menus ?? []).includes(menuKey)) return true;
    return router.createUrlTree([menuFallbackPath(user)]);
  };
}
