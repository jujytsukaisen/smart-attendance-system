import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DepartmentTable, DepartmentFilters } from '../components';
import { useDepartments } from '../hooks';
import type { DepartmentFilter } from '../types';

export const DepartmentsPage = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<DepartmentFilter>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
  });

  const { data, isLoading, isError, error } = useDepartments(filters);
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
    <div className="flex flex-col gap-6 p-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Departments</h1>
          <p className="text-muted-foreground mt-1">
            Manage your organization's departments and their structures.
          </p>
        </div>
        <Button onClick={() => navigate('/departments/new')}>
          <Plus className="mr-2 h-4 w-4" /> Add Department
        </Button>
      </div>

      <div className="bg-card border rounded-lg p-4 flex flex-col gap-4">
        <DepartmentFilters
          onSearchChange={(searchTerm) => setFilters(prev => ({ ...prev, searchTerm, pageNumber: 1 }))}
        />
        <DepartmentTable
          departments={data?.items || []}
          isLoading={isLoading}
        />

        {/* Basic Pagination Controls */}
        {data && data.pagination.totalPages > 1 && (
          <div className="flex items-center justify-between pt-4 border-t">
            <p className="text-sm text-muted-foreground">
              Showing page {data.pagination.pageNumber} of {data.pagination.totalPages}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={!data.pagination.hasPreviousPage}
                onClick={() => setFilters(prev => ({ ...prev, pageNumber: prev.pageNumber - 1 }))}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={!data.pagination.hasNextPage}
                onClick={() => setFilters(prev => ({ ...prev, pageNumber: prev.pageNumber + 1 }))}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
