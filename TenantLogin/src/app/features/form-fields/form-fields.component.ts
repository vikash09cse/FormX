import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface FormItem {
  id: string;
  name: string;
}

interface FormGroupItem {
  id: string;
  groupName: string;
}

interface FormField {
  id: string;
  formGroupId: string;
  controlLabel: string;
  controlTypeName: string;
  controlRequired: boolean;
  displayOrder: number;
  parentFieldId?: string | null;
}

@Component({
  selector: 'app-form-fields',
  standalone: true,
  imports: [FormsModule, RouterLink, ConfirmDialogComponent],
  templateUrl: './form-fields.component.html',
  styleUrl: './form-fields.component.scss'
})
export class FormFieldsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  readonly forms = signal<FormItem[]>([]);
  readonly groups = signal<FormGroupItem[]>([]);
  readonly fields = signal<FormField[]>([]);
  readonly loading = signal(true);
  readonly fieldsLoading = signal(false);
  readonly error = signal('');
  readonly confirmTarget = signal<FormField | null>(null);

  selectedFormId = '';

  readonly confirmMessage = computed(() => {
    const f = this.confirmTarget();
    return f ? `Delete field "${f.controlLabel}"?` : '';
  });

  ngOnInit() {
    this.loadForms();
  }

  groupName(groupId: string): string {
    return this.groups().find(g => g.id === groupId)?.groupName ?? '—';
  }

  loadForms() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<FormItem[]>>('/forms').subscribe({
      next: res => {
        const forms = (res.data ?? []).map(f => ({ id: f.id, name: f.name }));
        this.forms.set(forms);
        this.loading.set(false);
        const queryFormId = this.route.snapshot.queryParamMap.get('formId');
        const initial = forms.find(f => f.id === queryFormId)?.id ?? forms[0]?.id ?? '';
        if (initial) {
          this.selectedFormId = initial;
          this.loadFormData();
        }
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load forms.');
        this.loading.set(false);
      }
    });
  }

  onFormChange() {
    this.loadFormData();
  }

  loadFormData() {
    if (!this.selectedFormId) {
      this.groups.set([]);
      this.fields.set([]);
      return;
    }
    this.fieldsLoading.set(true);
    this.error.set('');
    this.api.get<ApiResult<FormGroupItem[]>>(`/forms/${this.selectedFormId}/groups`).subscribe({
      next: res => this.groups.set(res.data ?? [])
    });
    this.api.get<ApiResult<FormField[]>>(`/forms/${this.selectedFormId}/fields`).subscribe({
      next: res => {
        this.fields.set(res.data ?? []);
        this.fieldsLoading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load fields.');
        this.fieldsLoading.set(false);
      }
    });
  }

  askDelete(item: FormField) {
    this.confirmTarget.set(item);
  }

  cancelDelete() {
    this.confirmTarget.set(null);
  }

  confirmDelete() {
    const item = this.confirmTarget();
    if (!item || !this.selectedFormId) return;
    this.api.delete<ApiResult<boolean>>(`/forms/${this.selectedFormId}/fields/${item.id}`).subscribe({
      next: () => {
        this.confirmTarget.set(null);
        this.loadFormData();
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Delete failed.');
        this.confirmTarget.set(null);
      }
    });
  }
}
