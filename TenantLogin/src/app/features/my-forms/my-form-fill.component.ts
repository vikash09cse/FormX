import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';

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
  groups: FormGroup[];
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

  readonly formId = signal('');
  readonly submissionId = signal<string | null>(null);
  readonly definition = signal<FormDefinition | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly formError = signal('');

  values: Record<string, string> = {};
  multiValues: Record<string, Set<string>> = {};

  readonly isEditing = computed(() => !!this.submissionId());
  readonly backLink = computed(() => ['/my-forms', this.formId()]);

  readonly allFields = computed(() =>
    (this.definition()?.groups ?? []).flatMap(g => g.fields)
  );

  ngOnInit() {
    const formId = this.route.snapshot.paramMap.get('formId') ?? '';
    const submissionId = this.route.snapshot.paramMap.get('submissionId');
    this.formId.set(formId);
    this.submissionId.set(submissionId);

    this.api.get<ApiResult<FormDefinition>>(`/submissions/forms/${formId}/definition`).subscribe({
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
        for (const f of def.groups.flatMap(g => g.fields)) {
          if (f.controlType === 3) {
            this.multiValues[f.id] = new Set();
          } else {
            this.values[f.id] = '';
          }
        }
        if (submissionId) {
          this.loadSubmission(submissionId);
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

  /** Safe CSS layout classes from field.classname (+ safe defaults). */
  layoutClasses(field: FormField): string {
    // Conditional follow-ups (e.g. Other) always stack full-width under the parent
    if (field.parentFieldId) {
      return 'col-md-12';
    }

    const tokens = (field.className ?? '')
      .split(/\s+/)
      .map(t => t.trim().toLowerCase())
      .filter(t => LAYOUT_TOKENS.has(t));

    if (tokens.length === 0) {
      // Option groups and labels default to full width when classname unset
      if (field.controlType === 2 || field.controlType === 3 || field.controlType === 6 || field.controlType === 7) {
        return 'col-md-12';
      }
      return 'col-md-6';
    }

    // Normalize aliases to grid classes we style
    const normalized = tokens.map(t =>
      t === 'col-12' || t === 'span-2' ? 'col-md-12' : t
    );
    return [...new Set(normalized)].join(' ');
  }

  loadSubmission(id: string) {
    this.api.get<ApiResult<{ values: { fieldId: string; valueText?: string | null }[] }>>(
      `/submissions/${id}`
    ).subscribe({
      next: res => {
        const data = res.data;
        if (!data) {
          this.error.set('Entry not found.');
          this.loading.set(false);
          return;
        }
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
  }

  isMultiChecked(fieldId: string, optionValue: string): boolean {
    return this.multiValues[fieldId]?.has(optionValue) ?? false;
  }

  save() {
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

      if (field.controlRequired && !value) {
        this.formError.set(`'${field.controlLabel}' is required.`);
        return;
      }

      if (value && field.controlMaxLength && value.length > field.controlMaxLength) {
        this.formError.set(`'${field.controlLabel}' exceeds max length.`);
        return;
      }

      if (value && field.validationRegexPattern) {
        try {
          if (!new RegExp(field.validationRegexPattern).test(value)) {
            this.formError.set(
              `'${field.controlLabel}' does not match the required format` +
                (field.validationRegexName ? ` (${field.validationRegexName})` : '') +
                '.'
            );
            return;
          }
        } catch {
          // ignore invalid pattern
        }
      }

      payloadValues.push({ fieldId: field.id, valueText: value || null });
    }

    this.saving.set(true);
    this.formError.set('');
    const body = { formId: this.formId(), values: payloadValues };
    const editId = this.submissionId();
    const req = editId
      ? this.api.put(`/submissions/${editId}`, { values: payloadValues })
      : this.api.post('/submissions', body);

    req.subscribe({
      next: () => this.router.navigate(['/my-forms', this.formId()]),
      error: err => {
        this.formError.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }
}
