import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { LocationItem } from '../locations/location.models';

interface FieldOption {
  id: string;
  optionText: string;
  optionValue: string;
  displayOrder: number;
}

interface FormField {
  id: string;
  formGroupId: string;
  controlLabel: string;
  controlType: number;
  controlTypeName: string;
  controlMaxLength?: number | null;
  controlRequired: boolean;
  displayOrder: number;
  controlNotes?: string | null;
  fieldKey: string;
  className?: string | null;
  parentFieldId?: string | null;
  validationRegexPattern?: string | null;
  validationRegexName?: string | null;
  options: FieldOption[];
  parentOptionIds: string[];
}

interface FormGroup {
  id: string;
  groupName: string;
  displayOrder: number;
  fields: FormField[];
}

interface FormDefinition {
  formId: string;
  name: string;
  description?: string | null;
  projectId?: string | null;
  projectName?: string | null;
  collectLocation?: boolean;
  groups: FormGroup[];
}

interface SubmissionLoadResponse {
  values: { fieldId: string; valueText?: string | null }[];
  stateId?: string | null;
  districtId?: string | null;
  blockId?: string | null;
  villageId?: string | null;
}

interface FollowupsConfigResponse {
  config?: {
    followUpFormId: string;
    followUpFormName?: string | null;
    allowMultiple: boolean;
  } | null;
}

/** Allowed layout tokens from field.classname (Bootstrap-style). */
const LAYOUT_TOKENS = new Set(['col-md-6', 'col-md-12', 'col-12', 'span-2']);

