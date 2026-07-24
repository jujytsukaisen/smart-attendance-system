import { useState } from 'react';
import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { AttendanceTable, AttendanceFilters } from '../components';
import { useAttendances } from '../hooks';
import type { AttendanceFilter } from '../types';

export function AttendancePage() {
  const navigate = useNavigate();
  const [filter, setFilter] = useState<AttendanceFilter>({
    pageNumber: 1,
    pageSize: 10,
  });

  const { data, isLoading, isError } = useAttendances(filter);

  const handleFilterChange = (newFilters: Partial<AttendanceFilter>) => {
    setFilter((prev) => ({ ...prev, ...newFilters, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  if (isError) {
    return (
      <div className="flex justify-center items-center h-64">
        <p className="text-red-500">Failed to load attendance records. Please try again.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Attendance</h1>
          <p className="mt-1 text-sm text-gray-500">
            Monitor and manage employee check-ins and check-outs.
          </p>
        </div>
        <Button onClick={() => navigate('/attendance/manual')}>
          <Plus className="mr-2 h-4 w-4" />
          Manual Entry
        </Button>
      </div>

      <AttendanceFilters onFilterChange={handleFilterChange} />

      {isLoading ? (
        <div className="space-y-4">
          <div className="h-10 bg-gray-200 rounded animate-pulse" />
          <div className="h-20 bg-gray-200 rounded animate-pulse" />
          <div className="h-20 bg-gray-200 rounded animate-pulse" />
        </div>
      ) : data?.items.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow">
          <h3 className="mt-2 text-sm font-semibold text-gray-900">No attendance records found</h3>
          <p className="mt-1 text-sm text-gray-500">Adjust your filters or add a manual entry.</p>
        </div>
      ) : (
        <>
          <AttendanceTable data={data?.items || []} />
          
          {data?.pagination && data.pagination.totalPages > 1 && (
            <div className="flex justify-between items-center bg-white px-4 py-3 border-t border-gray-200 sm:px-6 rounded-lg shadow-sm mt-4">
              <div className="flex justify-between flex-1 sm:hidden">
                <Button
                  onClick={() => handlePageChange(data.pagination.pageNumber - 1)}
                  disabled={!data.pagination.hasPreviousPage}
                  variant="outline"
                >
                  Previous
                </Button>
                <Button
                  onClick={() => handlePageChange(data.pagination.pageNumber + 1)}
                  disabled={!data.pagination.hasNextPage}
                  variant="outline"
                >
                  Next
                </Button>
              </div>
              <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm text-gray-700">
                    Showing page <span className="font-medium">{data.pagination.pageNumber}</span> of{' '}
                    <span className="font-medium">{data.pagination.totalPages}</span>
                  </p>
                </div>
                <div>
                  <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    <Button
                      onClick={() => handlePageChange(data.pagination.pageNumber - 1)}
                      disabled={!data.pagination.hasPreviousPage}
                      variant="outline"
                      className="rounded-l-md rounded-r-none"
                    >
                      Previous
                    </Button>
                    <Button
                      onClick={() => handlePageChange(data.pagination.pageNumber + 1)}
                      disabled={!data.pagination.hasNextPage}
                      variant="outline"
                      className="rounded-l-none rounded-r-md"
                    >
                      Next
                    </Button>
                  </nav>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
