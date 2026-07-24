import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Activity } from 'lucide-react';
import type { RecentActivity } from '../types';

interface RecentActivitiesProps {
  activities: RecentActivity[];
}

export const RecentActivities = ({ activities }: RecentActivitiesProps) => {
  return (
    <Card className="col-span-1 md:col-span-2 lg:col-span-3">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Activity className="h-5 w-5" />
          Recent Activities
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-8">
          {activities.map((activity) => (
            <div key={activity.id} className="flex items-center">
              <span className="relative flex h-2 w-2 mr-4 flex-shrink-0">
                <span
                  className={`animate-ping absolute inline-flex h-full w-full rounded-full opacity-75 ${
                    activity.type === 'attendance'
                      ? 'bg-green-400'
                      : activity.type === 'leave'
                      ? 'bg-amber-400'
                      : 'bg-blue-400'
                  }`}
                ></span>
                <span
                  className={`relative inline-flex rounded-full h-2 w-2 ${
                    activity.type === 'attendance'
                      ? 'bg-green-500'
                      : activity.type === 'leave'
                      ? 'bg-amber-500'
                      : 'bg-blue-500'
                  }`}
                ></span>
              </span>
              <div className="space-y-1">
                <p className="text-sm font-medium leading-none">{activity.message}</p>
                <p className="text-sm text-muted-foreground">
                  {new Date(activity.timestamp).toLocaleString()}
                </p>
              </div>
            </div>
          ))}
          {activities.length === 0 && (
            <p className="text-sm text-muted-foreground text-center py-4">
              No recent activities found.
            </p>
          )}
        </div>
      </CardContent>
    </Card>
  );
};
