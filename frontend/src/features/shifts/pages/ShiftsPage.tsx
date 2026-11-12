import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Clock } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { ShiftTable, ShiftFilters } from '../components';
import { useShifts } from '../hooks';
import type { ShiftFilter } from '../types';
import { useTranslation } from '@/shared/i18n';

export const ShiftsPage = () => {
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const [filters, setFilters] = useState<ShiftFilter>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
  });

  const { data, isLoading, isError, error } = useShifts(filters);

  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : (isRtl ? 'فشل تحميل الورديات' : 'Failed to load shifts')}
        </p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-2">
            <Clock className="h-7 w-7 text-primary" />
            <h1 className="text-3xl font-bold tracking-tight">
              {isRtl ? 'ورديات العمل' : 'Work Shifts'}
            </h1>
          </div>
          <p className="text-muted-foreground mt-1">
            {isRtl
              ? 'إدارة ورديات وساعات العمل، وتحديد أوقات الحضور وفترات السماح.'
              : 'Manage organization work shifts, timings, and grace periods.'}
          </p>
        </div>
        <Button onClick={() => navigate('/shifts/new')} className="gap-2">
          <Plus className="h-4 w-4" />
          <span>{isRtl ? 'إضافة وردية' : 'Add Shift'}</span>
        </Button>
      </div>

      <div className="bg-card border rounded-lg p-4 flex flex-col gap-4">
        <ShiftFilters
          onSearchChange={(searchTerm) => setFilters((prev) => ({ ...prev, searchTerm, pageNumber: 1 }))}
        />
        <ShiftTable
          shifts={data?.items || []}
          isLoading={isLoading}
        />

        {/* Pagination Controls */}
        {data && data.pagination.totalPages > 1 && (
          <div className="flex items-center justify-between pt-4 border-t">
            <p className="text-sm text-muted-foreground">
              {isRtl
                ? `عرض الصفحة ${data.pagination.pageNumber} من ${data.pagination.totalPages}`
                : `Showing page ${data.pagination.pageNumber} of ${data.pagination.totalPages}`}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={!data.pagination.hasPreviousPage}
                onClick={() => setFilters((prev) => ({ ...prev, pageNumber: prev.pageNumber - 1 }))}
              >
                {isRtl ? 'السابق' : 'Previous'}
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={!data.pagination.hasNextPage}
                onClick={() => setFilters((prev) => ({ ...prev, pageNumber: prev.pageNumber + 1 }))}
              >
                {isRtl ? 'التالي' : 'Next'}
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
