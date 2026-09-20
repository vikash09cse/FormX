import { Component, computed, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { fileNameFromContentDisposition, triggerBlobDownload } from './location-import.util';
import { LocationImportResult, LocationItem } from './location.models';

@Component({
  selector: 'app-states',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './states.component.html',
  styleUrl: './states.component.scss'
})
export class StatesComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly importInput = viewChild<ElementRef<HTMLInputElement>>('importInput');

  readonly items = signal<LocationItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<LocationItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<LocationItem | null>(null);
  readonly downloadingTemplate = signal(false);
  readonly importing = signal(false);
  readonly importMessage = signal('');
  readonly importErrors = signal<{ rowNumber: number; message: string }[]>([]);

  name = '';
  code = '';
  statusCode = 1;

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete state "${item.name}"?` : '';
  });

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<LocationItem[]>>('/locations/states').subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load states.');
        this.loading.set(false);
      }
    });
  }

  openCreate() {
    this.editing.set(null);
    this.name = '';
    this.code = '';
    this.statusCode = 1;
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: LocationItem) {
    this.editing.set(item);
    this.name = item.name;
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
    if (!this.name.trim()) {
      this.formError.set('Name is required.');
      return;
    }
    if (!this.code.trim()) {
      this.formError.set('Code is required.');
      return;
    }
    this.saving.set(true);
    this.formError.set('');
    const body = { name: this.name.trim(), code: this.code.trim(), status: this.statusCode };
    const req = this.isEditing()
      ? this.api.put<ApiResult<LocationItem>>(`/locations/states/${this.editing()!.id}`, body)
      : this.api.post<ApiResult<LocationItem>>('/locations/states', body);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }

  askDelete(item: LocationItem) {
    this.confirmTarget.set(item);
  }

  cancelDelete() {
    this.confirmTarget.set(null);
  }

  confirmDelete() {
    const item = this.confirmTarget();
    if (!item) return;
    this.api.delete<ApiResult<boolean>>(`/locations/states/${item.id}`).subscribe({
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

  downloadTemplate() {
    if (this.downloadingTemplate()) return;
    this.downloadingTemplate.set(true);
    this.error.set('');
    this.api.getBlob('/locations/import/template').subscribe({
      next: res => {
        const blob = res.body;
        if (!blob) {
          this.error.set('Template download failed.');
          this.downloadingTemplate.set(false);
          return;
        }
        const fileName =
          fileNameFromContentDisposition(res.headers.get('content-disposition')) ??
          'location-import-template.xlsx';
        triggerBlobDownload(blob, fileName);
        this.downloadingTemplate.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Template download failed.');
        this.downloadingTemplate.set(false);
      }
    });
  }

  openImportPicker() {
    this.importInput()?.nativeElement.click();
  }

  onImportFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || this.importing()) return;

    this.importing.set(true);
    this.importMessage.set('');
    this.importErrors.set([]);
    this.error.set('');

    const formData = new FormData();
    formData.append('file', file);

    this.api.postFormData<ApiResult<LocationImportResult>>('/locations/import', formData).subscribe({
      next: res => {
        const result = res.data;
        if (result) {
          this.importMessage.set(
            `Import finished: ${result.imported} row(s) imported${result.errorCount ? `, ${result.errorCount} error(s)` : ''}.`
          );
          this.importErrors.set(result.errors ?? []);
        }
        this.importing.set(false);
        this.load();
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Import failed.');
        this.importing.set(false);
      }
    });
  }
}
