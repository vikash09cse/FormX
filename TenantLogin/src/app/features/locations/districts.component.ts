import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { LocationItem } from './location.models';

@Component({
  selector: 'app-districts',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './districts.component.html',
  styleUrl: './districts.component.scss'
})
export class DistrictsComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly items = signal<LocationItem[]>([]);
  readonly states = signal<LocationItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<LocationItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<LocationItem | null>(null);
  readonly filterStateId = signal('');

  name = '';
  code = '';
  statusCode = 1;
  stateId = '';

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete district "${item.name}"?` : '';
  });

  ngOnInit() {
    this.loadStates();
    this.load();
  }

  loadStates() {
    this.api.get<ApiResult<LocationItem[]>>('/locations/states').subscribe({
      next: res => this.states.set(res.data ?? []),
      error: () => this.states.set([])
    });
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const stateId = this.filterStateId();
    const path = stateId ? `/locations/districts?stateId=${stateId}` : '/locations/districts';
    this.api.get<ApiResult<LocationItem[]>>(path).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load districts.');
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.load();
  }

  openCreate() {
    this.editing.set(null);
    this.name = '';
    this.code = '';
    this.statusCode = 1;
    this.stateId = this.filterStateId() || '';
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: LocationItem) {
    this.editing.set(item);
    this.name = item.name;
    this.code = item.code ?? '';
    this.statusCode = item.statusCode;
    this.stateId = item.stateId ?? '';
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  save() {
    if (!this.stateId) {
      this.formError.set('State is required.');
      return;
    }
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
    const body = {
      stateId: this.stateId,
      name: this.name.trim(),
      code: this.code.trim(),
      status: this.statusCode
    };
    const req = this.isEditing()
      ? this.api.put<ApiResult<LocationItem>>(`/locations/districts/${this.editing()!.id}`, body)
      : this.api.post<ApiResult<LocationItem>>('/locations/districts', body);
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
    this.api.delete<ApiResult<boolean>>(`/locations/districts/${item.id}`).subscribe({
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
