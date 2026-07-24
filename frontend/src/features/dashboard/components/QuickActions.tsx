import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { PlusCircle, FileText, Settings, Users, Bell } from 'lucide-react';
import type { Notification } from '../types';

interface QuickActionsProps {
  notifications: Notification[];
}

export const QuickActions = ({ notifications }: QuickActionsProps) => {
  const unreadCount = notifications.filter(n => !n.read).length;

  return (
    <Card className="col-span-1">
      <CardHeader>
        <CardTitle>Quick Actions</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4">
        <Button variant="outline" className="w-full justify-start">
          <PlusCircle className="mr-2 h-4 w-4" />
          Add Employee
        </Button>
        <Button variant="outline" className="w-full justify-start">
          <FileText className="mr-2 h-4 w-4" />
          Generate Report
        </Button>
        <Button variant="outline" className="w-full justify-start">
          <Users className="mr-2 h-4 w-4" />
          Manage Leaves
        </Button>
        <Button variant="outline" className="w-full justify-start">
          <Settings className="mr-2 h-4 w-4" />
          System Settings
        </Button>
        <div className="pt-4 border-t">
          <h4 className="text-sm font-medium mb-3 flex items-center">
            <Bell className="mr-2 h-4 w-4" />
            Notifications
            {unreadCount > 0 && (
              <span className="ml-2 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-[10px] text-primary-foreground">
                {unreadCount}
              </span>
            )}
          </h4>
          <div className="space-y-3">
            {notifications.map((notification) => (
              <div key={notification.id} className="text-sm">
                <p className={`font-medium ${!notification.read ? 'text-foreground' : 'text-muted-foreground'}`}>
                  {notification.title}
                </p>
                <p className="text-xs text-muted-foreground mt-1 line-clamp-2">
                  {notification.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
