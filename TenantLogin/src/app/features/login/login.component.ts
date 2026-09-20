import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { getApiErrorMessage } from '../../core/api/api.util';
import { tenantHomePath } from '../../core/auth/auth.guard';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    rememberMe: [false]
  });

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password, rememberMe } = this.form.getRawValue();
    this.auth.login(email!, password!, !!rememberMe).subscribe({
      next: () => this.router.navigate([tenantHomePath(this.auth.currentUser())]),
      error: err => {
        const timedOut = err?.name === 'TimeoutError';
        this.error.set(
          timedOut
            ? 'Login is taking too long. Please try again.'
            : getApiErrorMessage(err, 'Invalid email or password.')
        );
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }
}
