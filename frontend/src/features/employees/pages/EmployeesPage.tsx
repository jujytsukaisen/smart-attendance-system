import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { EmployeeTable, EmployeeFilters } from '../components';
import { useEmployees } from '../hooks';
import { useTranslation } from '@/shared/i18n';

export const EmployeesPage = () => {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string>('all');

  const pageSize = 10;
  const statusParam = status !== 'all' ? parseInt(status) as any : undefined;

  const {
    data,
    isLoading,
    isError,
    error,
  } = useEmployees({
    pageNumber: page,
    pageSize,
    searchTerm: search || undefined,
    status: statusParam,
  });

  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : (isRtl ? 'فشل تحميل بيانات الموظفين' : 'Failed to load employees')}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">{t('employees.title')}</h1>
          <p className="text-muted-foreground mt-1">
            {t('employees.subtitle')}
          </p>
        </div>
        <Button onClick={() => navigate('/employees/create')} className="gap-2">
          <Plus className="h-4 w-4" />
          <span>{t('employees.addNew')}</span>
        </Button>
      </div>

      <EmployeeFilters
        onSearchChange={(val) => {
          setSearch(val);
          setPage(1);
        }}
        statusFilter={status}
        onStatusChange={(val) => {
          setStatus(val);
          setPage(1);
        }}
      />

      <EmployeeTable
        employees={data?.items || []}
        isLoading={isLoading}
      />

      {data && data.pagination.totalPages > 1 && (
        <div className="flex items-center justify-between pt-4 border-t border-border">
          <p className="text-sm text-muted-foreground">
            {t('attendance.showingPage')}{' '}
            <span className="font-medium text-foreground">{data.pagination.pageNumber}</span>{' '}
            {t('attendance.of')}{' '}
            <span className="font-medium text-foreground">{data.pagination.totalPages}</span>
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!data.pagination.hasPreviousPage}
              onClick={() => setPage(p => p - 1)}
            >
              {t('attendance.previous')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!data.pagination.hasNextPage}
              onClick={() => setPage(p => p + 1)}
            >
              {t('attendance.next')}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};
