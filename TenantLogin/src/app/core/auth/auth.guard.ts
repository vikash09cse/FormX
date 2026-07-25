import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** Default landing path by system role (Staff/Doctor → My Forms; Super Admin → Users). */
export function tenantHomePath(role: string | null | undefined): string {
  return role === 'TenantSuperAdmin' ? '/users' : '/my-forms';
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
    return router.createUrlTree([tenantHomePath(auth.currentUser()?.role)]);
  }

  // Avoid guest↔auth loops when a dead JWT is still stored.
  auth.discardInvalidSession();
  return true;
};

/** Empty-path redirect after auth shell loads (Super Admin → Users). */
export const homeRedirectGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return router.createUrlTree([tenantHomePath(auth.currentUser()?.role)]);
};

export const tenantSuperAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.currentUser()?.role;
  if (role === 'TenantSuperAdmin') return true;
  return router.createUrlTree(['/my-forms']);
};
