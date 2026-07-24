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

export function ReportsDashboardPage() {
  const [filter, setFilter] = useState<ReportFilterDto>({});
  const { data, isLoading, isError, error } = useDashboardStatistics(filter);
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
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Reports & Analytics</h1>
          <p className="mt-1 text-sm text-gray-500">Comprehensive overview of attendance metrics.</p>
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
          <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-5">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Total Staff</CardTitle>
                <Users className="h-4 w-4 text-gray-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.totalEmployees}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Present Today</CardTitle>
                <UserCheck className="h-4 w-4 text-green-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.presentToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Absent</CardTitle>
                <UserX className="h-4 w-4 text-red-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.absentToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Late Arrivals</CardTitle>
                <Clock className="h-4 w-4 text-yellow-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.lateToday}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">On Leave</CardTitle>
                <CalendarDays className="h-4 w-4 text-blue-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{data.onLeaveToday}</div>
              </CardContent>
            </Card>
          </div>

          {/* Charts Row 1 */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>Attendance Trend (Last 7 Days)</CardTitle>
                <CardDescription>Daily present vs absent vs late</CardDescription>
              </CardHeader>
              <CardContent>
                <AttendanceTrendChart data={data.weeklyTrend} />
              </CardContent>
            </Card>
            
            <Card>
              <CardHeader>
                <CardTitle>Leave Distribution</CardTitle>
                <CardDescription>Breakdown of current leaves</CardDescription>
              </CardHeader>
              <CardContent>
                <LeaveDistributionChart data={data.leaveDistribution} />
              </CardContent>
            </Card>
          </div>

          {/* Charts Row 2 */}
          <Card>
            <CardHeader>
              <CardTitle>Department Comparison</CardTitle>
              <CardDescription>Attendance metrics by department</CardDescription>
            </CardHeader>
            <CardContent>
              <DepartmentComparisonChart data={data.departmentAttendance} />
            </CardContent>
          </Card>
        </>
      ) : null}
    </div>
  );
}
