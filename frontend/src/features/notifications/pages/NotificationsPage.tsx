import { useState } from 'react';
import { format } from 'date-fns';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Button } from '@/shared/components/ui/button';
import { useNotifications, useMarkAsRead, useMarkAllAsRead, useDeleteNotification, useDeleteReadNotifications } from '../hooks';
import { NotificationStatus, type NotificationFilterDto } from '../types';
import { useNavigate } from 'react-router-dom';
import { Check, Trash2, MailOpen } from 'lucide-react';

export function NotificationsPage() {
  const [filter, setFilter] = useState<NotificationFilterDto>({});
  const { data, isLoading, isError, error } = useNotifications(filter);
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();
  const deleteNotification = useDeleteNotification();
  const deleteRead = useDeleteReadNotifications();
  const navigate = useNavigate();
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }



  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Notifications</h1>
          <p className="mt-1 text-sm text-gray-500">Manage your alerts, approvals, and system messages.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => markAllAsRead.mutate()} disabled={markAllAsRead.isPending}>
            <Check className="h-4 w-4 mr-2" /> Mark All as Read
          </Button>
          <Button variant="outline" onClick={() => deleteRead.mutate()} disabled={deleteRead.isPending} className="text-red-600 hover:text-red-700">
            <Trash2 className="h-4 w-4 mr-2" /> Delete Read
          </Button>
        </div>
      </div>

      <div className="flex gap-4 mb-4 border-b pb-2">
        <Button variant={filter.status === undefined ? 'default' : 'ghost'} onClick={() => setFilter({})}>All</Button>
        <Button variant={filter.status === NotificationStatus.Unread ? 'default' : 'ghost'} onClick={() => setFilter({ status: NotificationStatus.Unread })}>Unread</Button>
      </div>

      <Card>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="p-4 space-y-4">
              <Skeleton className="h-16 w-full" />
              <Skeleton className="h-16 w-full" />
              <Skeleton className="h-16 w-full" />
            </div>
          ) : data?.items && data.items.length > 0 ? (
            <div className="divide-y">
              {data.items.map((notif) => (
                <div key={notif.id} className={`p-4 flex gap-4 items-start transition-colors hover:bg-gray-50 ${!notif.isRead ? 'bg-blue-50/50' : ''}`}>
                  <div className="flex-1 cursor-pointer" onClick={() => navigate(`/notifications/${notif.id}`)}>
                    <div className="flex justify-between items-start mb-1">
                      <h3 className={`text-sm ${!notif.isRead ? 'font-bold text-gray-900' : 'font-medium text-gray-700'}`}>{notif.title}</h3>
                      <span className="text-xs text-gray-500 whitespace-nowrap ml-4">{format(new Date(notif.createdAt), 'MMM dd, h:mm a')}</span>
                    </div>
                    <p className="text-sm text-gray-600">{notif.message}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    {!notif.isRead && (
                       <Button variant="ghost" size="icon" title="Mark as read" onClick={() => markAsRead.mutate(notif.id)}>
                         <MailOpen className="h-4 w-4 text-gray-400 hover:text-primary" />
                       </Button>
                    )}
                    <Button variant="ghost" size="icon" title="Delete" onClick={() => deleteNotification.mutate(notif.id)}>
                      <Trash2 className="h-4 w-4 text-gray-400 hover:text-red-600" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="p-12 text-center text-gray-500">
              No notifications found.
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
