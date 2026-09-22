import { Routes } from '@angular/router';
import { authGuard, guestGuard, menuGuard } from './core/auth/auth.guard';
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
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        canActivate: [menuGuard('dashboard')],
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/account/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'change-password',
        loadComponent: () =>
          import('./features/account/change-password.component').then(m => m.ChangePasswordComponent)
      },
      {
        path: 'my-forms',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () => import('./features/my-forms/my-forms.component').then(m => m.MyFormsComponent)
      },
      {
        path: 'my-forms/:formId',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-entries.component').then(m => m.MyFormEntriesComponent)
      },
      {
        path: 'my-forms/:formId/new',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId/followups/new',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId/followups/:followUpId/edit',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId/followups',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-followups.component').then(m => m.MyFormFollowupsComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId/edit',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-fill.component').then(m => m.MyFormFillComponent)
      },
      {
        path: 'my-forms/:formId/entries/:submissionId',
        canActivate: [menuGuard('my-forms')],
        loadComponent: () =>
          import('./features/my-forms/my-form-entry-detail.component').then(m => m.MyFormEntryDetailComponent)
      },
      {
        path: 'users',
        canActivate: [menuGuard('users')],
        loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'roles',
        canActivate: [menuGuard('roles')],
        loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'projects',
        canActivate: [menuGuard('projects')],
        loadComponent: () => import('./features/projects/projects.component').then(m => m.ProjectsComponent)
      },
      {
        path: 'locations/states',
        canActivate: [menuGuard('location')],
        loadComponent: () => import('./features/locations/states.component').then(m => m.StatesComponent)
      },
      {
        path: 'locations/districts',
        canActivate: [menuGuard('location')],
        loadComponent: () => import('./features/locations/districts.component').then(m => m.DistrictsComponent)
      },
      {
        path: 'locations/blocks',
        canActivate: [menuGuard('location')],
        loadComponent: () => import('./features/locations/blocks.component').then(m => m.BlocksComponent)
      },
      {
        path: 'locations/villages',
        canActivate: [menuGuard('location')],
        loadComponent: () => import('./features/locations/villages.component').then(m => m.VillagesComponent)
      },
      {
        path: 'forms',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/forms/forms.component').then(m => m.FormsComponent)
      },
      {
        path: 'forms/:id',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/forms/form-builder.component').then(m => m.FormBuilderComponent)
      },
      {
        path: 'form-groups',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/form-groups/form-groups.component').then(m => m.FormGroupsComponent)
      },
      {
        path: 'form-fields',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/form-fields/form-fields.component').then(m => m.FormFieldsComponent)
      },
      {
        path: 'form-fields/new',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/form-fields/form-field-form.component').then(m => m.FormFieldFormComponent)
      },
      {
        path: 'form-fields/:id/edit',
        canActivate: [menuGuard('forms')],
        loadComponent: () => import('./features/form-fields/form-field-form.component').then(m => m.FormFieldFormComponent)
      },
      {
        path: 'document-templates',
        canActivate: [menuGuard('templates')],
        loadComponent: () => import('./features/document-templates/document-templates.component').then(m => m.DocumentTemplatesComponent)
      },
      {
        path: 'document-templates/:id/edit',
        canActivate: [menuGuard('templates')],
        loadComponent: () => import('./features/document-templates/document-template-edit.component').then(m => m.DocumentTemplateEditComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
