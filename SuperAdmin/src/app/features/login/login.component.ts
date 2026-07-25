import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { getApiErrorMessage } from '../../core/api/api.util';

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
    email: ['admin@formx.com', [Validators.required, Validators.email]],
    password: ['Admin@123', Validators.required]
  });

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password } = this.form.getRawValue();
    this.auth.login(email!, password!).subscribe({
      next: () => this.router.navigate(['/tenants']),
      error: err => {
        this.error.set(getApiErrorMessage(err, 'Invalid credentials.'));
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }
}
