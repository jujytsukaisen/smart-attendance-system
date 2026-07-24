import { Input } from '@/shared/components/ui/input';
import { Search } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';

interface EmployeeFiltersProps {
  onSearchChange: (value: string) => void;
  statusFilter: string;
  onStatusChange: (value: string) => void;
}

export const EmployeeFilters = ({ onSearchChange, statusFilter, onStatusChange }: EmployeeFiltersProps) => {
  return (
    <div className="flex flex-col sm:flex-row gap-4 w-full mb-6">
      <div className="relative flex-1 max-w-sm">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search by name, email or employee number..."
          className="pl-8"
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>
      
      <Select value={statusFilter} onValueChange={(val) => onStatusChange(val as string)}>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="All Statuses" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Statuses</SelectItem>
          <SelectItem value="0">Active</SelectItem>
          <SelectItem value="1">On Leave</SelectItem>
          <SelectItem value="2">Suspended</SelectItem>
          <SelectItem value="3">Terminated</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
};
