import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api/api.service';
import { ApiResult } from '../../core/models/api.models';

interface AvailableForm {
  id: string;
  name: string;
  description?: string | null;
  displayOrder: number;
}

@Component({
  selector: 'app-my-forms',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './my-forms.component.html',
  styleUrl: './my-forms.component.scss'
})
export class MyFormsComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly items = signal<AvailableForm[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit() {
    this.api.get<ApiResult<AvailableForm[]>>('/submissions/available-forms').subscribe({
      next: res => {
        this.items.set(res.data ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Unable to load forms.');
        this.loading.set(false);
      }
    });
  }
}
