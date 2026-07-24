import { useState, useEffect } from 'react';
import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import type { UserFilter } from '../types';

interface UserFiltersProps {
  onFilterChange: (filters: Partial<UserFilter>) => void;
}

export function UserFilters({ onFilterChange }: UserFiltersProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [status, setStatus] = useState<string>('all');

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      onFilterChange({ searchTerm: searchTerm || undefined });
    }, 500);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  const handleStatusChange = (val: string) => {
    setStatus(val);
    let isActive: boolean | undefined = undefined;
    if (val === 'active') isActive = true;
    if (val === 'suspended') isActive = false;
    
    onFilterChange({ isActive });
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      <div className="relative flex-1">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <Search className="h-5 w-5 text-gray-400" />
        </div>
        <Input
          type="text"
          placeholder="Search users..."
          className="pl-10 w-full"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>
      <div className="w-full sm:w-48">
        <Select value={status} onValueChange={(val) => handleStatusChange(val || 'all')}>
          <SelectTrigger>
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value="active">Active</SelectItem>
            <SelectItem value="suspended">Suspended</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
