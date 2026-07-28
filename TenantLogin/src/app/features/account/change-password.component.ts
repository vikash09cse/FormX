import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ApiService } from '../../core/api/api.service';
import { getApiErrorMessage } from '../../core/api/api.util';
import { ApiResult, ChangePasswordRequest } from '../../core/models/api.models';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const next = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  if (!next || !confirm) return null;
  return next === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  readonly saving = signal(false);
  readonly error = signal('');
  readonly success = signal('');

  form = this.fb.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.success.set('');

    const raw = this.form.getRawValue();
    const body: ChangePasswordRequest = {
      currentPassword: raw.currentPassword!,
      newPassword: raw.newPassword!
    };

    this.api.put<ApiResult<boolean>>('/account/password', body).subscribe({
      next: res => {
        this.success.set(res.message || 'Password changed successfully.');
        this.form.reset();
        this.saving.set(false);
      },
      error: err => {
        this.error.set(getApiErrorMessage(err, 'Unable to change password.'));
        this.saving.set(false);
      }
    });
  }
}
