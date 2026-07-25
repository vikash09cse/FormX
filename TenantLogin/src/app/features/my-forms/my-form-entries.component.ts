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

interface SubmissionListPage {
  totalCount: number;
  page: number;
  pageSize: number;
  columns: ListColumn[];
  items: SubmissionItem[];
}

@Component({
  selector: 'app-my-form-entries',
  standalone: true,
  imports: [RouterLink, ConfirmDialogComponent],
  templateUrl: './my-form-entries.component.html',
  styleUrl: './my-form-entries.component.scss'
})
export class MyFormEntriesComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  readonly formId = signal('');
  readonly formName = signal('Form');
  readonly columns = signal<ListColumn[]>([]);
  readonly items = signal<SubmissionItem[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(25);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly confirmTarget = signal<SubmissionItem | null>(null);

  readonly totalPages = computed(() => {
    const size = this.pageSize();
    const total = this.totalCount();
    return size > 0 ? Math.max(1, Math.ceil(total / size)) : 1;
  });

  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete entry for ${item.projectName} (${this.formatDate(item.submittedAt)})?` : '';
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('formId') ?? '';
    this.formId.set(id);
    this.api.get<ApiResult<{ name: string }>>(`/submissions/forms/${id}/definition`).subscribe({
      next: res => this.formName.set(res.data?.name ?? 'Form'),
      error: () => this.formName.set('Form')
    });
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const page = this.page();
    const pageSize = this.pageSize();
    this.api.get<ApiResult<SubmissionListPage>>(
      `/submissions?formId=${this.formId()}&page=${page}&pageSize=${pageSize}`
    ).subscribe({
      next: res => {
        const data = res.data;
        this.totalCount.set(data?.totalCount ?? 0);
        this.columns.set(data?.columns ?? []);
        this.items.set(
          (data?.items ?? []).map(i => ({
            ...i,
            values: i.values ?? {}
          }))
        );
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load entries.');
        this.loading.set(false);
      }
    });
  }

  cellValue(item: SubmissionItem, fieldId: string): string {
    const v = item.values?.[fieldId];
    return v == null || v === '' ? '—' : v;
  }

  prevPage() {
    if (this.page() <= 1) return;
    this.page.update(p => p - 1);
    this.load();
  }

  nextPage() {
    if (this.page() >= this.totalPages()) return;
    this.page.update(p => p + 1);
    this.load();
  }

  formatDate(iso: string): string {
    try {
      return new Date(iso).toLocaleString();
    } catch {
      return iso;
    }
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
    this.api.delete<ApiResult<boolean>>(`/submissions/${item.id}`).subscribe({
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
