import { useState } from 'react';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { AuditFilters, AuditTable } from '../components';
import { useAuditLogs } from '../hooks';
import type { AuditFilterDto } from '../types';

export function AuditLogsPage() {
  const [filter, setFilter] = useState<AuditFilterDto>({});
  const { data, isLoading, isError, error } = useAuditLogs(filter);
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
          <h1 className="text-2xl font-semibold text-gray-900">Audit Logs</h1>
          <p className="mt-1 text-sm text-gray-500">Track and monitor all system activities and user actions.</p>
        </div>
      </div>

      <AuditFilters onFilterChange={setFilter} />

      <Card>
        <CardContent className="p-0">
          {isLoading ? (
             <div className="p-6"><Skeleton className="h-96 w-full" /></div>
          ) : (
            <AuditTable data={data?.items || []} />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
