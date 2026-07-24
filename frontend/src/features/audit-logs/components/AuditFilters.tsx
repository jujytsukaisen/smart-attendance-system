import { useState } from 'react';
import { Search } from 'lucide-react';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { AuditActionType, AuditEntityType, type AuditFilterDto } from '../types';

interface AuditFiltersProps {
  onFilterChange: (filter: AuditFilterDto) => void;
}

export function AuditFilters({ onFilterChange }: AuditFiltersProps) {
  const [filter, setFilter] = useState<AuditFilterDto>({});

  const handleChange = (key: keyof AuditFilterDto, value: any) => {
    const updated = { ...filter, [key]: value };
    setFilter(updated);
    onFilterChange(updated);
  };

  return (
    <div className="flex flex-col sm:flex-row flex-wrap gap-4 mb-6">
      <div className="relative flex-1 min-w-[200px]">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <Search className="h-4 w-4 text-gray-400" />
        </div>
        <Input
          type="text"
          placeholder="Search user, entity ID, or log ID..."
          className="pl-9 w-full"
          value={filter.searchTerm || ''}
          onChange={(e) => handleChange('searchTerm', e.target.value || undefined)}
        />
      </div>

      <div className="w-full sm:w-40">
        <Select onValueChange={(val: any) => handleChange('action', val === 'all' ? undefined : val as AuditActionType)}>
          <SelectTrigger>
            <SelectValue placeholder="Action" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Actions</SelectItem>
            {Object.values(AuditActionType).map(v => (
               <SelectItem key={v} value={v}>{v}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="w-full sm:w-40">
        <Select onValueChange={(val: any) => handleChange('entityType', val === 'all' ? undefined : val as AuditEntityType)}>
          <SelectTrigger>
            <SelectValue placeholder="Entity" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Entities</SelectItem>
            {Object.values(AuditEntityType).map(v => (
               <SelectItem key={v} value={v}>{v}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
