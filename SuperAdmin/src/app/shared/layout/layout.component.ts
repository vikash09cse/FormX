import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: 'tenants' | 'users' | 'templates';
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  private readonly auth = inject(AuthService);

  readonly user = computed(() => this.auth.currentUser());

  readonly adminNav: NavItem[] = [
    { label: 'Tenants', route: '/tenants', icon: 'tenants' },
    { label: 'Platform Users', route: '/platform-users', icon: 'users' },
    { label: 'Templates', route: '/document-templates', icon: 'templates' }
  ];

  logout() {
    this.auth.logout();
  }
}
