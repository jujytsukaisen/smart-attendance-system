import { useDashboardData } from '../hooks';
import { useDashboardStatistics } from '@/features/reports/hooks';
import { StatisticsGrid } from '../components/StatisticsGrid';
import { RecentActivities } from '../components/RecentActivities';
import { QuickActions } from '../components/QuickActions';
import { Loader2 } from 'lucide-react';
import { useAuthStore } from '@/features/auth';

export const DashboardPage = () => {
  const { user } = useAuthStore();
  const { data, isLoading: dashboardLoading } = useDashboardData();
  const { data: statsData, isLoading: statsLoading } = useDashboardStatistics();

  const isLoading = dashboardLoading || statsLoading;

  if (isLoading) {
    return (
      <div className="flex h-full w-full items-center justify-center p-8">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex h-full w-full flex-col items-center justify-center p-8 text-center">
        <h2 className="text-2xl font-semibold mb-2">Failed to load dashboard</h2>
        <p className="text-muted-foreground">{'An unknown error occurred.'}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight">Welcome back, {user?.firstName || 'User'}!</h2>
        <p className="text-muted-foreground">
          Here is an overview of the attendance system today.
        </p>
      </div>

      <StatisticsGrid stats={{
        ...data.stats,
        totalEmployees: statsData?.totalEmployees || data.stats.totalEmployees,
        todaysAttendance: statsData?.presentToday || data.stats.todaysAttendance,
        lateEmployees: statsData?.lateToday || data.stats.lateEmployees,
        activeLeaves: statsData?.onLeaveToday || data.stats.activeLeaves,
      }} />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <RecentActivities activities={data.recentActivities} />
        <QuickActions notifications={data.notifications} />
      </div>
    </div>
  );
};
