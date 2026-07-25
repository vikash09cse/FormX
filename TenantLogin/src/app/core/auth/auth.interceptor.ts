import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

const AUTH_PATHS = ['/auth/tenant-login', '/auth/refresh-token', '/auth/logout'];

function isAuthPath(url: string): boolean {
  return AUTH_PATHS.some(path => url.includes(path));
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  let request = req;
  if (token && !isAuthPath(req.url)) {
    request = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthPath(req.url)) {
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        switchMap(res => next(req.clone({
          setHeaders: { Authorization: `Bearer ${res.token}` }
        }))),
        catchError(refreshError => {
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
