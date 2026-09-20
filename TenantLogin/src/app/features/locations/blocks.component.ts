import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { LocationItem } from './location.models';

@Component({
  selector: 'app-blocks',
  standalone: true,
  imports: [FormsModule, ConfirmDialogComponent],
  templateUrl: './blocks.component.html',
  styleUrl: './blocks.component.scss'
})
export class BlocksComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly items = signal<LocationItem[]>([]);
  readonly districts = signal<LocationItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly drawerOpen = signal(false);
  readonly editing = signal<LocationItem | null>(null);
  readonly saving = signal(false);
  readonly formError = signal('');
  readonly confirmTarget = signal<LocationItem | null>(null);
  readonly filterDistrictId = signal('');

  name = '';
  code = '';
  statusCode = 1;
  districtId = '';

  readonly isEditing = computed(() => this.editing() !== null);
  readonly confirmMessage = computed(() => {
    const item = this.confirmTarget();
    return item ? `Delete block "${item.name}"?` : '';
  });

  ngOnInit() {
    this.loadDistricts();
    this.load();
  }

  loadDistricts() {
    this.api.get<ApiResult<LocationItem[]>>('/locations/districts').subscribe({
      next: res => this.districts.set(res.data ?? []),
      error: () => this.districts.set([])
    });
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const districtId = this.filterDistrictId();
    const path = districtId ? `/locations/blocks?districtId=${districtId}` : '/locations/blocks';
    this.api.get<ApiResult<LocationItem[]>>(path).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load blocks.');
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
    this.districtId = this.filterDistrictId() || '';
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  openEdit(item: LocationItem) {
    this.editing.set(item);
    this.name = item.name;
    this.code = item.code ?? '';
    this.statusCode = item.statusCode;
    this.districtId = item.districtId ?? item.parentId ?? '';
    this.formError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  save() {
    if (!this.districtId) {
      this.formError.set('District is required.');
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
      districtId: this.districtId,
      name: this.name.trim(),
      code: this.code.trim(),
      status: this.statusCode
    };
    const req = this.isEditing()
      ? this.api.put<ApiResult<LocationItem>>(`/locations/blocks/${this.editing()!.id}`, body)
      : this.api.post<ApiResult<LocationItem>>('/locations/blocks', body);
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
    this.api.delete<ApiResult<boolean>>(`/locations/blocks/${item.id}`).subscribe({
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
