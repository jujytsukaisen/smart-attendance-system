import { useState } from 'react';
import { Button } from '@/shared/components/ui/button';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/shared/components/ui/select';
import { useEmployees } from '@/features/employees/hooks';
import { LeaveStatus, type LeaveFilter } from '../types';
import { useTranslation } from '@/shared/i18n';

interface LeaveFiltersProps {
  onFilterChange: (filters: Partial<LeaveFilter>) => void;
  isEmployeeView?: boolean;
}

export function LeaveFilters({ onFilterChange, isEmployeeView = false }: LeaveFiltersProps) {
  const { t, isRtl } = useTranslation();
  const [employeeId, setEmployeeId] = useState<string>('all');
  const [status, setStatus] = useState<string>('all');
  
  const { data: employeesData } = useEmployees({ pageNumber: 1, pageSize: 100 }, !isEmployeeView);

  const handleApplyFilters = () => {
    onFilterChange({
      employeeId: employeeId === 'all' ? undefined : employeeId,
      status: status === 'all' ? undefined : (parseInt(status) as LeaveStatus),
    });
  };

  const handleClearFilters = () => {
    setEmployeeId('all');
    setStatus('all');
    onFilterChange({
      employeeId: undefined,
      status: undefined,
    });
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6 items-end">
      {!isEmployeeView && (
        <div className="w-full sm:w-64">
          <label className="block text-sm font-medium text-muted-foreground mb-1">
            {t('attendance.employee')}
          </label>
          <Select value={employeeId} onValueChange={(val) => setEmployeeId(val || 'all')}>
            <SelectTrigger>
              <SelectValue placeholder={isRtl ? 'جميع الموظفين' : 'All Employees'} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">{isRtl ? 'جميع الموظفين' : 'All Employees'}</SelectItem>
              {employeesData?.items.map((emp) => (
                <SelectItem key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}
      <div className="w-full sm:w-48">
        <label className="block text-sm font-medium text-muted-foreground mb-1">
          {t('common.status')}
        </label>
        <Select value={status} onValueChange={(val) => setStatus(val || 'all')}>
          <SelectTrigger>
            <SelectValue placeholder={t('attendance.allStatuses')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('attendance.allStatuses')}</SelectItem>
            <SelectItem value={LeaveStatus.Pending.toString()}>{t('leaves.pending')}</SelectItem>
            <SelectItem value={LeaveStatus.Approved.toString()}>{t('leaves.approved')}</SelectItem>
            <SelectItem value={LeaveStatus.Rejected.toString()}>{t('leaves.rejected')}</SelectItem>
            <SelectItem value={LeaveStatus.Cancelled.toString()}>{t('leaves.cancelled')}</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div className="flex gap-2 w-full sm:w-auto">
        <Button onClick={handleApplyFilters} className="w-full sm:w-auto">
          {isRtl ? 'تطبيق' : 'Apply'}
        </Button>
        <Button onClick={handleClearFilters} variant="outline" className="w-full sm:w-auto">
          {isRtl ? 'إعادة ضبط' : 'Clear'}
        </Button>
      </div>
    </div>
  );
}
