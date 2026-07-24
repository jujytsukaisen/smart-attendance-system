import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ReportFilters, ExportDropdown, ReportDataTable, type ColumnDef } from '../components';
import { useLeaveReport } from '../hooks';
import type { ReportFilterDto, LeaveReportDto } from '../types';

export function LeaveReportsPage() {
  const [filter, setFilter] = useState<ReportFilterDto>({});
  const { data, isLoading, isError, error } = useLeaveReport(filter);

  const columns: ColumnDef<LeaveReportDto>[] = [
    { header: 'Employee', accessorKey: 'employeeName', sortable: true },
    { header: 'Type', accessorKey: 'leaveType', sortable: true },
    { header: 'Start Date', accessorKey: 'startDate', sortable: true },
    { header: 'End Date', accessorKey: 'endDate', sortable: true },
    { header: 'Total Days', accessorKey: 'totalDays', sortable: true },
    { header: 'Status', accessorKey: 'status', sortable: true },
  ];
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
          <h1 className="text-2xl font-semibold text-gray-900">Leave Reports</h1>
          <p className="mt-1 text-sm text-gray-500">Comprehensive leave history and status tracking.</p>
        </div>
        <ExportDropdown filter={filter} />
      </div>

      <ReportFilters onFilterChange={setFilter} />

      <Card>
        <CardHeader>
          <CardTitle>Leave Requests Log</CardTitle>
          <CardDescription>All leave requests matching the current filters.</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? <Skeleton className="h-96" /> : <ReportDataTable data={data || []} columns={columns} />}
        </CardContent>
      </Card>
    </div>
  );
}
