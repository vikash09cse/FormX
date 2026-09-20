import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ApiResult } from '../../core/models/api.models';

type DashboardLayout = 'table' | 'cards' | 'topN';

interface DashboardSettings {
  layout: DashboardLayout;
  topN: number;
  selectedFormIds?: string[];
}

interface DashboardForm {
  id: string;
  name: string;
  projectName?: string | null;
  entryCount: number;
  displayOrder: number;
}

interface AvailableForm {
  id: string;
  name: string;
  displayOrder: number;
}

interface DashboardData {
  settings: DashboardSettings;
  totals: { formCount: number; entryCount: number };
  forms: DashboardForm[];
  availableForms?: AvailableForm[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly error = signal('');
  readonly settings = signal<DashboardSettings>({ layout: 'table', topN: 12, selectedFormIds: [] });
  readonly totals = signal({ formCount: 0, entryCount: 0 });
  readonly forms = signal<DashboardForm[]>([]);
  readonly availableForms = signal<AvailableForm[]>([]);
  readonly searchText = signal('');
  readonly formPickerSearch = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(25);
  readonly drawerOpen = signal(false);
  readonly saving = signal(false);
  readonly settingsError = signal('');
  readonly selectedFormIds = signal<Set<string>>(new Set());

  editLayout: DashboardLayout = 'table';
  editTopN = 12;

  readonly isSuperAdmin = computed(
    () => this.auth.currentUser()?.role === 'TenantSuperAdmin'
  );

  readonly filteredForms = computed(() => {
    const q = this.searchText().trim().toLowerCase();
    let list = this.forms();
    if (q) {
      list = list.filter(f =>
        f.name.toLowerCase().includes(q) ||
        (f.projectName ?? '').toLowerCase().includes(q)
      );
    }
    return list;
  });

  readonly displayForms = computed(() => {
    const layout = this.settings().layout;
    let list = this.filteredForms();

    if (layout === 'topN') {
      return [...list]
        .sort((a, b) => b.entryCount - a.entryCount || a.name.localeCompare(b.name))
        .slice(0, this.settings().topN);
    }

    if (layout === 'table') {
      const start = (this.page() - 1) * this.pageSize();
      return list.slice(start, start + this.pageSize());
    }

    return list;
  });

  readonly totalFiltered = computed(() => this.filteredForms().length);
  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalFiltered() / this.pageSize()))
  );

  readonly pickerForms = computed(() => {
    const q = this.formPickerSearch().trim().toLowerCase();
    let list = this.availableForms();
    if (q) {
      list = list.filter(f => f.name.toLowerCase().includes(q));
    }
    return list;
  });

  readonly selectedCount = computed(() => this.selectedFormIds().size);

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<DashboardData>>('/dashboard').subscribe({
      next: res => {
        const data = res.data;
        if (!data) {
          this.error.set('Unable to load dashboard.');
          this.loading.set(false);
          return;
        }
        this.settings.set({
          layout: normalizeLayout(data.settings?.layout),
          topN: data.settings?.topN ?? 12,
          selectedFormIds: data.settings?.selectedFormIds ?? []
        });
        this.totals.set(data.totals ?? { formCount: 0, entryCount: 0 });
        this.forms.set(data.forms ?? []);
        this.availableForms.set(data.availableForms ?? []);
        this.page.set(1);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load dashboard.');
        this.loading.set(false);
      }
    });
  }

  onSearch(value: string) {
    this.searchText.set(value);
    this.page.set(1);
  }

  setPageSize(size: number) {
    this.pageSize.set(size);
    this.page.set(1);
  }

  prevPage() {
    if (this.page() > 1) this.page.update(p => p - 1);
  }

  nextPage() {
    if (this.page() < this.totalPages()) this.page.update(p => p + 1);
  }

  openSettings() {
    this.editLayout = this.settings().layout;
    this.editTopN = this.settings().topN;
    this.selectedFormIds.set(new Set(this.settings().selectedFormIds ?? []));
    this.formPickerSearch.set('');
    this.settingsError.set('');
    this.drawerOpen.set(true);
  }

  closeSettings() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  isFormSelected(id: string): boolean {
    return this.selectedFormIds().has(id);
  }

  toggleForm(id: string, checked: boolean) {
    const next = new Set(this.selectedFormIds());
    if (checked) next.add(id);
    else next.delete(id);
    this.selectedFormIds.set(next);
  }

  selectAllForms() {
    this.selectedFormIds.set(new Set(this.availableForms().map(f => f.id)));
  }

  clearFormSelection() {
    this.selectedFormIds.set(new Set());
  }

  saveSettings() {
    this.saving.set(true);
    this.settingsError.set('');
    const formIds = [...this.selectedFormIds()];
    this.api.put<ApiResult<DashboardSettings>>('/dashboard/settings', {
      layout: this.editLayout,
      topN: this.editTopN,
      formIds
    }).subscribe({
      next: res => {
        const saved = res.data;
        if (saved) {
          this.settings.set({
            layout: normalizeLayout(saved.layout),
            topN: saved.topN,
            selectedFormIds: saved.selectedFormIds ?? []
          });
        }
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
      },
      error: err => {
        this.settingsError.set(err.error?.message ?? 'Unable to save settings.');
        this.saving.set(false);
      }
    });
  }
}

function normalizeLayout(layout?: string | null): DashboardLayout {
  if (layout === 'cards' || layout === 'topN') return layout;
  return 'table';
}
