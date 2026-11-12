import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import { useTranslation } from '@/shared/i18n';

interface ShiftFiltersProps {
  onSearchChange: (value: string) => void;
}

export const ShiftFilters = ({ onSearchChange }: ShiftFiltersProps) => {
  const { isRtl } = useTranslation();

  return (
    <div className="flex flex-col sm:flex-row gap-4 w-full">
      <div className="relative flex-1 max-w-sm">
        <Search className={`absolute ${isRtl ? 'right-2.5' : 'left-2.5'} top-2.5 h-4 w-4 text-muted-foreground`} />
        <Input
          placeholder={isRtl ? 'بحث في الورديات...' : 'Search shifts...'}
          className={isRtl ? 'pr-8' : 'pl-8'}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>
    </div>
  );
};
