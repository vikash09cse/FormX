import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';
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
  selector: 'app-my-form-followups',
  standalone: true,
  imports: [RouterLink, ConfirmDialogComponent],
  templateUrl: './my-form-followups.component.html',
  styleUrl: './my-form-followups.component.scss'
})
export class MyFormFollowupsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);

  readonly formId = signal('');
  readonly submissionId = signal('');
  readonly loading = signal(true);
  readonly error = signal('');
  readonly followups = signal<SubmissionItem[]>([]);
  readonly columns = signal<ListColumn[]>([]);
  readonly config = signal<FollowupsPage['config']>(null);
  readonly confirmTarget = signal<SubmissionItem | null>(null);

  readonly canCreate = computed(() => this.auth.currentUser()?.canCreate ?? true);

  readonly canAddFollowup = computed(() => {
    if (!this.canCreate()) return false;
    const cfg = this.config();
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
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const parentId = this.submissionId();

    this.api.get<ApiResult<FollowupsPage>>(`/submissions/${parentId}/followups`).subscribe({
      next: res => {
        this.config.set(res.data?.config ?? null);
        this.columns.set(res.data?.columns ?? []);
        this.followups.set(res.data?.items ?? []);
        this.loading.set(false);
        if (!res.data?.config) {
          this.error.set('No follow-up form is configured for this entry.');
        }
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load follow-ups.');
        this.loading.set(false);
      }
    });
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
          this.load();
        },
        error: err => {
          this.error.set(err.error?.message ?? 'Delete failed.');
          this.confirmTarget.set(null);
        }
      });
  }
}
