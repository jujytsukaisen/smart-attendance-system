import { Bell } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useUnreadCount } from '../hooks';

interface NotificationBellProps {
  onClick?: () => void;
}

export function NotificationBell({ onClick }: NotificationBellProps) {
  const { data: unreadCount = 0 } = useUnreadCount();

  return (
    <Button variant="ghost" size="icon" className="relative" onClick={onClick}>
      <Bell className="h-5 w-5" />
      {unreadCount > 0 && (
        <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </Button>
  );
}
