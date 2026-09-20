import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface RoleItem { id: string; name: string; }
interface ProjectItem { id: string; projectName: string; }
interface FormItem {
  id: string;
  name: string;
  description?: string | null;
  projectId?: string | null;
  projectName?: string | null;
  status: string;
  statusCode: number;
  displayOrder: number;
  roleIds: string[];
  collectLocation?: boolean;
}

interface FollowupConfig {
  id?: string | null;
  primaryFormId: string;
  primaryFormName?: string | null;
  followUpFormId?: string | null;
  followUpFormName?: string | null;
  allowMultiple: boolean;
  usedAsFollowUpForFormId?: string | null;
  usedAsFollowUpForFormName?: string | null;
}

@Component({
  selector: 'app-forms',
  standalone: true,
  imports: [FormsModule, RouterLink, ConfirmDialogComponent],
  templateUrl: './forms.component.html',
  styleUrl: './forms.component.scss'
})
export class FormsComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly items = signal<FormItem[]>([]);
  readonly roles = signal<RoleItem[]>([]);
  readonly projects = signal<ProjectItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<FormItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<FormItem | null>(null);
  readonly followupConfig = signal<FollowupConfig | null>(null);
  readonly followupOptions = signal<FormItem[]>([]);

  name = '';
  description = '';
  projectId = '';
  statusCode = 1;
  displayOrder = 0;
  selectedRoleIds = new Set<string>();
  followUpFormId = '';
  allowMultiple = true;
  collectLocation = true;

  deleteMessage(): string {
    const item = this.confirmTarget();
    return item ? `Delete form "${item.name}"?` : '';
  }

  ngOnInit() {
    this.loadRoles();
    this.loadProjects();
    this.load();
  }

  loadRoles() {
    this.api.get<ApiResult<RoleItem[]>>('/roles').subscribe({
      next: res => this.roles.set((res.data ?? []).map(r => ({ id: r.id, name: r.name })))
    });
  }

  loadProjects() {
    this.api.get<ApiResult<(ProjectItem & { statusCode?: number })[]>>('/projects').subscribe({
      next: res => this.projects.set(
        (res.data ?? [])
          .filter(p => p.statusCode !== 2)
          .map(p => ({ id: p.id, projectName: p.projectName }))
      )
    });
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<FormItem[]>>('/forms').subscribe({
      next: res => { this.items.set(res.data ?? []); this.loading.set(false); },
      error: err => { this.error.set(err.error?.message ?? 'Unable to load forms.'); this.loading.set(false); }
    });
  }

  openCreate() {
    this.editing.set(null);
    this.name = '';
    this.description = '';
    this.projectId = this.projects().length === 1 ? this.projects()[0].id : '';
    this.statusCode = 1;
    this.displayOrder = 0;
    this.selectedRoleIds = new Set();
    this.followUpFormId = '';
    this.allowMultiple = true;
    this.collectLocation = true;
    this.followupConfig.set(null);
    this.followupOptions.set([]);
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: FormItem) {
    this.editing.set(item);
    this.name = item.name;
    this.description = item.description ?? '';
    this.projectId = item.projectId ?? '';
    this.statusCode = item.statusCode;
    this.displayOrder = item.displayOrder;
    this.selectedRoleIds = new Set(item.roleIds ?? []);
    this.collectLocation = item.collectLocation ?? false;
    this.followUpFormId = '';
    this.allowMultiple = true;
    this.followupConfig.set(null);
    this.followupOptions.set([]);
    this.formError.set('');
    this.drawerOpen.set(true);
    this.loadFollowupConfig(item.id);
  }

  private loadFollowupConfig(formId: string) {
    this.api.get<ApiResult<FollowupConfig>>(`/forms/${formId}/followup-config`).subscribe({
      next: res => {
        const cfg = res.data ?? null;
        this.followupConfig.set(cfg);
        this.followUpFormId = cfg?.followUpFormId ?? '';
        this.allowMultiple = cfg?.allowMultiple ?? true;
        this.refreshFollowupOptions(formId, cfg?.followUpFormId ?? null);
      },
      error: () => {
        this.followupConfig.set(null);
        this.refreshFollowupOptions(formId, null);
      }
    });
  }

  private refreshFollowupOptions(primaryFormId: string, _currentFollowUpId: string | null) {
    this.followupOptions.set(
      this.items().filter(f => f.id !== primaryFormId && f.statusCode === 1)
    );
  }

  toggleRole(id: string, checked: boolean) {
    if (checked) this.selectedRoleIds.add(id);
    else this.selectedRoleIds.delete(id);
  }

  isRoleSelected(id: string) { return this.selectedRoleIds.has(id); }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  save() {
    if (!this.name.trim()) { this.formError.set('Name is required.'); return; }
    if (!this.projectId) { this.formError.set('Project is required.'); return; }
    this.saving.set(true);
    const body = {
      name: this.name.trim(),
      description: this.description.trim() || null,
      projectId: this.projectId,
      status: this.statusCode,
      displayOrder: this.displayOrder,
      roleIds: [...this.selectedRoleIds],
      collectLocation: this.collectLocation
    };
    const editing = this.editing();
    const req = editing
      ? this.api.put<ApiResult<FormItem>>(`/forms/${editing.id}`, body)
      : this.api.post<ApiResult<FormItem>>('/forms', body);
    req.subscribe({
      next: res => {
        const saved = res.data;
        if (editing && saved) {
          this.saveFollowupConfig(saved.id);
          return;
        }
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
      },
      error: err => { this.formError.set(err.error?.message ?? 'Save failed.'); this.saving.set(false); }
    });
  }

  private saveFollowupConfig(formId: string) {
    const usedAs = this.followupConfig()?.usedAsFollowUpForFormId;
    if (usedAs) {
      this.saving.set(false);
      this.drawerOpen.set(false);
      this.load();
      return;
    }

    this.api.put<ApiResult<FollowupConfig>>(`/forms/${formId}/followup-config`, {
      followUpFormId: this.followUpFormId || null,
      allowMultiple: this.allowMultiple
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Form saved, but follow-up config failed.');
        this.saving.set(false);
      }
    });
  }

  askDelete(item: FormItem) { this.confirmTarget.set(item); }
  cancelDelete() { this.confirmTarget.set(null); }
  confirmDelete() {
    const item = this.confirmTarget();
    if (!item) return;
    this.api.delete<ApiResult<boolean>>(`/forms/${item.id}`).subscribe({
      next: () => { this.confirmTarget.set(null); this.load(); },
      error: err => { this.error.set(err.error?.message ?? 'Delete failed.'); this.confirmTarget.set(null); }
    });
  }
}
