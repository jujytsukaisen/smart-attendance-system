import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import { useTranslation } from '@/shared/i18n';

interface DepartmentFiltersProps {
  onSearchChange: (value: string) => void;
}

export const DepartmentFilters = ({ onSearchChange }: DepartmentFiltersProps) => {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col sm:flex-row gap-4 w-full">
      <div className="relative flex-1 max-w-sm">
        <Search className="absolute ltr:left-2.5 rtl:right-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder={t('departments.searchPlaceholder')}
          className="ltr:pl-8 rtl:pr-8"
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>
    </div>
  );
};
