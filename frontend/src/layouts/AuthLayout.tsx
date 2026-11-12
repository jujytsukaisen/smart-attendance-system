import { Outlet } from 'react-router-dom';
import { LanguageSwitcher } from '@/shared/components/LanguageSwitcher';
import { ShieldCheck } from 'lucide-react';
import { useTranslation } from '@/shared/i18n';

const AuthLayout = () => {
  const { t } = useTranslation();

  return (
    <div className="flex min-h-screen flex-col bg-muted/40 p-4">
      {/* Top Header for Auth Screen */}
      <header className="flex w-full items-center justify-between py-2 px-4">
        <div className="flex items-center gap-2">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md">
            <ShieldCheck className="h-5 w-5" />
          </div>
          <span className="font-bold text-foreground tracking-tight text-sm">
            {t('common.appName')}
          </span>
        </div>
        <LanguageSwitcher />
      </header>

      {/* Auth Content */}
      <div className="flex flex-1 items-center justify-center">
        <div className="w-full max-w-sm md:max-w-md lg:max-w-lg">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
