import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface RoleItem {
  id: string;
  name: string;
  dataScope: number;
  dataScopeLabel: string;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
  menus: string[];
  status: string;
  statusCode: number;
}

const MENU_OPTIONS: { key: string; label: string }[] = [
  { key: 'dashboard', label: 'Dashboard' },
  { key: 'my-forms', label: 'My forms' },
  { key: 'users', label: 'Users' },
  { key: 'roles', label: 'Roles' },
  { key: 'projects', label: 'Projects' },
  { key: 'forms', label: 'Forms' },
  { key: 'templates', label: 'Templates' },
  { key: 'location', label: 'Location' }
];

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly menuOptions = MENU_OPTIONS;
  readonly items = signal<RoleItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<RoleItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<RoleItem | null>(null);

  name = '';
  dataScope = 0;
  canCreate = true;
  canEdit = true;
  canDelete = true;
  selectedMenus = new Set<string>();
  statusCode = 1;

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const r = this.confirmTarget();
    return r ? `Delete role "${r.name}"?` : '';
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<RoleItem[]>>('/roles').subscribe({
      next: res => { this.items.set(res.data ?? []); this.loading.set(false); },
      error: err => { this.error.set(err.error?.message ?? 'Unable to load roles.'); this.loading.set(false); }
    });
  }

  menusLabel(item: RoleItem): string {
    if (!item.menus?.length) return '—';
    return item.menus
      .map(k => this.menuOptions.find(m => m.key === k)?.label ?? k)
      .join(', ');
  }

  permissionsLabel(item: RoleItem): string {
    const parts: string[] = [];
    if (item.canCreate) parts.push('Create');
    if (item.canEdit) parts.push('Edit');
    if (item.canDelete) parts.push('Delete');
    return parts.length ? parts.join(', ') : 'View only';
  }

  openCreate() {
    this.editing.set(null);
    this.name = '';
    this.dataScope = 0;
    this.canCreate = true;
    this.canEdit = true;
    this.canDelete = true;
    this.selectedMenus = new Set(['dashboard', 'my-forms']);
    this.statusCode = 1;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: RoleItem) {
    this.editing.set(item);
    this.name = item.name;
    this.dataScope = item.dataScope;
    this.canCreate = item.canCreate;
    this.canEdit = item.canEdit;
    this.canDelete = item.canDelete;
    this.selectedMenus = new Set(item.menus ?? []);
    this.statusCode = item.statusCode;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  toggleMenu(key: string, checked: boolean) {
    if (checked) this.selectedMenus.add(key);
    else this.selectedMenus.delete(key);
  }

  isMenuSelected(key: string): boolean {
    return this.selectedMenus.has(key);
  }

  save() {
    if (!this.name.trim()) { this.formError.set('Name is required.'); return; }
    this.saving.set(true);
    this.formError.set('');
    const body = {
      name: this.name.trim(),
      dataScope: this.dataScope,
      canCreate: this.canCreate,
      canEdit: this.canEdit,
      canDelete: this.canDelete,
      menus: [...this.selectedMenus],
      status: this.statusCode
    };
    const req = this.isEditing()
      ? this.api.put<ApiResult<RoleItem>>(`/roles/${this.editing()!.id}`, body)
      : this.api.post<ApiResult<RoleItem>>('/roles', body);
    req.subscribe({
      next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); },
      error: err => { this.formError.set(err.error?.message ?? 'Save failed.'); this.saving.set(false); }
    });
  }

  askDelete(item: RoleItem) { this.confirmTarget.set(item); }
  cancelDelete() { this.confirmTarget.set(null); }
  confirmDelete() {
    const item = this.confirmTarget();
    if (!item) return;
    this.api.delete<ApiResult<boolean>>(`/roles/${item.id}`).subscribe({
      next: () => { this.confirmTarget.set(null); this.load(); },
      error: err => { this.error.set(err.error?.message ?? 'Delete failed.'); this.confirmTarget.set(null); }
    });
  }
}
