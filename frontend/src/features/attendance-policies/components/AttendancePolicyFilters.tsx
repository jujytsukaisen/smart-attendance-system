import { Input } from '@/shared/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { Search } from 'lucide-react';
import type { AttendancePolicyFilter } from '../types';

interface AttendancePolicyFiltersProps {
  filters: AttendancePolicyFilter;
  onChange: (filters: AttendancePolicyFilter) => void;
}

export function AttendancePolicyFilters({ filters, onChange }: AttendancePolicyFiltersProps) {
  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6 w-full">
      <div className="relative flex-1 max-w-sm">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          type="text"
          placeholder="Search policies..."
          className="pl-8"
          value={filters.searchTerm || ''}
          onChange={(e) => onChange({ ...filters, searchTerm: e.target.value, pageNumber: 1 })}
        />
      </div>
      <Select
        value={filters.isActive === undefined ? 'all' : filters.isActive.toString()}
        onValueChange={(val) => {
          onChange({
            ...filters,
            isActive: val === 'all' ? undefined : val === 'true',
            pageNumber: 1,
          });
        }}
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="All Statuses" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Statuses</SelectItem>
          <SelectItem value="true">Active</SelectItem>
          <SelectItem value="false">Inactive</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
