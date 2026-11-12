import { useState } from 'react';
import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import type { ReportFilterDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface ReportFiltersProps {
  onFilterChange: (filter: ReportFilterDto) => void;
  showSearch?: boolean;
}

export function ReportFilters({ onFilterChange, showSearch = true }: ReportFiltersProps) {
  const { t } = useTranslation();
  const [filter, setFilter] = useState<ReportFilterDto>({});

  const handleChange = (key: keyof ReportFilterDto, value: string | undefined) => {
    const updated = { ...filter, [key]: value };
    setFilter(updated);
    onFilterChange(updated);
  };

  return (
    <div className="flex flex-col sm:flex-row flex-wrap gap-4 mb-6">
      {showSearch && (
        <div className="relative flex-1 min-w-[200px]">
          <div className="absolute inset-y-0 ltr:left-0 rtl:right-0 ltr:pl-3 rtl:pr-3 flex items-center pointer-events-none">
            <Search className="h-4 w-4 text-muted-foreground" />
          </div>
          <Input
            type="text"
            placeholder={t('reports.searchPlaceholder')}
            className="ltr:pl-9 rtl:pr-9 w-full"
            value={filter.searchTerm || ''}
            onChange={(e) => handleChange('searchTerm', e.target.value || undefined)}
          />
        </div>
      )}
      
      <div className="w-full sm:w-44">
        <Select onValueChange={(val: any) => handleChange('departmentId', val === 'all' ? undefined : String(val))}>
          <SelectTrigger>
            <SelectValue placeholder={t('reports.allDepartments')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('reports.allDepartments')}</SelectItem>
            <SelectItem value="d1">Engineering</SelectItem>
            <SelectItem value="d2">Sales</SelectItem>
            <SelectItem value="d3">HR</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="w-full sm:w-40">
        <Input 
          type="date" 
          value={filter.startDate || ''}
          onChange={(e) => handleChange('startDate', e.target.value || undefined)}
          placeholder={t('reports.startDate')}
        />
      </div>

      <div className="w-full sm:w-40">
        <Input 
          type="date" 
          value={filter.endDate || ''}
          onChange={(e) => handleChange('endDate', e.target.value || undefined)}
          placeholder={t('reports.endDate')}
        />
      </div>
    </div>
  );
}
