import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlatformUserService } from '../../core/auth/auth.service';
import { BackOfficeUser, CreateBackOfficeUserRequest, PlatformUserTypes } from '../../core/models/api.models';

@Component({
  selector: 'app-platform-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './platform-users.component.html',
  styleUrl: './platform-users.component.scss'
})
export class PlatformUsersComponent implements OnInit {
  private readonly userService = inject(PlatformUserService);
  private readonly fb = inject(FormBuilder);

  readonly users = signal<BackOfficeUser[]>([]);
  readonly showForm = signal(false);
  readonly message = signal('');
  readonly userTypes = PlatformUserTypes;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(6)]],
    userType: [PlatformUserTypes[1].value, Validators.required]
  });

  ngOnInit() { this.load(); }

  toggleForm() {
    this.showForm.update(v => !v);
    if (!this.showForm()) {
      this.form.reset({ userType: PlatformUserTypes[1].value });
    }
  }

  load() {
    this.userService.getUsers().subscribe(res => this.users.set(res.data?.items ?? []));
  }

  create() {
    if (this.form.invalid) return;
    const body = this.form.getRawValue() as CreateBackOfficeUserRequest;
    this.userService.createUser(body).subscribe({
      next: res => {
        this.message.set(res.message);
        this.showForm.set(false);
        this.form.reset({ userType: PlatformUserTypes[1].value });
        this.load();
      },
      error: err => this.message.set(err.error?.message ?? 'Failed to create user.')
    });
  }

  setStatus(id: string, status: number) {
    this.userService.setStatus(id, status).subscribe(() => this.load());
  }

  deleteUser(user: BackOfficeUser) {
    const name = `${user.firstName} ${user.lastName}`.trim();
    if (!confirm(`Delete platform user "${name}"? This cannot be undone.`)) return;

    this.userService.deleteUser(user.id).subscribe({
      next: res => {
        this.message.set(res.message);
        this.load();
      },
      error: err => this.message.set(err.error?.message ?? 'Failed to delete user.')
    });
  }
}
