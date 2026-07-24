import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { ReportFilters, ExportDropdown } from '../components';
import type { ReportFilterDto } from '../types';

export function EmployeeReportsPage() {
  const [filter, setFilter] = useState<ReportFilterDto>({});

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Employee Reports</h1>
          <p className="mt-1 text-sm text-gray-500">Individual attendance and performance history.</p>
        </div>
        <ExportDropdown filter={filter} />
      </div>

      <ReportFilters onFilterChange={setFilter} />

      <Card>
        <CardHeader>
          <CardTitle>Detailed Employee History</CardTitle>
          <CardDescription>Select an employee from the filters to view their detailed timeline.</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center h-64 text-gray-500">
            <p>Please use the search or select an employee above to generate the report.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
