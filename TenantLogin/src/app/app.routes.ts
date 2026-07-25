import { Routes } from '@angular/router';
import { authGuard, guestGuard, homeRedirectGuard, tenantSuperAdminGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard]
  },
  {
    path: '',
    loadComponent: () => import('./shared/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        canActivate: [homeRedirectGuard],
        loadComponent: () => import('./features/my-forms/my-forms.component').then(m => m.MyFormsComponent)
      },
      {
        path: 'my-forms',
        loadComponent: () => import('./features/my-forms/my-forms.component').then(m => m.MyFormsComponent)
      },
      {
        path: 'my-forms/:formId',
        loadComponent: () =>
          import('./features/my-forms/my-form-entries.component').then(m => m.MyFormEntriesComponent)
      },
      {
        path: 'my-forms/:formId/new',
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId/edit',
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'users',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'roles',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'projects',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/projects/projects.component').then(m => m.ProjectsComponent)
      },
      {
        path: 'forms',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/forms/forms.component').then(m => m.FormsComponent)
      },
      {
        path: 'forms/:id',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/forms/form-builder.component').then(m => m.FormBuilderComponent)
      },
      {
        path: 'form-groups',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/form-groups/form-groups.component').then(m => m.FormGroupsComponent)
      },
      {
        path: 'form-fields',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/form-fields/form-fields.component').then(m => m.FormFieldsComponent)
      },
      {
        path: 'form-fields/new',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/form-fields/form-field-form.component').then(m => m.FormFieldFormComponent)
      },
      {
        path: 'form-fields/:id/edit',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/form-fields/form-field-form.component').then(m => m.FormFieldFormComponent)
      },
      {
        path: 'document-templates',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/document-templates/document-templates.component').then(m => m.DocumentTemplatesComponent)
      },
      {
        path: 'document-templates/:id/edit',
        canActivate: [tenantSuperAdminGuard],
        loadComponent: () => import('./features/document-templates/document-template-edit.component').then(m => m.DocumentTemplateEditComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
