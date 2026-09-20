import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface ListColumn {
  fieldId: string;
  label: string;
}

interface SubmissionItem {
  id: string;
  formId: string;
  projectId: string;
  projectName: string;
  submittedAt: string;
  status: number;
  values: Record<string, string | null | undefined>;
}

interface SubmissionDetail {
  id: string;
  formId: string;
  projectId: string;
  projectName: string;
  submittedAt: string;
  status: number;
  parentSubmissionId?: string | null;
  values: { fieldId: string; valueText?: string | null }[];
}

interface FormDefinitionField {
  id: string;
  controlLabel: string;
  controlType: number;
  displayOrder: number;
}

interface FormDefinition {
  formId: string;
  name: string;
  projectName?: string | null;
  groups: { fields: FormDefinitionField[] }[];
}

interface FollowupsPage {
  config?: {
    followUpFormId: string;
    followUpFormName?: string | null;
    allowMultiple: boolean;
  } | null;
  columns: ListColumn[];
  items: SubmissionItem[];
}

@Component({
  selector: 'app-my-form-entry-detail',
  standalone: true,
  imports: [RouterLink, ConfirmDialogComponent],
  templateUrl: './my-form-entry-detail.component.html',
  styleUrl: './my-form-entry-detail.component.scss'
})
export class MyFormEntryDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  readonly formId = signal('');
  readonly submissionId = signal('');
  readonly formName = signal('Form');
  readonly loading = signal(true);
  readonly error = signal('');
  readonly parent = signal<SubmissionDetail | null>(null);
  readonly parentFields = signal<FormDefinitionField[]>([]);
  readonly followups = signal<SubmissionItem[]>([]);
  readonly followupColumns = signal<ListColumn[]>([]);
  readonly followupConfig = signal<FollowupsPage['config']>(null);
  readonly confirmTarget = signal<SubmissionItem | null>(null);

  readonly canAddFollowup = computed(() => {
    const cfg = this.followupConfig();
    if (!cfg) return false;
    if (cfg.allowMultiple) return true;
    return this.followups().length === 0;
  });

  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete follow-up from ${this.formatDate(item.submittedAt)}?` : '';
  });

  ngOnInit() {
    this.formId.set(this.route.snapshot.paramMap.get('formId') ?? '');
    this.submissionId.set(this.route.snapshot.paramMap.get('submissionId') ?? '');
    this.reload();
  }

  reload() {
    this.loading.set(true);
    this.error.set('');
    const formId = this.formId();
    const submissionId = this.submissionId();

    this.api.get<ApiResult<FormDefinition>>(`/submissions/forms/${formId}/definition`).subscribe({
      next: res => {
        this.formName.set(res.data?.name ?? 'Form');
        const fields = (res.data?.groups ?? [])
          .flatMap(g => g.fields)
          .filter(f => f.controlType !== 7)
          .sort((a, b) => a.displayOrder - b.displayOrder);
        this.parentFields.set(fields);
      }
    });

    this.api.get<ApiResult<SubmissionDetail>>(`/submissions/${submissionId}`).subscribe({
      next: res => {
        if (!res.data || res.data.parentSubmissionId) {
          this.error.set('Entry not found.');
          this.loading.set(false);
          return;
        }
        this.parent.set(res.data);
        this.loadFollowups(submissionId);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load entry.');
        this.loading.set(false);
      }
    });
  }

  private loadFollowups(parentId: string) {
    this.api.get<ApiResult<FollowupsPage>>(`/submissions/${parentId}/followups`).subscribe({
      next: res => {
        this.followupConfig.set(res.data?.config ?? null);
        this.followupColumns.set(res.data?.columns ?? []);
        this.followups.set(res.data?.items ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load follow-ups.');
        this.loading.set(false);
      }
    });
  }

  parentValue(fieldId: string): string {
    const v = this.parent()?.values?.find(x => x.fieldId === fieldId);
    return v?.valueText?.trim() || '—';
  }

  cellValue(item: SubmissionItem, fieldId: string): string {
    const v = item.values?.[fieldId];
    return (v ?? '').toString().trim() || '—';
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return iso;
    return d.toLocaleString();
  }

  askDelete(item: SubmissionItem) {
    this.confirmTarget.set(item);
  }

  cancelDelete() {
    this.confirmTarget.set(null);
  }

  confirmDelete() {
    const item = this.confirmTarget();
    if (!item) return;
    this.api
      .delete<ApiResult<boolean>>(`/submissions/${this.submissionId()}/followups/${item.id}`)
      .subscribe({
        next: () => {
          this.confirmTarget.set(null);
          this.loadFollowups(this.submissionId());
        },
        error: err => {
          this.error.set(err.error?.message ?? 'Delete failed.');
          this.confirmTarget.set(null);
        }
      });
  }
}
