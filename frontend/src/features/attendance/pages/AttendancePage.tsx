import { useState } from 'react';
import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { AttendanceTable, AttendanceFilters } from '../components';
import { useAuthStore } from '@/features/auth/store';
import { useAttendances, useMyAttendances } from '../hooks';
import type { AttendanceFilter } from '../types';
import { useTranslation } from '@/shared/i18n';

export function AttendancePage() {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const { user } = useAuthStore();
  const isAdmin = user?.roles?.includes('Admin');
  const isEmployeeOnly = !isAdmin;

  const [filter, setFilter] = useState<AttendanceFilter>({
    pageNumber: 1,
    pageSize: 10,
  });

  const allQuery = useAttendances(filter, !isEmployeeOnly);
  const myQuery = useMyAttendances(filter, isEmployeeOnly);
  
  // Choose query and data based on role
  const { data, isLoading, isError } = isEmployeeOnly ? myQuery : allQuery;

  const handleFilterChange = (newFilters: Partial<AttendanceFilter>) => {
    setFilter((prev) => ({ ...prev, ...newFilters, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter((prev) => ({ ...prev, pageNumber: newPage }));
  };

  if (isError) {
    return (
      <div className="flex justify-center items-center h-64">
        <p className="text-destructive">
          {isRtl ? 'فشل تحميل سجلات الحضور. يرجى المحاولة مجدداً.' : 'Failed to load attendance records. Please try again.'}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">{t('attendance.title')}</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {t('attendance.subtitle')}
          </p>
        </div>
        {!isEmployeeOnly && (
          <Button onClick={() => navigate('/attendance/manual')} className="gap-2">
            <Plus className="h-4 w-4" />
            <span>{t('attendance.manualEntry')}</span>
          </Button>
        )}
      </div>

      <AttendanceFilters onFilterChange={handleFilterChange} isEmployeeView={isEmployeeOnly} />

      {isLoading ? (
        <div className="space-y-4">
          <div className="h-10 bg-muted rounded animate-pulse" />
          <div className="h-20 bg-muted rounded animate-pulse" />
          <div className="h-20 bg-muted rounded animate-pulse" />
        </div>
      ) : data?.items.length === 0 ? (
        <div className="text-center py-12 bg-card text-card-foreground border border-border rounded-lg shadow-sm">
          <h3 className="mt-2 text-sm font-semibold text-foreground">{t('attendance.noRecordsFound')}</h3>
          <p className="mt-1 text-sm text-muted-foreground">{t('attendance.noRecordsHint')}</p>
        </div>
      ) : (
        <>
          <AttendanceTable data={data?.items || []} isEmployeeView={isEmployeeOnly} />
          
          {data?.pagination && data.pagination.totalPages > 1 && (
            <div className="flex justify-between items-center bg-card text-card-foreground px-4 py-3 border border-border sm:px-6 rounded-lg shadow-sm mt-4">
              <div className="flex justify-between flex-1 sm:hidden">
                <Button
                  onClick={() => handlePageChange(data.pagination.pageNumber - 1)}
                  disabled={!data.pagination.hasPreviousPage}
                  variant="outline"
                >
                  {t('attendance.previous')}
                </Button>
                <Button
                  onClick={() => handlePageChange(data.pagination.pageNumber + 1)}
                  disabled={!data.pagination.hasNextPage}
                  variant="outline"
                >
                  {t('attendance.next')}
                </Button>
              </div>
              <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm text-muted-foreground">
                    {t('attendance.showingPage')}{' '}
                    <span className="font-medium text-foreground">{data.pagination.pageNumber}</span>{' '}
                    {t('attendance.of')}{' '}
                    <span className="font-medium text-foreground">{data.pagination.totalPages}</span>
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
                      {t('attendance.previous')}
                    </Button>
                    <Button
                      onClick={() => handlePageChange(data.pagination.pageNumber + 1)}
                      disabled={!data.pagination.hasNextPage}
                      variant="outline"
                      className="rounded-l-none rounded-r-md"
                    >
                      {t('attendance.next')}
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
