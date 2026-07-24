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

const items = [
  { title: 'General', href: '/settings/general', icon: Settings },
  { title: 'Attendance', href: '/settings/attendance', icon: Clock },
  { title: 'Security', href: '/settings/security', icon: Shield },
  { title: 'Notifications', href: '/settings/notifications', icon: Bell },
  { title: 'Devices', href: '/settings/devices', icon: Smartphone },
  { title: 'Email', href: '/settings/email', icon: Mail },
  { title: 'SMS', href: '/settings/sms', icon: MessageSquare },
  { title: 'Backup', href: '/settings/backup', icon: Database },
  { title: 'Face Recognition', href: '/settings/face-recognition', icon: ScanFace },
];

export function SettingsSidebar() {
  return (
    <nav className="flex space-x-2 lg:flex-col lg:space-x-0 lg:space-y-1">
      {items.map((item) => (
        <NavLink
          key={item.href}
          to={item.href}
          className={({ isActive }) =>
            cn(
              "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-muted hover:text-primary",
              isActive ? "bg-muted text-primary" : "text-muted-foreground"
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
