import { useState } from 'react';
import { Input } from '@/shared/components/ui/input';
import { Button } from '@/shared/components/ui/button';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/shared/components/ui/select';
import { Search } from 'lucide-react';
import { AttendanceStatus, type AttendanceFilter } from '../types';
import { useTranslation } from '@/shared/i18n';

interface AttendanceFiltersProps {
  onFilterChange: (filters: Partial<AttendanceFilter>) => void;
  isEmployeeView?: boolean;
}

export function AttendanceFilters({ onFilterChange, isEmployeeView }: AttendanceFiltersProps) {
  const { t, isRtl } = useTranslation();
  const [searchTerm, setSearchTerm] = useState('');

  const handleSearch = () => {
    onFilterChange({ searchTerm });
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      {!isEmployeeView && (
        <div className="flex-1 relative">
          <Input
            placeholder={t('attendance.searchPlaceholder')}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className={isRtl ? 'pr-10 pl-3' : 'pl-10 pr-3'}
          />
          <Search className={`absolute ${isRtl ? 'right-3' : 'left-3'} top-2.5 h-5 w-5 text-muted-foreground`} />
        </div>
      )}
      <div className="w-full sm:w-48">
        <Select
          onValueChange={(value: string | null) => {
            if (!value) return;
            if (value === 'all') {
              onFilterChange({ status: undefined });
            } else {
              onFilterChange({ status: parseInt(value) as AttendanceStatus });
            }
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder={t('attendance.allStatuses')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('attendance.allStatuses')}</SelectItem>
            <SelectItem value={AttendanceStatus.Present.toString()}>{t('attendance.present')}</SelectItem>
            <SelectItem value={AttendanceStatus.Pending.toString()}>{t('attendance.pending')}</SelectItem>
            <SelectItem value={AttendanceStatus.Absent.toString()}>{t('attendance.absent')}</SelectItem>
            <SelectItem value={AttendanceStatus.Late.toString()}>{t('attendance.late')}</SelectItem>
            <SelectItem value={AttendanceStatus.EarlyLeave.toString()}>{t('attendance.earlyLeave')}</SelectItem>
            <SelectItem value={AttendanceStatus.PresentDuringLeave.toString()}>{t('attendance.presentDuringLeave') || 'Present (On Leave)'}</SelectItem>
            <SelectItem value={AttendanceStatus.LateDuringLeave.toString()}>{t('attendance.lateDuringLeave') || 'Late (On Leave)'}</SelectItem>
          </SelectContent>
        </Select>
      </div>
      {!isEmployeeView && (
        <Button onClick={handleSearch}>{t('common.search')}</Button>
      )}
    </div>
  );
}
