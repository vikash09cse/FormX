import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TenantService } from '../../core/auth/auth.service';
import { CreateTenantRequest, TenantDetail, TenantSummary, UpdateTenantRequest } from '../../core/models/api.models';

type DrawerMode = 'create' | 'edit';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tenants.component.html',
  styleUrl: './tenants.component.scss'
})
export class TenantsComponent implements OnInit {
  private readonly tenantService = inject(TenantService);
  private readonly fb = inject(FormBuilder);

  readonly tenants = signal<TenantSummary[]>([]);
  readonly drawerOpen = signal(false);
  readonly drawerMode = signal<DrawerMode>('create');
  readonly saving = signal(false);
  readonly message = signal('');
  readonly editingTenantId = signal<string | null>(null);

  form = this.fb.group({
    hospitalName: ['', Validators.required],
    subdomain: ['', Validators.required],
    primaryContactFirstName: ['', Validators.required],
    primaryContactLastName: ['', Validators.required],
    primaryContactEmail: ['', [Validators.required, Validators.email]],
    primaryContactPhone: ['', Validators.required],
    address: ['', Validators.required],
    timezone: ['Asia/Kolkata', Validators.required],
    password: ['']
  });

  ngOnInit() { this.load(); }

  load() {
    this.tenantService.getTenants().subscribe(res => this.tenants.set(res.data ?? []));
  }

  openCreate() {
    this.drawerMode.set('create');
    this.editingTenantId.set(null);
    this.form.reset({ timezone: 'Asia/Kolkata' });
    this.form.controls.password.setValidators([Validators.required, Validators.minLength(6)]);
    this.form.controls.password.updateValueAndValidity();
    this.drawerOpen.set(true);
  }

  openEdit(id: string) {
    this.drawerMode.set('edit');
    this.editingTenantId.set(id);
    this.form.controls.password.clearValidators();
    this.form.controls.password.updateValueAndValidity();

    this.tenantService.getTenant(id).subscribe({
      next: res => {
        const t = res.data as TenantDetail;
        this.form.patchValue({
          hospitalName: t.hospitalName,
          subdomain: t.subdomain,
          primaryContactFirstName: t.primaryContactFirstName,
          primaryContactLastName: t.primaryContactLastName,
          primaryContactEmail: t.primaryContactEmail,
          primaryContactPhone: t.primaryContactPhone,
          address: t.address,
          timezone: t.timezone,
          password: ''
        });
        this.drawerOpen.set(true);
      },
      error: () => this.message.set('Failed to load tenant details.')
    });
  }

  closeDrawer() {
    this.drawerOpen.set(false);
    this.saving.set(false);
  }

  submit() {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);

    if (this.drawerMode() === 'create') {
      this.tenantService.createTenant(this.form.getRawValue() as CreateTenantRequest).subscribe({
        next: res => {
          this.message.set(res.message);
          this.closeDrawer();
          this.load();
        },
        error: err => {
          this.message.set(err.error?.message ?? 'Failed to create tenant.');
          this.saving.set(false);
        }
      });
      return;
    }

    const id = this.editingTenantId();
    if (!id) return;

    const { password, subdomain, ...updateBody } = this.form.getRawValue();
    this.tenantService.updateTenant(id, updateBody as UpdateTenantRequest).subscribe({
      next: res => {
        this.message.set(res.message);
        this.closeDrawer();
        this.load();
      },
      error: err => {
        this.message.set(err.error?.message ?? 'Failed to update tenant.');
        this.saving.set(false);
      }
    });
  }

  suspend(id: string) {
    this.tenantService.suspendTenant(id).subscribe(() => { this.load(); this.message.set('Tenant suspended.'); });
  }

  reactivate(id: string) {
    this.tenantService.reactivateTenant(id).subscribe(() => { this.load(); this.message.set('Tenant reactivated.'); });
  }
}
