import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';

interface FormGroupItem {
  id: string;
  groupName: string;
}

interface FieldOption {
  id?: string | null;
  optionText: string;
  optionValue: string;
  displayOrder: number;
}

interface FormField {
  id: string;
  formId: string;
  formGroupId: string;
  controlLabel: string;
  controlType: number;
  controlTypeName: string;
  controlMaxLength?: number | null;
  controlRequired: boolean;
  displayOrder: number;
  controlNotes?: string | null;
  fieldKey: string;
  displayControlLabel: string;
  className?: string | null;
  parentFieldId?: string | null;
  isSendEmailNotification: boolean;
  validationRegexPresetId?: string | null;
  displayOnList: boolean;
  options: FieldOption[];
  parentOptionIds: string[];
}

interface RegexPreset {
  id: string;
  name: string;
}

const CONTROL_TYPES = [
  { id: 1, name: 'Dropdown' },
  { id: 2, name: 'Radio Button' },
  { id: 3, name: 'Checkbox' },
  { id: 4, name: 'Textbox' },
  { id: 5, name: 'File' },
  { id: 6, name: 'Checkbox - Single' },
  { id: 7, name: 'Label' },
  { id: 8, name: 'Email' },
  { id: 9, name: 'Date' }
];

@Component({
  selector: 'app-form-field-form',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './form-field-form.component.html',
  styleUrl: './form-field-form.component.scss'
})
export class FormFieldFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly formId = signal('');
  readonly fieldId = signal<string | null>(null);
  readonly formName = signal('');
  readonly groups = signal<FormGroupItem[]>([]);
  readonly fields = signal<FormField[]>([]);
  readonly presets = signal<RegexPreset[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly formError = signal('');

  controlLabel = '';
  controlType = 4;
  formGroupId = '';
  controlMaxLength: number | null = null;
  controlRequired = false;
  displayOrder = 0;
  controlNotes = '';
  fieldKey = '';
  displayControlLabel = 'visible';
  className = 'col-md-6';
  parentFieldId: string | null = null;
  isSendEmailNotification = false;
  validationRegexPresetId: string | null = null;
  displayOnList = false;
  optionRows: FieldOption[] = [{ optionText: '', optionValue: '', displayOrder: 1 }];
  selectedParentOptionIds = new Set<string>();

  readonly controlTypes = CONTROL_TYPES;
  readonly isEditing = computed(() => !!this.fieldId());
  readonly backLink = computed(() => ['/forms', this.formId()]);

  isEmailType(): boolean {
    return this.controlType === 8;
  }

  isLabelType(): boolean {
    return this.controlType === 7;
  }

  isTextboxType(): boolean {
    return this.controlType === 4;
  }

  listFieldCount(): number {
    return this.fields().filter(
      f => f.displayOnList && f.id !== this.fieldId() && f.controlType !== 7
    ).length;
  }

  canEnableDisplayOnList(): boolean {
    if (this.isLabelType()) return false;
    if (this.displayOnList) return true;
    return this.listFieldCount() < 5;
  }

  needsOptions(): boolean {
    return [1, 2, 3, 6].includes(this.controlType);
  }

  readonly parentCandidates = computed(() =>
    this.fields().filter(f => f.id !== this.fieldId() && [1, 2, 3, 6].includes(f.controlType))
  );

  readonly parentOptions = computed(() => {
    const parentId = this.parentFieldId;
    if (!parentId) return [] as FieldOption[];
    return this.fields().find(f => f.id === parentId)?.options ?? [];
  });

  ngOnInit() {
    const formId = this.route.snapshot.queryParamMap.get('formId') ?? '';
    const fieldId = this.route.snapshot.paramMap.get('id');
    this.formId.set(formId);
    this.fieldId.set(fieldId && fieldId !== 'new' ? fieldId : null);

    if (!formId) {
      this.error.set('Form is required.');
      this.loading.set(false);
      return;
    }

    this.api.get<ApiResult<{ name: string }>>(`/forms/${formId}`).subscribe({
      next: res => this.formName.set(res.data?.name ?? 'Form')
    });
    this.api.get<ApiResult<RegexPreset[]>>('/validation-regex-presets').subscribe({
      next: res => this.presets.set(res.data ?? [])
    });

    this.api.get<ApiResult<FormGroupItem[]>>(`/forms/${formId}/groups`).subscribe({
      next: res => {
        const groups = res.data ?? [];
        this.groups.set(groups);
        if (!this.formGroupId && groups.length) {
          this.formGroupId = groups[0].id;
        }
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load groups.');
        this.loading.set(false);
      }
    });

    this.api.get<ApiResult<FormField[]>>(`/forms/${formId}/fields`).subscribe({
      next: res => {
        const fields = res.data ?? [];
        this.fields.set(fields);
        const id = this.fieldId();
        if (id) {
          const existing = fields.find(f => f.id === id);
          if (!existing) {
            this.error.set('Field not found.');
            this.loading.set(false);
            return;
          }
          this.populate(existing);
        }
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load fields.');
        this.loading.set(false);
      }
    });
  }

  populate(f: FormField) {
    this.controlLabel = f.controlLabel;
    this.controlType = f.controlType;
    this.formGroupId = f.formGroupId;
    this.controlMaxLength = f.controlMaxLength ?? null;
    this.controlRequired = f.controlRequired;
    this.displayOrder = f.displayOrder;
    this.controlNotes = f.controlNotes ?? '';
    this.fieldKey = f.fieldKey ?? '';
    this.displayControlLabel = f.displayControlLabel ?? 'visible';
    this.className = f.className ?? 'col-md-6';
    this.parentFieldId = f.parentFieldId ?? null;
    this.isSendEmailNotification = f.isSendEmailNotification;
    this.validationRegexPresetId = f.validationRegexPresetId ?? null;
    this.displayOnList = !!f.displayOnList;
    this.optionRows = f.options?.length
      ? f.options.map(o => ({ ...o }))
      : [{ optionText: '', optionValue: '', displayOrder: 1 }];
    this.selectedParentOptionIds = new Set(f.parentOptionIds ?? []);
  }

  addOptionRow() {
    this.optionRows = [...this.optionRows, { optionText: '', optionValue: '', displayOrder: this.optionRows.length + 1 }];
  }

  removeOptionRow(i: number) {
    this.optionRows = this.optionRows.filter((_, idx) => idx !== i);
  }

  toggleParentOption(id: string, checked: boolean) {
    if (checked) this.selectedParentOptionIds.add(id);
    else this.selectedParentOptionIds.delete(id);
  }

  isParentOptionSelected(id: string) {
    return this.selectedParentOptionIds.has(id);
  }

  save() {
    if (!this.controlLabel.trim()) {
      this.formError.set('Label is required.');
      return;
    }
    if (!this.formGroupId) {
      this.formError.set('Form group is required.');
      return;
    }
    if (this.needsOptions()) {
      const validOptions = this.optionRows.filter(o => o.optionText.trim());
      if (validOptions.length === 0) {
        this.formError.set('Add at least one option text and value.');
        return;
      }
    }

    this.saving.set(true);
    this.formError.set('');
    const body = {
      formGroupId: this.formGroupId,
      controlLabel: this.controlLabel.trim(),
      controlType: this.controlType,
      controlMaxLength: this.isTextboxType() ? this.controlMaxLength : null,
      controlRequired: this.controlRequired,
      displayOrder: this.displayOrder,
      controlNotes: this.controlNotes || null,
      fieldKey: this.fieldKey || null,
      displayControlLabel: this.displayControlLabel,
      className: this.className || null,
      parentFieldId: this.parentFieldId,
      isSendEmailNotification: this.controlType === 8 && this.isSendEmailNotification,
      validationRegexPresetId: this.validationRegexPresetId,
      displayOnList: !this.isLabelType() && this.displayOnList,
      options: this.needsOptions()
        ? this.optionRows.filter(o => o.optionText.trim()).map((o, i) => ({
            id: o.id ?? null,
            optionText: o.optionText.trim(),
            optionValue: (o.optionValue || o.optionText).trim(),
            displayOrder: o.displayOrder || i + 1
          }))
        : [],
      parentOptionIds: this.parentFieldId ? [...this.selectedParentOptionIds] : []
    };

    const formId = this.formId();
    const fieldId = this.fieldId();
    const req = fieldId
      ? this.api.put(`/forms/${formId}/fields/${fieldId}`, body)
      : this.api.post(`/forms/${formId}/fields`, body);

    req.subscribe({
      next: () => this.router.navigate(['/forms', formId]),
      error: err => {
        this.formError.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }
}
