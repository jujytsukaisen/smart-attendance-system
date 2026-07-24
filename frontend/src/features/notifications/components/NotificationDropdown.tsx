import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Bell, Check } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
  DropdownMenuLabel,
} from '@/shared/components/ui/dropdown-menu';
import { NotificationBell } from './NotificationBell';
import { useUnreadNotifications, useMarkAllAsRead, useMarkAsRead } from '../hooks';
import { formatDistanceToNow } from 'date-fns';

export function NotificationDropdown() {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();
  const { data, isLoading } = useUnreadNotifications();
  const markAllMutation = useMarkAllAsRead();
  const markAsReadMutation = useMarkAsRead();

  const handleMarkAllRead = async () => {
    await markAllMutation.mutateAsync();
  };

  const handleNotificationClick = async (id: string) => {
    await markAsReadMutation.mutateAsync(id);
    setOpen(false);
    navigate(`/notifications/${id}`);
  };

  const handleViewAll = () => {
    setOpen(false);
    navigate('/notifications');
  };

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger>
        <div className="cursor-pointer">
          <NotificationBell />
        </div>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-80">
        <DropdownMenuLabel className="flex justify-between items-center">
          <span>Notifications</span>
          {(data?.unreadCount || 0) > 0 && (
            <Button variant="ghost" size="sm" onClick={handleMarkAllRead} disabled={markAllMutation.isPending} className="h-auto p-1 text-xs">
              <Check className="mr-1 h-3 w-3" /> Mark all read
            </Button>
          )}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <div className="max-h-80 overflow-y-auto">
          {isLoading ? (
            <div className="p-4 text-center text-sm text-gray-500">Loading...</div>
          ) : data?.items && data.items.length > 0 ? (
            data.items.map((notification) => (
              <DropdownMenuItem 
                key={notification.id} 
                className="flex flex-col items-start p-3 cursor-pointer border-b last:border-0"
                onClick={() => handleNotificationClick(notification.id)}
              >
                <div className="flex justify-between w-full mb-1">
                  <span className="font-medium text-sm text-gray-900">{notification.title}</span>
                  <span className="text-xs text-gray-500">
                    {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                  </span>
                </div>
                <span className="text-xs text-gray-600 line-clamp-2">{notification.message}</span>
              </DropdownMenuItem>
            ))
          ) : (
            <div className="p-4 text-center flex flex-col items-center">
              <Bell className="h-8 w-8 text-gray-300 mb-2" />
              <span className="text-sm text-gray-500">No unread notifications</span>
            </div>
          )}
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem className="justify-center text-primary font-medium cursor-pointer" onClick={handleViewAll}>
          View all notifications
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
