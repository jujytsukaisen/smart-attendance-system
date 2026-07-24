import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ReportFilters, ExportDropdown, DepartmentComparisonChart } from '../components';
import { useDashboardStatistics } from '../hooks';
import type { ReportFilterDto } from '../types';

export function AnalyticsPage() {
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
          <h1 className="text-2xl font-semibold text-gray-900">Advanced Analytics</h1>
          <p className="mt-1 text-sm text-gray-500">Deep-dive comparative analytics across departments and branches.</p>
        </div>
        <ExportDropdown filter={filter} />
      </div>

      <ReportFilters onFilterChange={setFilter} />

      <Card>
        <CardHeader>
          <CardTitle>Department Productivity Comparison</CardTitle>
          <CardDescription>Comparing attendance versus lateness/absence by department.</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
             <Skeleton className="h-[400px]" />
          ) : data ? (
            <div className="pt-4">
               <DepartmentComparisonChart data={data.departmentAttendance} height={400} />
            </div>
          ) : null}
        </CardContent>
      </Card>
    </div>
  );
}
