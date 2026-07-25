import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface FormItem {
  id: string;
  name: string;
}

interface FormGroupItem {
  id: string;
  formId: string;
  groupName: string;
  groupDisplayNameKey?: string | null;
  displayOrder: number;
}

@Component({
  selector: 'app-form-groups',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './form-groups.component.html',
  styleUrl: './form-groups.component.scss'
})
export class FormGroupsComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly forms = signal<FormItem[]>([]);
  readonly items = signal<FormGroupItem[]>([]);
  readonly loading = signal(true);
  readonly groupsLoading = signal(false);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<FormGroupItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<FormGroupItem | null>(null);

  selectedFormId = '';
  groupName = '';
  groupDisplayNameKey = '';
  displayOrder = 0;

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const g = this.confirmTarget();
    return g ? `Delete form group "${g.groupName}"?` : '';
  });

  ngOnInit() {
    this.loadForms();
  }

  loadForms() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<FormItem[]>>('/forms').subscribe({
      next: res => {
        const forms = (res.data ?? []).map(f => ({ id: f.id, name: f.name }));
        this.forms.set(forms);
        this.loading.set(false);
        if (forms.length > 0) {
          this.selectedFormId = forms[0].id;
          this.loadGroups();
        }
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load forms.');
        this.loading.set(false);
      }
    });
  }

  onFormChange() {
    this.loadGroups();
  }

  loadGroups() {
    if (!this.selectedFormId) {
      this.items.set([]);
      return;
    }
    this.groupsLoading.set(true);
    this.error.set('');
    this.api.get<ApiResult<FormGroupItem[]>>(`/forms/${this.selectedFormId}/groups`).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.groupsLoading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load form groups.');
        this.groupsLoading.set(false);
      }
    });
  }

  openCreate() {
    if (!this.selectedFormId) return;
    this.editing.set(null);
    this.groupName = '';
    this.groupDisplayNameKey = '';
    this.displayOrder = 0;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: FormGroupItem) {
    this.editing.set(item);
    this.groupName = item.groupName;
    this.groupDisplayNameKey = item.groupDisplayNameKey ?? '';
    this.displayOrder = item.displayOrder;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  save() {
    if (!this.groupName.trim()) {
      this.formError.set('Group name is required.');
      return;
    }
    if (!this.selectedFormId) {
      this.formError.set('Select a form first.');
      return;
    }

    this.saving.set(true);
    this.formError.set('');
    const body = {
      groupName: this.groupName.trim(),
      groupDisplayNameKey: this.groupDisplayNameKey.trim() || null,
      displayOrder: this.displayOrder
    };
    const editing = this.editing();
    const req = editing
      ? this.api.put<ApiResult<FormGroupItem>>(`/forms/${this.selectedFormId}/groups/${editing.id}`, body)
      : this.api.post<ApiResult<FormGroupItem>>(`/forms/${this.selectedFormId}/groups`, body);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.loadGroups();
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }

  askDelete(item: FormGroupItem) {
    this.confirmTarget.set(item);
  }

  cancelDelete() {
    this.confirmTarget.set(null);
  }

  confirmDelete() {
    const item = this.confirmTarget();
    if (!item || !this.selectedFormId) return;
    this.api.delete<ApiResult<boolean>>(`/forms/${this.selectedFormId}/groups/${item.id}`).subscribe({
      next: () => {
        this.confirmTarget.set(null);
        this.loadGroups();
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Delete failed.');
        this.confirmTarget.set(null);
      }
    });
  }
}
