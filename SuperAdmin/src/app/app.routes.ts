import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';
import { LayoutComponent } from './shared/layout/layout.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent), canActivate: [guestGuard] },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'tenants', loadComponent: () => import('./features/tenants/tenants.component').then(m => m.TenantsComponent) },
      { path: 'platform-users', loadComponent: () => import('./features/platform-users/platform-users.component').then(m => m.PlatformUsersComponent) },
      { path: 'document-templates', loadComponent: () => import('./features/document-templates/document-templates.component').then(m => m.DocumentTemplatesComponent) },
      { path: '', redirectTo: 'tenants', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'tenants' }
];
