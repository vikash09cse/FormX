import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';

interface DocumentTemplate {
  id: string;
  globalDocumentTemplateId?: string | null;
  templateType: number;
  templateTypeName: string;
  name: string;
  subject?: string | null;
  bodyHtml: string;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-document-templates',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './document-templates.component.html',
  styleUrl: './document-templates.component.scss'
})
export class DocumentTemplatesComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  readonly items = signal<DocumentTemplate[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly message = signal('');

  typeFilter: number | null = null;

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    const q = this.typeFilter != null ? `?templateType=${this.typeFilter}` : '';
    this.api.get<ApiResult<DocumentTemplate[]>>(`/document-templates${q}`).subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load templates.');
        this.loading.set(false);
      }
    });
  }

  edit(item: DocumentTemplate) {
    this.router.navigate(['/document-templates', item.id, 'edit']);
  }

  setDefault(item: DocumentTemplate) {
    this.api.patch<ApiResult<boolean>>(`/document-templates/${item.id}/default`).subscribe({
      next: res => {
        this.message.set(res.message || 'Default updated.');
        this.load();
      },
      error: err => this.message.set(err.error?.message ?? 'Unable to set default.')
    });
  }
}
