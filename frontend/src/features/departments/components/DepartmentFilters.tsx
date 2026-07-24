import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';

interface DepartmentFiltersProps {
  onSearchChange: (value: string) => void;
}

export const DepartmentFilters = ({ onSearchChange }: DepartmentFiltersProps) => {
  return (
    <div className="flex flex-col sm:flex-row gap-4 w-full">
      <div className="relative flex-1 max-w-sm">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search departments..."
          className="pl-8"
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>
    </div>
  );
};
