import { NavLink } from 'react-router-dom';
import { 
  Settings, 
  Clock, 
  Shield, 
  Bell, 
  Smartphone, 
  Mail, 
  MessageSquare, 
  Database, 
  ScanFace 
} from 'lucide-react';
import { cn } from '@/shared/utils';
import { useTranslation } from '@/shared/i18n';

export function SettingsSidebar() {
  const { t } = useTranslation();

  const items = [
    { title: t('settings.general'), href: '/settings/general', icon: Settings },
    { title: t('settings.attendance'), href: '/settings/attendance', icon: Clock },
    { title: t('settings.security'), href: '/settings/security', icon: Shield },
    { title: t('settings.notifications'), href: '/settings/notifications', icon: Bell },
    { title: t('settings.devices'), href: '/settings/devices', icon: Smartphone },
    { title: t('settings.email'), href: '/settings/email', icon: Mail },
    { title: t('settings.sms'), href: '/settings/sms', icon: MessageSquare },
    { title: t('settings.backup'), href: '/settings/backup', icon: Database },
    { title: t('settings.faceRecognition'), href: '/settings/face-recognition', icon: ScanFace },
  ];

  return (
    <nav className="flex space-x-2 lg:flex-col lg:space-x-0 lg:space-y-1">
      {items.map((item) => (
        <NavLink
          key={item.href}
          to={item.href}
          className={({ isActive }) =>
            cn(
              "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-muted hover:text-primary",
              isActive ? "bg-muted text-primary font-semibold" : "text-muted-foreground"
            )
          }
        >
          <item.icon className="h-4 w-4" />
          {item.title}
        </NavLink>
      ))}
    </nav>
  );
}
