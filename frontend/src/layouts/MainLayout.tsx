import { useState } from 'react';
import { Outlet, NavLink, useLocation, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Clock,
  ScanFace,
  Camera,
  Users,
  CalendarOff,
  Building2,
  CalendarDays,
  MapPin,
  FileText,
  Settings,
  LogOut,
  Menu,
  X,
  ArrowRight,
  ArrowLeft,
  ShieldCheck,
  User as UserIcon,
  Building,
} from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { NotificationDropdown } from '@/features/notifications/components';
import { LanguageSwitcher } from '@/shared/components/LanguageSwitcher';
import { ThemeToggle } from '@/shared/components/ThemeToggle';
import { useAuthStore } from '@/features/auth/store';
import { useTranslation } from '@/shared/i18n';
import { cn } from '@/shared/utils';
import { useGeneralSettings } from '@/features/settings/hooks';

export const MainLayout = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();
  const { t, isRtl } = useTranslation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const { data: generalSettings } = useGeneralSettings();
  const companyName = generalSettings?.companyName || t('common.appName');

  const isDashboard = location.pathname === '/dashboard' || location.pathname === '/';

  const isAdmin = user?.roles?.includes('Admin');
  const isEmployeeOnly = !isAdmin;

  const navItems = [
    { key: 'dashboard', label: t('nav.dashboard'), href: '/dashboard', icon: LayoutDashboard, isPublic: true },
    { key: 'attendance', label: t('nav.attendance'), href: '/attendance', icon: Clock, isPublic: true },
    { key: 'faceCheckIn', label: t('nav.faceCheckIn'), href: '/face-check-in', icon: ScanFace, isPublic: false },
    { key: 'autoKiosk', label: t('nav.autoKiosk'), href: '/auto-kiosk', icon: Camera, isPublic: false },
    { key: 'companies', label: t('nav.companies') || 'Companies', href: '/companies', icon: Building, isPublic: false },
    { key: 'employees', label: t('nav.employees'), href: '/employees', icon: Users, isPublic: false },
    { key: 'leaves', label: t('nav.leaves'), href: '/leaves', icon: CalendarOff, isPublic: true },
    { key: 'departments', label: t('nav.departments'), href: '/departments', icon: Building2, isPublic: false },
    { key: 'shifts', label: t('nav.shifts'), href: '/shifts', icon: CalendarDays, isPublic: false },
    { key: 'attendancePolicies', label: isRtl ? 'سياسات الحضور' : 'Attendance Policies', href: '/attendance-policies', icon: FileText, isPublic: false },
    { key: 'branches', label: t('nav.branches'), href: '/branches', icon: MapPin, isPublic: false },
    { key: 'reports', label: t('nav.reports'), href: '/reports', icon: FileText, isPublic: false },
    { key: 'users', label: t('nav.users') || 'Users', href: '/users', icon: ShieldCheck, isPublic: false },

    { key: 'settings', label: t('nav.settings'), href: '/settings', icon: Settings, isPublic: false },
  ].filter(item => !isEmployeeOnly || item.isPublic);

  const userDisplayName = user
    ? `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email
    : t('auth.userProfile');

  const userRole = user?.roles?.length ? user.roles.join(', ') : t('auth.adminRole');

  // Get current active item for page title & breadcrumbs
  const currentNavItem = navItems.find((item) =>
    item.href === '/dashboard'
      ? location.pathname === '/dashboard'
      : location.pathname.startsWith(item.href)
  );

  const handleLogout = () => {
    logout();
    navigate('/auth/login');
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div className="flex min-h-screen w-full bg-muted/30">
      {/* Mobile Backdrop */}
      {isMobileMenuOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 backdrop-blur-xs md:hidden"
          onClick={() => setIsMobileMenuOpen(false)}
        />
      )}

      {/* Desktop & Mobile Sidebar */}
      <aside
        className={cn(
          'fixed inset-y-0 z-50 flex flex-col bg-card shadow-sm transition-transform duration-200 ease-in-out md:translate-x-0 md:w-64',
          isRtl ? 'right-0 border-l' : 'left-0 border-r',
          isMobileMenuOpen
            ? 'translate-x-0 w-72'
            : isRtl
            ? 'translate-x-full md:translate-x-0'
            : '-translate-x-full md:translate-x-0'
        )}
      >
        {/* Sidebar Brand Header */}
        <div className="flex h-16 items-center justify-between border-b px-4">
          <div
            onClick={() => {
              navigate('/dashboard');
              setIsMobileMenuOpen(false);
            }}
            className="flex items-center gap-3 cursor-pointer select-none group"
          >
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md transition-transform group-hover:scale-105">
              <ShieldCheck className="h-6 w-6" />
            </div>
            <div className="flex flex-col">
              <span className="text-sm font-bold tracking-tight text-foreground">
                {companyName}
              </span>
              <span className="text-[11px] text-muted-foreground">
                {t('common.appSubtitle')}
              </span>
            </div>
          </div>

          {/* Close button on mobile */}
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => setIsMobileMenuOpen(false)}
          >
            <X className="h-5 w-5" />
          </Button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 overflow-y-auto p-3 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive =
              item.href === '/dashboard'
                ? location.pathname === '/dashboard' || location.pathname === '/'
                : location.pathname.startsWith(item.href);

            return (
              <NavLink
                key={item.href}
                to={item.href}
                onClick={() => setIsMobileMenuOpen(false)}
                className={cn(
                  'flex items-center justify-between rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground shadow-xs font-semibold'
                    : 'text-muted-foreground hover:bg-accent hover:text-foreground'
                )}
              >
                <div className="flex items-center gap-3">
                  <Icon className={cn('h-4 w-4 shrink-0', isActive ? 'text-primary-foreground' : 'text-muted-foreground')} />
                  <span>{item.label}</span>
                </div>
              </NavLink>
            );
          })}
        </nav>

        {/* Sidebar Footer User Info */}
        <div className="border-t p-3 bg-muted/20">
          <div className="flex items-center justify-between rounded-lg p-2 bg-card border shadow-2xs">
            <div className="flex items-center gap-2.5 overflow-hidden">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary font-bold text-xs uppercase">
                {user?.firstName?.charAt(0) || user?.email?.charAt(0) || 'U'}
              </div>
              <div className="flex flex-col truncate">
                <span className="text-xs font-semibold truncate text-foreground">
                  {userDisplayName}
                </span>
                <span className="text-[10px] text-muted-foreground truncate">
                  {userRole}
                </span>
              </div>
            </div>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
              title={t('auth.logout')}
              onClick={handleLogout}
            >
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </aside>

      {/* Main Content Area */}
      <div
        className={cn(
          'flex flex-1 flex-col transition-[padding] duration-200',
          isRtl ? 'md:pr-64' : 'md:pl-64'
        )}
      >
        {/* Top Header Bar */}
        <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b bg-card/95 backdrop-blur-sm px-4 sm:px-6 shadow-2xs">
          <div className="flex items-center gap-3">
            {/* Mobile Hamburger Menu Toggle */}
            <Button
              variant="outline"
              size="icon"
              className="md:hidden h-9 w-9"
              onClick={() => setIsMobileMenuOpen(true)}
              aria-label="Toggle Menu"
            >
              <Menu className="h-5 w-5" />
            </Button>

            {/* Back to Dashboard Button (Shown when not on Dashboard) */}
            {!isDashboard ? (
              <Button
                variant="outline"
                size="sm"
                className="gap-2 font-medium hover:bg-primary hover:text-primary-foreground transition-colors shadow-2xs"
                onClick={() => navigate('/dashboard')}
              >
                <BackIcon className="h-4 w-4" />
                <span className="hidden sm:inline">{t('common.backToDashboard')}</span>
                <span className="sm:hidden">{t('common.home')}</span>
              </Button>
            ) : (
              <div className="flex items-center gap-2 text-sm font-semibold text-foreground">
                <LayoutDashboard className="h-4 w-4 text-primary" />
                <span>{t('dashboard.title')}</span>
              </div>
            )}

            {/* Breadcrumb / Page Title */}
            {currentNavItem && !isDashboard && (
              <div className="hidden sm:flex items-center gap-1.5 text-xs text-muted-foreground">
                <span>/</span>
                <span className="font-semibold text-foreground">{currentNavItem.label}</span>
              </div>
            )}
          </div>

          {/* Header Right Actions */}
          <div className="flex items-center gap-2 sm:gap-3">
            {/* Theme Toggle */}
            <ThemeToggle />

            {/* Language Switcher Dropdown */}
            <LanguageSwitcher />

            {/* Notifications */}
            <NotificationDropdown />

            {/* User Dropdown Menu */}
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex h-9 w-9 items-center justify-center rounded-full border bg-muted/40 hover:bg-muted transition-colors cursor-pointer outline-none">
                <UserIcon className="h-4 w-4" />
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel className="font-normal">
                  <div className="flex flex-col space-y-1">
                    <p className="text-sm font-medium leading-none">{userDisplayName}</p>
                    <p className="text-xs leading-none text-muted-foreground">{user?.email}</p>
                  </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => navigate('/dashboard')} className="cursor-pointer gap-2">
                  <LayoutDashboard className="h-4 w-4" />
                  {t('nav.dashboard')}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => navigate('/attendance')} className="cursor-pointer gap-2">
                  <Clock className="h-4 w-4" />
                  {t('nav.attendance')}
                </DropdownMenuItem>
                {!isEmployeeOnly && (
                  <DropdownMenuItem onClick={() => navigate('/settings')} className="cursor-pointer gap-2">
                    <Settings className="h-4 w-4" />
                    {t('nav.settings')}
                  </DropdownMenuItem>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout} className="cursor-pointer gap-2 text-destructive focus:text-destructive">
                  <LogOut className="h-4 w-4" />
                  {t('auth.logout')}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 p-4 sm:p-6 md:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default MainLayout;
