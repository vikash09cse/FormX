import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { debounceTime, Subject } from 'rxjs';
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
  imports: [
    RouterLink,
    FormsModule,
    ConfirmDialogComponent,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './my-form-entries.component.html',
  styleUrl: './my-form-entries.component.scss'
})
export class MyFormEntriesComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchInput$ = new Subject<string>();

  readonly formId = signal('');
  readonly formName = signal('Form');
  readonly columns = signal<ListColumn[]>([]);
  readonly items = signal<SubmissionItem[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(25);
  readonly searchText = signal('');
  readonly search = signal('');
  readonly loading = signal(true);
  readonly error = signal('');
  readonly confirmTarget = signal<SubmissionItem | null>(null);
  readonly hasLoadedOnce = signal(false);

  readonly displayedColumns = computed(() => [
    'projectName',
    ...this.columns().map(c => c.fieldId),
    'submittedAt',
    'actions'
  ]);

  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete entry for ${item.projectName} (${this.formatDate(item.submittedAt)})?` : '';
  });

  readonly isFiltered = computed(() => this.searchText().trim().length > 0);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('formId') ?? '';
    this.formId.set(id);
    this.api.get<ApiResult<{ name: string }>>(`/submissions/forms/${id}/definition`).subscribe({
      next: res => this.formName.set(res.data?.name ?? 'Form'),
      error: () => this.formName.set('Form')
    });

    this.searchInput$.pipe(
      debounceTime(300),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(term => {
      this.search.set(term.trim());
      this.page.set(1);
      this.load();
    });

    this.load();
  }

  onSearchInput(value: string) {
    this.searchText.set(value);
    this.searchInput$.next(value);
  }

  clearSearch() {
    this.searchText.set('');
    this.search.set('');
    this.page.set(1);
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const page = this.page();
    const pageSize = this.pageSize();
    const search = this.search().trim();
    const params = new URLSearchParams({
      formId: this.formId(),
      page: String(page),
      pageSize: String(pageSize)
    });
    if (search) params.set('search', search);

    this.api.get<ApiResult<SubmissionListPage>>(
      `/submissions?${params.toString()}`
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
        this.hasLoadedOnce.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load entries.');
        this.hasLoadedOnce.set(true);
        this.loading.set(false);
      }
    });
  }

  onPage(event: PageEvent) {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  cellValue(item: SubmissionItem, fieldId: string): string {
    const v = item.values?.[fieldId];
    return v == null || v === '' ? '—' : v;
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
