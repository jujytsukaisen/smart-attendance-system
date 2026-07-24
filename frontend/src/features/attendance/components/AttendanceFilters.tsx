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

interface AttendanceFiltersProps {
  onFilterChange: (filters: Partial<AttendanceFilter>) => void;
}

export function AttendanceFilters({ onFilterChange }: AttendanceFiltersProps) {
  const [searchTerm, setSearchTerm] = useState('');

  const handleSearch = () => {
    onFilterChange({ searchTerm });
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      <div className="flex-1 relative">
        <Input
          placeholder="Search attendances..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          className="pl-10"
        />
        <Search className="absolute left-3 top-2.5 h-5 w-5 text-gray-400" />
      </div>
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
            <SelectValue placeholder="All Statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value={AttendanceStatus.Present.toString()}>Present</SelectItem>
            <SelectItem value={AttendanceStatus.Pending.toString()}>Pending</SelectItem>
            <SelectItem value={AttendanceStatus.Absent.toString()}>Absent</SelectItem>
            <SelectItem value={AttendanceStatus.Late.toString()}>Late</SelectItem>
            <SelectItem value={AttendanceStatus.EarlyLeave.toString()}>Early Leave</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <Button onClick={handleSearch}>Search</Button>
    </div>
  );
}
