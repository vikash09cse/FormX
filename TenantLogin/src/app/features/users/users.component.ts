import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface TenantUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  designation?: string | null;
  role: string;
  roleCode: number;
  status: string;
  statusCode: number;
  password?: string | null;
  roleIds: string[];
  projectScopeIds: string[];
  districtScopeIds: string[];
}

interface UserList {
  items: TenantUser[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface RoleItem {
  id: string;
  name: string;
}

interface ProjectItem {
  id: string;
  projectName: string;
}

interface DistrictItem {
  id: string;
  name: string;
  stateName?: string | null;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly users = signal<TenantUser[]>([]);
  readonly roles = signal<RoleItem[]>([]);
  readonly projects = signal<ProjectItem[]>([]);
  readonly districts = signal<DistrictItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editingUser = signal<TenantUser | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly deletingId = signal<string | null>(null);
  readonly confirmTarget = signal<TenantUser | null>(null);
  readonly currentUserId = signal<string | null>(null);

  firstName = '';
  lastName = '';
  email = '';
  designation = '';
  roleCode = 2;
  temporaryPassword = '';
  selectedRoleIds = new Set<string>();
  selectedProjectScopeIds = new Set<string>();
  selectedDistrictScopeIds = new Set<string>();

  readonly isEditing = computed(() => this.editingUser() !== null);

  readonly confirmMessage = computed(() => {
    const user = this.confirmTarget();
    if (!user) return '';
    const name = `${user.firstName} ${user.lastName}`.trim();
    return `${name} (${user.email}) will be removed from the Users list. The account is soft-deleted and cannot log in.`;
  });

  ngOnInit() {
    this.currentUserId.set(this.auth.currentUser()?.userId ?? null);
    this.loadRoles();
    this.loadProjects();
    this.loadDistricts();
    this.loadUsers();
  }

  canManageUser(user: TenantUser): boolean {
    if (user.id === this.currentUserId()) return false;
    return user.roleCode === 2; // Staff (system JWT role)
  }

  roleNames(user: TenantUser): string {
    const names = (user.roleIds ?? [])
      .map(id => this.roles().find(r => r.id === id)?.name)
      .filter((n): n is string => !!n);
    return names.length ? names.join(', ') : '—';
  }

  projectNames(user: TenantUser): string {
    const names = (user.projectScopeIds ?? [])
      .map(id => this.projects().find(p => p.id === id)?.projectName)
      .filter((n): n is string => !!n);
    return names.length ? names.join(', ') : '—';
  }

  districtNames(user: TenantUser): string {
    const names = (user.districtScopeIds ?? [])
      .map(id => this.districts().find(d => d.id === id)?.name)
      .filter((n): n is string => !!n);
    return names.length ? names.join(', ') : '—';
  }

  loadRoles() {
    this.api.get<ApiResult<RoleItem[]>>('/roles').subscribe({
      next: res => this.roles.set((res.data ?? []).map(r => ({ id: r.id, name: r.name })))
    });
  }

  loadProjects() {
    this.api.get<ApiResult<ProjectItem[]>>('/projects').subscribe({
      next: res => this.projects.set((res.data ?? []).map(p => ({ id: p.id, projectName: p.projectName })))
    });
  }

  loadDistricts() {
    this.api.get<ApiResult<{ id: string; name: string; stateName?: string | null }[]>>('/locations/districts').subscribe({
      next: res =>
        this.districts.set(
          (res.data ?? []).map(d => ({ id: d.id, name: d.name, stateName: d.stateName }))
        )
    });
  }

  loadUsers() {
    this.loading.set(true);
    this.error.set('');

    this.api.get<ApiResult<UserList>>('/users?page=1&pageSize=50').subscribe({
      next: res => {
        this.users.set(res.data?.items ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load users.');
        this.loading.set(false);
      }
    });
  }

  openDrawer() {
    this.resetForm();
    this.editingUser.set(null);
    this.drawerOpen.set(true);
  }

  openEdit(user: TenantUser) {
    if (!this.canManageUser(user)) return;
    this.editingUser.set(user);
    this.firstName = user.firstName;
    this.lastName = user.lastName;
    this.email = user.email;
    this.designation = user.designation ?? '';
    this.roleCode = user.roleCode;
    this.temporaryPassword = '';
    this.selectedRoleIds = new Set(user.roleIds ?? []);
    this.selectedProjectScopeIds = new Set(user.projectScopeIds ?? []);
    this.selectedDistrictScopeIds = new Set(user.districtScopeIds ?? []);
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
    this.editingUser.set(null);
    this.formError.set('');
  }

  resetForm() {
    this.firstName = '';
    this.lastName = '';
    this.email = '';
    this.designation = '';
    this.roleCode = 2;
    this.temporaryPassword = '';
    this.selectedRoleIds = new Set();
    this.selectedProjectScopeIds = new Set();
    this.selectedDistrictScopeIds = new Set();
    this.formError.set('');
  }

  toggleRole(id: string, checked: boolean) {
    if (checked) this.selectedRoleIds.add(id);
    else this.selectedRoleIds.delete(id);
  }

  isRoleSelected(id: string) {
    return this.selectedRoleIds.has(id);
  }

  toggleProject(id: string, checked: boolean) {
    if (checked) this.selectedProjectScopeIds.add(id);
    else this.selectedProjectScopeIds.delete(id);
  }

  isProjectSelected(id: string) {
    return this.selectedProjectScopeIds.has(id);
  }

  toggleDistrict(id: string, checked: boolean) {
    if (checked) this.selectedDistrictScopeIds.add(id);
    else this.selectedDistrictScopeIds.delete(id);
  }

  isDistrictSelected(id: string) {
    return this.selectedDistrictScopeIds.has(id);
  }

  districtLabel(d: DistrictItem): string {
    return d.stateName ? `${d.name} (${d.stateName})` : d.name;
  }

  submitUser() {
    if (!this.firstName.trim() || !this.lastName.trim()) {
      this.formError.set('First name and last name are required.');
      return;
    }

    if (this.isEditing()) {
      this.saveEdit();
      return;
    }

    if (!this.email.trim() || !this.email.includes('@')) {
      this.formError.set('A valid email is required.');
      return;
    }

    if (!this.temporaryPassword.trim() || this.temporaryPassword.trim().length < 6) {
      this.formError.set('Password is required (minimum 6 characters).');
      return;
    }

    this.saving.set(true);
    this.formError.set('');

    const body: Record<string, unknown> = {
      email: this.email.trim(),
      firstName: this.firstName.trim(),
      lastName: this.lastName.trim(),
      role: this.roleCode,
      temporaryPassword: this.temporaryPassword.trim(),
      roleIds: [...this.selectedRoleIds],
      projectScopeIds: [...this.selectedProjectScopeIds],
      districtScopeIds: [...this.selectedDistrictScopeIds]
    };
    if (this.designation.trim()) {
      body['designation'] = this.designation.trim();
    }

    this.api.post<ApiResult<TenantUser>>('/users', body).subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.resetForm();
        this.loadUsers();
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Unable to add user.');
        this.saving.set(false);
      }
    });
  }

