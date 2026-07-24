import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ReportFilters, ExportDropdown, ReportDataTable, type ColumnDef } from '../components';
import { useLateEmployeesReport, useAbsenceReport } from '../hooks';
import type { ReportFilterDto, LateEmployeesReportDto, AbsenceReportDto } from '../types';

export function AttendanceReportsPage() {
  const [filter, setFilter] = useState<ReportFilterDto>({});
  
  const { data: lateData, isLoading: lateLoading, isError: isError1, error: error1 } = useLateEmployeesReport(filter);
  const { data: absenceData, isLoading: absenceLoading, isError: isError2, error: error2 } = useAbsenceReport(filter);

  const lateColumns: ColumnDef<LateEmployeesReportDto>[] = [
    { header: 'Employee', accessorKey: 'employeeName', sortable: true },
    { header: 'Department', accessorKey: 'department', sortable: true },
    { header: 'Total Late Min', accessorKey: 'totalLateMinutes', sortable: true },
    { header: 'Incidents', accessorKey: 'lateDaysCount', sortable: true },
  ];

  const absenceColumns: ColumnDef<AbsenceReportDto>[] = [
    { header: 'Employee', accessorKey: 'employeeName', sortable: true },
    { header: 'Department', accessorKey: 'department', sortable: true },
    { header: 'Days Absent', accessorKey: 'absentDaysCount', sortable: true },
    { header: 'Reason', accessorKey: 'reason' },
  ];
  if (isError1 || isError2) {
    const currentError = error1 ?? error2;
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {currentError instanceof Error && !currentError.message.toLowerCase().includes('internal server error') && !currentError.message.includes('status code')
            ? currentError.message
            : "Failed to load"}
        </p>
      </div>
    );
  }



  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Attendance Deep Dive</h1>
          <p className="mt-1 text-sm text-gray-500">Detailed logs for late arrivals and absences.</p>
        </div>
        <ExportDropdown filter={filter} />
      </div>

      <ReportFilters onFilterChange={setFilter} />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Late Employees</CardTitle>
            <CardDescription>Staff with highest accumulated late minutes</CardDescription>
          </CardHeader>
          <CardContent>
            {lateLoading ? <Skeleton className="h-64" /> : <ReportDataTable data={lateData || []} columns={lateColumns} />}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Frequent Absences</CardTitle>
            <CardDescription>Staff with highest absent days</CardDescription>
          </CardHeader>
          <CardContent>
            {absenceLoading ? <Skeleton className="h-64" /> : <ReportDataTable data={absenceData || []} columns={absenceColumns} />}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
