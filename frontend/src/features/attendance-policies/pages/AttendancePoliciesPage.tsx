import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { AttendancePolicyTable, AttendancePolicyFilters } from '../components';
import { useAttendancePolicies } from '../hooks';
import type { AttendancePolicyFilter } from '../types';

export function AttendancePoliciesPage() {
  const [filters, setFilters] = useState<AttendancePolicyFilter>({
    pageNumber: 1,
    pageSize: 10,
  });

  const { data, isLoading, isError } = useAttendancePolicies(filters);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Attendance Policies</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage attendance rules, timings, and verification settings.
          </p>
        </div>
        <Link to="/attendance-policies/create">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Create Policy
          </Button>
        </Link>
      </div>

      <div className="bg-white p-6 rounded-lg shadow">
        <AttendancePolicyFilters filters={filters} onChange={setFilters} />

        {isError ? (
          <div className="text-center py-12 bg-red-50 rounded-lg border border-red-200">
            <p className="text-red-500">Failed to load attendance policies. Please try again.</p>
          </div>
        ) : (
          <>
            <AttendancePolicyTable policies={data?.items || []} isLoading={isLoading} />
            {data && data.pagination.totalPages > 1 && (
              <div className="flex items-center justify-between pt-4 mt-6 border-t">
                <p className="text-sm text-muted-foreground">
                  Showing page {data.pagination.pageNumber} of {data.pagination.totalPages}
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={!data.pagination.hasPreviousPage}
                    onClick={() => setFilters(f => ({ ...f, pageNumber: (f.pageNumber || 1) - 1 }))}
                  >
                    Previous
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={!data.pagination.hasNextPage}
                    onClick={() => setFilters(f => ({ ...f, pageNumber: (f.pageNumber || 1) + 1 }))}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