@Component({
  selector: 'app-my-form-fill',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './my-form-fill.component.html',
  styleUrl: './my-form-fill.component.scss'
})
export class MyFormFillComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  /** Primary form id from the route (navigation context). */
  readonly formId = signal('');
  /** Form definition actually being filled (primary or follow-up form). */
  readonly fillFormId = signal('');
  readonly submissionId = signal<string | null>(null);
  readonly parentSubmissionId = signal<string | null>(null);
  readonly isFollowUp = signal(false);
  readonly definition = signal<FormDefinition | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  /** Server / unexpected save errors only (not per-field validation). */
  readonly formError = signal('');
  /** Per-control validation messages keyed by field id or location key. */
  readonly fieldErrors = signal<Record<string, string>>({});

  values: Record<string, string> = {};
  multiValues: Record<string, Set<string>> = {};

  readonly states = signal<LocationItem[]>([]);
  readonly districts = signal<LocationItem[]>([]);
  readonly blocks = signal<LocationItem[]>([]);
  readonly villages = signal<LocationItem[]>([]);

  stateId = '';
  districtId = '';
  blockId = '';
  villageId = '';

  readonly isEditing = computed(() => !!this.submissionId());
  readonly backLink = computed(() => {
    const parentId = this.parentSubmissionId();
    if (this.isFollowUp() && parentId) {
      return ['/my-forms', this.formId(), 'entries', parentId, 'followups'];
    }
    if (this.isEditing()) {
      return ['/my-forms', this.formId(), 'entries', this.submissionId()!];
    }
    return ['/my-forms', this.formId()];
  });
  readonly pageTitle = computed(() => {
    if (this.isFollowUp()) {
      return this.isEditing() ? 'Edit follow-up' : 'Add follow-up';
    }
    return this.isEditing() ? 'Edit entry' : 'Add new entry';
  });
  readonly backLabel = computed(() => {
    if (this.isFollowUp()) return '← Back to follow-ups';
    return this.isEditing() ? '← Back to entry' : '← Back to entries';
  });

  readonly allFields = computed(() =>
    (this.definition()?.groups ?? []).flatMap(g => g.fields)
  );

  readonly collectLocation = computed(() => !!this.definition()?.collectLocation);

  ngOnInit() {
    const formId = this.route.snapshot.paramMap.get('formId') ?? '';
    const submissionId = this.route.snapshot.paramMap.get('submissionId');
    const followUpId = this.route.snapshot.paramMap.get('followUpId');
    const url = this.router.url;
    const isFollowUp = url.includes('/followups/');

    this.formId.set(formId);
    this.isFollowUp.set(isFollowUp);

    if (isFollowUp) {
      this.parentSubmissionId.set(submissionId);
      this.submissionId.set(followUpId);
      this.beginFollowUp(formId, submissionId!, followUpId);
      return;
    }

    this.fillFormId.set(formId);
    this.submissionId.set(submissionId);
    this.parentSubmissionId.set(null);
    this.loadDefinition(formId, submissionId);
  }

  private beginFollowUp(_primaryFormId: string, parentId: string, followUpId: string | null) {
    this.api.get<ApiResult<FollowupsConfigResponse>>(`/submissions/${parentId}/followups`).subscribe({
      next: res => {
        const followUpFormId = res.data?.config?.followUpFormId;
        if (!followUpFormId) {
          this.error.set('No follow-up form is configured for this entry.');
          this.loading.set(false);
          return;
        }
        this.fillFormId.set(followUpFormId);
        this.loadDefinition(followUpFormId, followUpId, parentId);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load follow-up configuration.');
        this.loading.set(false);
      }
    });
  }

  private loadDefinition(definitionFormId: string, existingId: string | null | undefined, parentId?: string) {
    this.api.get<ApiResult<FormDefinition>>(`/submissions/forms/${definitionFormId}/definition`).subscribe({
      next: res => {
        const def = res.data;
        if (!def) {
          this.error.set('Form not found.');
          this.loading.set(false);
          return;
        }
        if (!def.projectId) {
          this.error.set('This form has no project assigned. Ask an admin to set a project on the form.');
          this.loading.set(false);
          return;
        }
        this.definition.set(def);
        if (def.collectLocation) {
          this.loadStates();
        }
        for (const f of def.groups.flatMap(g => g.fields)) {
          if (f.controlType === 3) {
            this.multiValues[f.id] = new Set();
          } else {
            this.values[f.id] = '';
          }
        }
        if (existingId) {
          this.loadSubmission(existingId, parentId);
        } else {
          this.loading.set(false);
        }
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load form.');
        this.loading.set(false);
      }
    });
  }

  /** Safe CSS layout classes from field.classname (+ safe defaults when empty). */
  layoutClasses(field: FormField): string {
    const tokens = (field.className ?? '')
      .split(/\s+/)
      .map(t => t.trim().toLowerCase())
      .filter(t => LAYOUT_TOKENS.has(t));

    // Form builder "CSS class" wins whenever set (e.g. col-md-6 / col-md-12).
    if (tokens.length > 0) {
      const normalized = tokens.map(t =>
        t === 'col-12' || t === 'span-2' ? 'col-md-12' : t
      );
      return [...new Set(normalized)].join(' ');
    }

    // Defaults only when CSS class was left blank in the builder.
    if (field.parentFieldId) {
      return 'col-md-12';
    }
    if (
      field.controlType === 2 ||
      field.controlType === 3 ||
      field.controlType === 6 ||
      field.controlType === 7
    ) {
      return 'col-md-12';
    }
    return 'col-md-6';
  }

  loadStates() {
    this.api.get<ApiResult<LocationItem[]>>('/locations/states').subscribe({
      next: res => this.states.set(res.data ?? []),
      error: () => this.states.set([])
    });
  }

  onStateChange() {
    this.clearFieldError('stateId');
    this.clearFieldError('districtId');
    this.districtId = '';
    this.blockId = '';
    this.villageId = '';
    this.districts.set([]);
    this.blocks.set([]);
    this.villages.set([]);
    if (!this.stateId) return;
    this.api.get<ApiResult<LocationItem[]>>(`/locations/districts?stateId=${this.stateId}`).subscribe({
      next: res => this.districts.set(res.data ?? []),
      error: () => this.districts.set([])
    });
  }

  onDistrictChange() {
    this.clearFieldError('districtId');
    this.blockId = '';
    this.villageId = '';
    this.blocks.set([]);
    this.villages.set([]);
    if (!this.districtId) return;
    this.api.get<ApiResult<LocationItem[]>>(`/locations/blocks?districtId=${this.districtId}`).subscribe({
      next: res => this.blocks.set(res.data ?? []),
      error: () => this.blocks.set([])
    });
  }

  onBlockChange() {
    this.villageId = '';
    this.villages.set([]);
    if (!this.blockId) return;
    this.api.get<ApiResult<LocationItem[]>>(`/locations/villages?blockId=${this.blockId}`).subscribe({
      next: res => this.villages.set(res.data ?? []),
      error: () => this.villages.set([])
    });
  }

  private applyLocationFromSubmission(data: SubmissionLoadResponse) {
    if (!this.collectLocation()) return;
    this.stateId = data.stateId ?? '';
    this.districtId = data.districtId ?? '';
    this.blockId = data.blockId ?? '';
    this.villageId = data.villageId ?? '';
    if (this.stateId) {
      this.api.get<ApiResult<LocationItem[]>>(`/locations/districts?stateId=${this.stateId}`).subscribe({
        next: res => {
          this.districts.set(res.data ?? []);
          if (this.districtId) {
            this.api.get<ApiResult<LocationItem[]>>(`/locations/blocks?districtId=${this.districtId}`).subscribe({
              next: blockRes => {
                this.blocks.set(blockRes.data ?? []);
                if (this.blockId) {
                  this.api
                    .get<ApiResult<LocationItem[]>>(`/locations/villages?blockId=${this.blockId}`)
                    .subscribe({
                      next: villageRes => this.villages.set(villageRes.data ?? []),
                      error: () => this.villages.set([])
                    });
                }
              },
              error: () => this.blocks.set([])
            });
          }
        },
        error: () => this.districts.set([])
      });
    }
  }

  loadSubmission(id: string, parentId?: string) {
    const url = parentId
      ? `/submissions/${parentId}/followups/${id}`
      : `/submissions/${id}`;

    this.api.get<ApiResult<SubmissionLoadResponse>>(url).subscribe({
      next: res => {
        const data = res.data;
        if (!data) {
          this.error.set('Entry not found.');
          this.loading.set(false);
          return;
        }
        this.applyLocationFromSubmission(data);
        for (const v of data.values ?? []) {
          const field = this.allFields().find(f => f.id === v.fieldId);
          if (!field) continue;
          const text = v.valueText ?? '';
          if (field.controlType === 3) {
            this.multiValues[field.id] = new Set(
              text.split(',').map(s => s.trim()).filter(Boolean)
            );
          } else {
            this.values[field.id] = text;
          }
        }
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load entry.');
        this.loading.set(false);
      }
    });
  }

  isVisible(field: FormField): boolean {
    if (!field.parentFieldId) return true;
    if (!field.parentOptionIds?.length) return false;
    const parent = this.allFields().find(f => f.id === field.parentFieldId);
    if (!parent) return false;

    const selectedValues = this.selectedValuesFor(parent);
    if (!selectedValues.length) return false;

    const selectedOptionIds = parent.options
      .filter(o => selectedValues.includes(o.optionValue))
      .map(o => o.id);

    return selectedOptionIds.some(id => field.parentOptionIds.includes(id));
  }

  selectedValuesFor(field: FormField): string[] {
    if (field.controlType === 3) {
      return [...(this.multiValues[field.id] ?? [])];
    }
    const v = this.values[field.id];
    return v ? [v] : [];
  }

  toggleMulti(fieldId: string, optionValue: string, checked: boolean) {
    const set = this.multiValues[fieldId] ?? new Set<string>();
    if (checked) set.add(optionValue);
    else set.delete(optionValue);
    this.multiValues[fieldId] = new Set(set);
    this.clearFieldError(fieldId);
  }

  isMultiChecked(fieldId: string, optionValue: string): boolean {
    return this.multiValues[fieldId]?.has(optionValue) ?? false;
  }

  fieldError(key: string): string {
    return this.fieldErrors()[key] ?? '';
  }

  clearFieldError(key: string) {
    const current = this.fieldErrors();
    if (!current[key]) return;
    const next = { ...current };
    delete next[key];
    this.fieldErrors.set(next);
  }

  onFieldValueChange(fieldId: string) {
    this.clearFieldError(fieldId);
  }

  private validateForm(): boolean {
    const errors: Record<string, string> = {};

    for (const field of this.allFields()) {
      if (!this.isVisible(field)) continue;
      if (field.controlType === 7) continue;

      let value = '';
      if (field.controlType === 3) {
        value = [...(this.multiValues[field.id] ?? [])].join(',');
      } else {
        value = (this.values[field.id] ?? '').trim();
      }

      if (field.controlRequired && !value) {
        errors[field.id] = 'This field is required.';
        continue;
      }

      if (value && field.controlMaxLength && value.length > field.controlMaxLength) {
        errors[field.id] = `Exceeds max length of ${field.controlMaxLength}.`;
        continue;
      }

      if (value && field.validationRegexPattern) {
        try {
          if (!new RegExp(field.validationRegexPattern).test(value)) {
            errors[field.id] =
              'Does not match the required format' +
              (field.validationRegexName ? ` (${field.validationRegexName})` : '') +
              '.';
          }
        } catch {
          // ignore invalid pattern
        }
      }
    }

    if (this.collectLocation()) {
      if (!this.stateId) errors['stateId'] = 'State is required.';
      if (!this.districtId) errors['districtId'] = 'District is required.';
    }

    this.fieldErrors.set(errors);
    return Object.keys(errors).length === 0;
  }

  private scrollToFirstError(errors: Record<string, string>) {
    const order: string[] = [];
    if (this.collectLocation()) {
      order.push('stateId', 'districtId');
    }
    for (const field of this.allFields()) {
      if (!this.isVisible(field) || field.controlType === 7) continue;
      order.push(field.id);
    }

    const firstKey = order.find(k => !!errors[k]);
    if (!firstKey) return;

    setTimeout(() => {
      const host = document.querySelector(`[data-field-key="${CSS.escape(firstKey)}"]`) as HTMLElement | null;
      if (!host) return;
      host.scrollIntoView({ behavior: 'smooth', block: 'center' });
      const focusable = host.querySelector('select, input:not([type="hidden"]), textarea') as HTMLElement | null;
      focusable?.focus({ preventScroll: true });
    });
  }

  save() {
    this.formError.set('');
    if (!this.validateForm()) {
      this.scrollToFirstError(this.fieldErrors());
      return;
    }

    const payloadValues: { fieldId: string; valueText: string | null }[] = [];
    for (const field of this.allFields()) {
      if (!this.isVisible(field)) continue;
      if (field.controlType === 7) continue;

      let value = '';
      if (field.controlType === 3) {
        value = [...(this.multiValues[field.id] ?? [])].join(',');
      } else {
        value = (this.values[field.id] ?? '').trim();
      }

      payloadValues.push({ fieldId: field.id, valueText: value || null });
    }

    this.saving.set(true);

    const parentId = this.parentSubmissionId();
    const editId = this.submissionId();
    const locationPayload = this.collectLocation()
      ? {
          stateId: this.stateId,
          districtId: this.districtId,
          blockId: this.blockId || null,
          villageId: this.villageId || null
        }
      : {};
    let req;

    if (this.isFollowUp() && parentId) {
      req = editId
        ? this.api.put(`/submissions/${parentId}/followups/${editId}`, { values: payloadValues, ...locationPayload })
        : this.api.post(`/submissions/${parentId}/followups`, { values: payloadValues, ...locationPayload });
    } else {
      req = editId
        ? this.api.put(`/submissions/${editId}`, { values: payloadValues, ...locationPayload })
        : this.api.post('/submissions', { formId: this.fillFormId(), values: payloadValues, ...locationPayload });
    }

    req.subscribe({
      next: () => {
        if (this.isFollowUp() && parentId) {
          this.router.navigate(['/my-forms', this.formId(), 'entries', parentId, 'followups']);
        } else if (editId) {
          this.router.navigate(['/my-forms', this.formId(), 'entries', editId]);
        } else {
          this.router.navigate(['/my-forms', this.formId()]);
        }
      },
      error: err => {
        this.formError.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }
}
