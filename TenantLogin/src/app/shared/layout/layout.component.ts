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

interface LocationNavItem {
  label: string;
  route: string;
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

  readonly showDashboard = computed(() => this.hasMenu('dashboard'));

  readonly mainNav = computed<NavItem[]>(() => {
    if (!this.hasMenu('my-forms')) return [];
    return [{ label: 'My Forms', route: '/my-forms', icon: 'my-forms' }];
  });

  readonly adminNavBeforeLocation = computed<NavItem[]>(() => {
    const items: NavItem[] = [];
    if (this.hasMenu('users')) {
      items.push({ label: 'Users', route: '/users', icon: 'users' });
    }
    if (this.hasMenu('roles')) {
      items.push({ label: 'Roles', route: '/roles', icon: 'roles' });
    }
    if (this.hasMenu('projects')) {
      items.push({ label: 'Projects', route: '/projects', icon: 'projects' });
    }
    return items;
  });

  readonly adminNavAfterLocation = computed<NavItem[]>(() => {
    const items: NavItem[] = [];
    if (this.hasMenu('forms')) {
      items.push({ label: 'Forms', route: '/forms', icon: 'forms' });
    }
    if (this.hasMenu('templates')) {
      items.push({ label: 'Templates', route: '/document-templates', icon: 'templates' });
    }
    return items;
  });

  readonly showLocationNav = computed(() => this.hasMenu('location'));

  readonly locationNav: LocationNavItem[] = [
    { label: 'State', route: '/locations/states' },
    { label: 'District', route: '/locations/districts' },
    { label: 'Block', route: '/locations/blocks' },
    { label: 'Village', route: '/locations/villages' }
  ];

  readonly locationNavOpen = signal(this.isLocationUrl(this.router.url));
  readonly locationNavActive = signal(this.isLocationUrl(this.router.url));

  readonly showAdminSection = computed(
    () =>
      this.adminNavBeforeLocation().length > 0 ||
      this.adminNavAfterLocation().length > 0 ||
      this.showLocationNav()
  );

  constructor() {
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntilDestroyed()
    ).subscribe(e => {
      const url = (e as NavigationEnd).urlAfterRedirects;
      const onLocation = this.isLocationUrl(url);
      this.locationNavActive.set(onLocation);
      if (onLocation) {
        this.locationNavOpen.set(true);
      }
      this.closeMobileSidebar();
      this.closeUserMenu();
    });
  }

  /** Tenant super admin sees all admin menus; staff see only assigned menu keys. */
  hasMenu(key: string): boolean {
    const u = this.user();
    if (!u) return false;
    if (u.role === 'TenantSuperAdmin') return true;
    return (u.menus ?? []).includes(key);
  }

  toggleLocationNav() {
    this.locationNavOpen.update(v => !v);
  }

  private isLocationUrl(url: string): boolean {
    return url.includes('/locations/');
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
