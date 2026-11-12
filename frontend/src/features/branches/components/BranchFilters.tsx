import { Input } from '@/shared/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { useTranslation } from '@/shared/i18n';

interface BranchFiltersProps {
  searchQuery: string;
  onSearchChange: (value: string) => void;
  statusFilter: string;
  onStatusChange: (value: string) => void;
}

export const BranchFilters = ({
  searchQuery,
  onSearchChange,
  statusFilter,
  onStatusChange,
}: BranchFiltersProps) => {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      <Input
        placeholder={t('branches.searchPlaceholder')}
        value={searchQuery}
        onChange={(e) => onSearchChange(e.target.value)}
        className="max-w-sm"
      />
      <Select value={statusFilter} onValueChange={(val) => val && onStatusChange(val)}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder={t('branches.allStatuses')} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">{t('branches.allStatuses')}</SelectItem>
          <SelectItem value="active">{t('common.active')}</SelectItem>
          <SelectItem value="inactive">{t('common.inactive')}</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
};
