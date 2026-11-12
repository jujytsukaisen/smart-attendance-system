import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { useTranslation } from '@/shared/i18n';

interface EmployeeFiltersProps {
  onSearchChange: (value: string) => void;
  statusFilter: string;
  onStatusChange: (value: string) => void;
}

export const EmployeeFilters = ({ onSearchChange, statusFilter, onStatusChange }: EmployeeFiltersProps) => {
  const { t, isRtl } = useTranslation();

  return (
    <div className="flex flex-col sm:flex-row gap-4 w-full mb-6">
      <div className="relative flex-1 max-w-sm">
        <Search className={`absolute ${isRtl ? 'right-3' : 'left-3'} top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground pointer-events-none`} />
        <Input
          placeholder={t('employees.searchPlaceholder')}
          className={`${isRtl ? 'pr-9 pl-3 text-right' : 'pl-9 pr-3 text-left'}`}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>
      
      <Select value={statusFilter} onValueChange={(val) => onStatusChange(val as string)}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder={t('employees.allStatuses')} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{t('employees.allStatuses')}</SelectItem>
          <SelectItem value="0">{t('employees.active')}</SelectItem>
          <SelectItem value="1">{t('employees.onLeave')}</SelectItem>
          <SelectItem value="2">{t('employees.suspended')}</SelectItem>
          <SelectItem value="3">{t('employees.terminated')}</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
};
