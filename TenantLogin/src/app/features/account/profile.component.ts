import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { getApiErrorMessage } from '../../core/api/api.util';
import { AuthService } from '../../core/auth/auth.service';
import { ApiResult, UpdateProfileRequest, UserProfile } from '../../core/models/api.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly email = signal('');
  readonly role = signal('');

  form = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    designation: ['']
  });

  ngOnInit() {
    this.api.get<ApiResult<UserProfile>>('/account/profile').subscribe({
      next: res => {
        const p = res.data;
        this.email.set(p.email);
        this.role.set(p.role);
        this.form.patchValue({
          firstName: p.firstName,
          lastName: p.lastName,
          designation: p.designation ?? ''
        });
        this.loading.set(false);
      },
      error: err => {
        this.error.set(getApiErrorMessage(err, 'Unable to load profile.'));
        this.loading.set(false);
      }
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.success.set('');

    const raw = this.form.getRawValue();
    const body: UpdateProfileRequest = {
      firstName: raw.firstName!.trim(),
      lastName: raw.lastName!.trim(),
      designation: raw.designation?.trim() || null
    };

    this.api.put<ApiResult<UserProfile>>('/account/profile', body).subscribe({
      next: res => {
        const p = res.data;
        this.auth.patchCurrentUser({
          fullName: `${p.firstName} ${p.lastName}`.trim(),
          designation: p.designation ?? null,
          email: p.email
        });
        this.success.set(res.message || 'Profile updated successfully.');
        this.saving.set(false);
      },
      error: err => {
        this.error.set(getApiErrorMessage(err, 'Unable to update profile.'));
        this.saving.set(false);
      }
    });
  }
}
