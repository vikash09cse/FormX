import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';
import { HtmlEditorComponent } from '../../shared/html-editor/html-editor.component';

interface DocumentTemplate {
  id: string;
  templateType: number;
  templateTypeName: string;
  name: string;
  subject?: string | null;
  bodyHtml: string;
  isDefault: boolean;
}

@Component({
  selector: 'app-document-template-edit',
  standalone: true,
  imports: [FormsModule, RouterLink, HtmlEditorComponent],
  templateUrl: './document-template-edit.component.html',
  styleUrl: './document-template-edit.component.scss'
})
export class DocumentTemplateEditComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  templateId = '';
  templateType = 1;
  templateTypeName = '';
  name = '';
  subject = '';
  bodyHtml = '';

  ngOnInit() {
    this.templateId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.templateId) {
      this.error.set('Template not found.');
      this.loading.set(false);
      return;
    }
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.get<ApiResult<DocumentTemplate>>(`/document-templates/${this.templateId}`).subscribe({
      next: res => {
        const t = res.data;
        if (!t) {
          this.error.set('Template not found.');
          this.loading.set(false);
          return;
        }
        this.templateType = t.templateType;
        this.templateTypeName = t.templateTypeName;
        this.name = t.name;
        this.subject = t.subject ?? '';
        this.bodyHtml = t.bodyHtml ?? '';
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load template.');
        this.loading.set(false);
      }
    });
  }

  save() {
    if (!this.name.trim() || !this.bodyHtml.trim()) {
      this.error.set('Name and body are required.');
      return;
    }
    if (this.templateType === 2 && !this.subject.trim()) {
      this.error.set('Email subject is required.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.message.set('');

    this.api.put<ApiResult<DocumentTemplate>>(`/document-templates/${this.templateId}`, {
      name: this.name.trim(),
      subject: this.templateType === 2 ? this.subject.trim() : null,
      bodyHtml: this.bodyHtml
    }).subscribe({
      next: res => {
        this.message.set(res.message || 'Template updated.');
        this.saving.set(false);
        this.router.navigate(['/document-templates']);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Update failed.');
        this.saving.set(false);
      }
    });
  }
}