  private saveEdit() {
    const user = this.editingUser();
    if (!user) return;

    const body: Record<string, unknown> = {
      firstName: this.firstName.trim(),
      lastName: this.lastName.trim(),
      role: this.roleCode,
      designation: this.designation.trim() || null,
      roleIds: [...this.selectedRoleIds],
      projectScopeIds: [...this.selectedProjectScopeIds],
      districtScopeIds: [...this.selectedDistrictScopeIds]
    };

    const newPassword = this.temporaryPassword.trim();
    if (newPassword) {
      if (newPassword.length < 6) {
        this.formError.set('Password must be at least 6 characters.');
        return;
      }
      body['temporaryPassword'] = newPassword;
    }

    this.saving.set(true);
    this.formError.set('');

    this.api.put<ApiResult<TenantUser>>(`/users/${user.id}`, body).subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.editingUser.set(null);
        this.resetForm();
        this.loadUsers();
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Unable to update user.');
        this.saving.set(false);
      }
    });
  }

  deleteUser(user: TenantUser) {
    if (!this.canManageUser(user)) return;
    this.confirmTarget.set(user);
  }

  cancelDelete() {
    if (!this.deletingId()) {
      this.confirmTarget.set(null);
    }
  }

  confirmDelete() {
    const user = this.confirmTarget();
    if (!user) return;

    this.deletingId.set(user.id);
    this.error.set('');

    this.api.delete<ApiResult<boolean>>(`/users/${user.id}`).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.confirmTarget.set(null);
        this.loadUsers();
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to delete user.');
        this.deletingId.set(null);
      }
    });
  }
}
