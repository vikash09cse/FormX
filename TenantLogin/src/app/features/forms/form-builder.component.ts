import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';

interface FormGroup {
  id: string;
  formId: string;
  groupName: string;
  groupDisplayNameKey?: string | null;
  displayOrder: number;
}

interface FormField {
  id: string;
  formGroupId: string;
  controlLabel: string;
  controlTypeName: string;
  controlRequired: boolean;
  displayOrder: number;
  parentFieldId?: string | null;
}

@Component({
  selector: 'app-form-builder',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './form-builder.component.html',
  styleUrl: './form-builder.component.scss'
})
export class FormBuilderComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  readonly formId = signal('');
  readonly formName = signal('');
  readonly groups = signal<FormGroup[]>([]);
  readonly fields = signal<FormField[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.formId.set(id);
    this.api.get<ApiResult<{ name: string }>>(`/forms/${id}`).subscribe({
      next: res => this.formName.set(res.data?.name ?? 'Form')
    });
    this.reload();
  }

  reload() {
    this.loading.set(true);
    this.error.set('');
    const id = this.formId();
    this.api.get<ApiResult<FormGroup[]>>(`/forms/${id}/groups`).subscribe({
      next: res => this.groups.set(res.data ?? [])
    });
    this.api.get<ApiResult<FormField[]>>(`/forms/${id}/fields`).subscribe({
      next: res => {
        this.fields.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load fields.');
        this.loading.set(false);
      }
    });
  }

  fieldsForGroup(groupId: string) {
    return this.fields().filter(f => f.formGroupId === groupId);
  }
}
