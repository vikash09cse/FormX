import { Component, computed, ElementRef, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: 'dashboard' | 'users' | 'templates' | 'roles' | 'projects' | 'forms' | 'my-forms';
}

const SIDEBAR_COLLAPSED_KEY = 'tenant_sidebar_collapsed';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly mobileSidebarOpen = signal(false);
  readonly sidebarCollapsed = signal(this.loadCollapsedPreference());
  readonly userMenuOpen = signal(false);
  readonly user = computed(() => this.auth.currentUser());

  readonly mainNav = computed<NavItem[]>(() => [
    { label: 'My Forms', route: '/my-forms', icon: 'my-forms' }
  ]);

  readonly dashboardItem: NavItem = { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' };

  readonly adminNav = computed<NavItem[]>(() => {
    if (this.user()?.role !== 'TenantSuperAdmin') return [];

    return [
      { label: 'Users', route: '/users', icon: 'users' },
      { label: 'Roles', route: '/roles', icon: 'roles' },
      { label: 'Projects', route: '/projects', icon: 'projects' },
      { label: 'Forms', route: '/forms', icon: 'forms' },
      { label: 'Templates', route: '/document-templates', icon: 'templates' }
    ];
  });

  readonly showAdminSection = computed(() => this.adminNav().length > 0);

  constructor() {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntilDestroyed()
    ).subscribe(() => {
      this.closeMobileSidebar();
      this.closeUserMenu();
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.userMenuOpen()) return;
    const target = event.target as Node | null;
    if (target && !this.host.nativeElement.querySelector('.user-menu')?.contains(target)) {
      this.closeUserMenu();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.closeUserMenu();
  }

  menuAriaLabel(): string {
    if (this.isMobile()) {
      return this.mobileSidebarOpen() ? 'Close menu' : 'Open menu';
    }
    return this.sidebarCollapsed() ? 'Expand menu' : 'Collapse menu';
  }

  isMobile(): boolean {
    return typeof window !== 'undefined' && window.matchMedia('(max-width: 767px)').matches;
  }

  toggleMenu() {
    if (this.isMobile()) {
      this.mobileSidebarOpen.update(v => !v);
      return;
    }
    this.toggleCollapse();
  }

  toggleCollapse() {
    this.sidebarCollapsed.update(v => {
      const next = !v;
      sessionStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(next));
      return next;
    });
  }

  closeMobileSidebar() {
    this.mobileSidebarOpen.set(false);
  }

  onNavClick() {
    if (this.isMobile()) {
      this.closeMobileSidebar();
    }
  }

  toggleUserMenu(event: Event) {
    event.stopPropagation();
    this.userMenuOpen.update(v => !v);
  }

  closeUserMenu() {
    this.userMenuOpen.set(false);
  }

  logout() {
    this.closeUserMenu();
    this.auth.logout();
  }

  private loadCollapsedPreference(): boolean {
    if (typeof window === 'undefined') return false;
    const saved = sessionStorage.getItem(SIDEBAR_COLLAPSED_KEY);
    if (saved !== null) return saved === 'true';
    return window.matchMedia('(min-width: 768px) and (max-width: 1199px)').matches;
  }
}
