import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';

interface SubmissionItem {
  id: string;
  formId: string;
  projectId: string;
  projectName: string;
  submittedAt: string;
  status: number;
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
  readonly items = signal<SubmissionItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly confirmTarget = signal<SubmissionItem | null>(null);

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
    this.api.get<ApiResult<SubmissionItem[]>>(`/submissions?formId=${this.formId()}`).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load entries.');
        this.loading.set(false);
      }
    });
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
