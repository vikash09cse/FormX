import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentTemplateService } from '../../core/auth/auth.service';
import { GlobalDocumentTemplate } from '../../core/models/api.models';
import { HtmlEditorComponent } from '../../shared/html-editor/html-editor.component';

@Component({
  selector: 'app-document-templates',
  standalone: true,
  imports: [FormsModule, HtmlEditorComponent, DatePipe],
  templateUrl: './document-templates.component.html',
  styleUrl: './document-templates.component.scss'
})
export class DocumentTemplatesComponent implements OnInit {
  private readonly templates = inject(DocumentTemplateService);

  readonly items = signal<GlobalDocumentTemplate[]>([]);
  readonly loading = signal(true);
  readonly message = signal('');
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly editingId = signal<string | null>(null);

  typeFilter: number | null = null;
  templateType = 1;
  name = '';
  subject = '';
  bodyHtml = '';
  isDefault = false;
  readonly placeholderHint =
    'Use placeholders such as {{HospitalName}}, {{PatientName}}, {{VisitCode}}, {{ReceiptNumber}}, {{TotalPaid}}.';

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.templates.getTemplates(this.typeFilter ?? undefined).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.message.set(err.error?.message ?? 'Unable to load templates.');
        this.loading.set(false);
      }
    });
  }

  openCreate() {
    this.editingId.set(null);
    this.templateType = 1;
    this.name = '';
    this.subject = '';
    this.bodyHtml = '';
    this.isDefault = false;
    this.showForm.set(true);
    this.message.set('');
  }

  openEdit(item: GlobalDocumentTemplate) {
    this.editingId.set(item.id);
    this.templateType = item.templateType;
    this.name = item.name;
    this.subject = item.subject ?? '';
    this.bodyHtml = item.bodyHtml;
    this.isDefault = item.isDefault;
    this.showForm.set(true);
    this.message.set('');
  }

  cancelForm() {
    this.showForm.set(false);
  }

  save() {
    if (!this.name.trim() || !this.bodyHtml.trim()) {
      this.message.set('Name and body are required.');
      return;
    }
    if (this.templateType === 2 && !this.subject.trim()) {
      this.message.set('Email subject is required.');
      return;
    }

    const body = {
      templateType: this.templateType,
      name: this.name.trim(),
      subject: this.templateType === 2 ? this.subject.trim() : null,
      bodyHtml: this.bodyHtml,
      isDefault: this.isDefault
    };

    this.saving.set(true);
    const req = this.editingId()
      ? this.templates.updateTemplate(this.editingId()!, body)
      : this.templates.createTemplate(body);

    req.subscribe({
      next: res => {
        this.message.set(res.message || 'Saved.');
        this.saving.set(false);
        this.showForm.set(false);
        this.load();
      },
      error: err => {
        this.message.set(err.error?.message ?? 'Save failed.');
        this.saving.set(false);
      }
    });
  }

  setDefault(item: GlobalDocumentTemplate) {
    this.templates.setDefault(item.id).subscribe({
      next: res => {
        this.message.set(res.message || 'Default updated.');
        this.load();
      },
      error: err => this.message.set(err.error?.message ?? 'Unable to set default.')
    });
  }

  remove(item: GlobalDocumentTemplate) {
    if (!confirm(`Delete template "${item.name}"? Tenant copies are kept.`)) return;
    this.templates.deleteTemplate(item.id).subscribe({
      next: res => {
        this.message.set(res.message || 'Deleted.');
        this.load();
      },
      error: err => this.message.set(err.error?.message ?? 'Delete failed.')
    });
  }
}
