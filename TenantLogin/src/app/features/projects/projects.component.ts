import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface ProjectItem {
  id: string;
  projectName: string;
  code?: string | null;
  status: string;
  statusCode: number;
}

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})
export class ProjectsComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly items = signal<ProjectItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<ProjectItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<ProjectItem | null>(null);

  projectName = '';
  code = '';
  statusCode = 1;

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const p = this.confirmTarget();
    return p ? `Delete project "${p.projectName}"?` : '';
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<ProjectItem[]>>('/projects').subscribe({
      next: res => { this.items.set(res.data ?? []); this.loading.set(false); },
      error: err => { this.error.set(err.error?.message ?? 'Unable to load projects.'); this.loading.set(false); }
    });
  }

  openCreate() {
    this.editing.set(null);
    this.projectName = '';
    this.code = '';
    this.statusCode = 1;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: ProjectItem) {
    this.editing.set(item);
    this.projectName = item.projectName;
    this.code = item.code ?? '';
    this.statusCode = item.statusCode;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  save() {
    if (!this.projectName.trim()) { this.formError.set('Project name is required.'); return; }
    this.saving.set(true);
    this.formError.set('');
    const body = { projectName: this.projectName.trim(), code: this.code.trim() || null, status: this.statusCode };
    const req = this.isEditing()
      ? this.api.put<ApiResult<ProjectItem>>(`/projects/${this.editing()!.id}`, body)
      : this.api.post<ApiResult<ProjectItem>>('/projects', body);
    req.subscribe({
      next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); },
      error: err => { this.formError.set(err.error?.message ?? 'Save failed.'); this.saving.set(false); }
    });
  }

  askDelete(item: ProjectItem) { this.confirmTarget.set(item); }
  cancelDelete() { this.confirmTarget.set(null); }
  confirmDelete() {
    const item = this.confirmTarget();
    if (!item) return;
    this.api.delete<ApiResult<boolean>>(`/projects/${item.id}`).subscribe({
      next: () => { this.confirmTarget.set(null); this.load(); },
      error: err => { this.error.set(err.error?.message ?? 'Delete failed.'); this.confirmTarget.set(null); }
    });
  }
}
