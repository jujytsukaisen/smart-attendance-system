import { useState } from 'react';
import { Users, UserCheck, UserX, Clock, CalendarDays } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { 
  ReportFilters, 
  ExportDropdown, 
  AttendanceTrendChart, 
  LeaveDistributionChart, 
  DepartmentComparisonChart 
} from '../components';
import { useDashboardStatistics } from '../hooks';
import type { ReportFilterDto } from '../types';
import { useTranslation } from '@/shared/i18n';

export function ReportsDashboardPage() {
  const { t } = useTranslation();
  const [filter, setFilter] = useState<ReportFilterDto>({});
  const { data, isLoading, isError, error } = useDashboardStatistics(filter);
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : t('common.error')}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('reports.title')}</h1>
          <p className="mt-1 text-sm text-muted-foreground">{t('reports.subtitle')}</p>
        </div>
        <ExportDropdown filter={filter} />
      </div>

      <ReportFilters onFilterChange={setFilter} showSearch={false} />

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <Skeleton className="h-32" />
          <Skeleton className="h-32" />
          <Skeleton className="h-32" />
          <Skeleton className="h-32" />
          <Skeleton className="h-80 md:col-span-2 lg:col-span-4" />
        </div>
      ) : data ? (
        <>
          {/* Key Metrics */}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{t('reports.totalStaff')}</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.totalEmployees}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{t('reports.presentToday')}</CardTitle>
                <UserCheck className="h-4 w-4 text-emerald-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">{data.presentToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{t('reports.absentToday')}</CardTitle>
                <UserX className="h-4 w-4 text-rose-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-rose-600 dark:text-rose-400">{data.absentToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{t('reports.lateArrivals')}</CardTitle>
                <Clock className="h-4 w-4 text-amber-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-amber-600 dark:text-amber-400">{data.lateToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{t('reports.onLeave')}</CardTitle>
                <CalendarDays className="h-4 w-4 text-sky-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-sky-600 dark:text-sky-400">{data.onLeaveToday}</div>
              </CardContent>
            </Card>
          </div>

          {/* Charts Row 1 */}
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>{t('reports.attendanceTrend')}</CardTitle>
                <CardDescription>{t('reports.attendanceTrendSubtitle')}</CardDescription>
              </CardHeader>
              <CardContent>
                <AttendanceTrendChart data={data.weeklyTrend || []} />
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>{t('reports.leaveDistribution')}</CardTitle>
                <CardDescription>{t('reports.leaveDistributionSubtitle')}</CardDescription>
              </CardHeader>
              <CardContent>
                <LeaveDistributionChart data={data.leaveDistribution || []} />
              </CardContent>
            </Card>
          </div>

          {/* Charts Row 2 */}
          <div className="grid grid-cols-1 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>{t('reports.departmentComparison')}</CardTitle>
                <CardDescription>{t('reports.departmentComparisonSubtitle')}</CardDescription>
              </CardHeader>
              <CardContent>
                <DepartmentComparisonChart data={data.departmentAttendance || []} />
              </CardContent>
            </Card>
          </div>
        </>
      ) : null}
    </div>
  );
}
