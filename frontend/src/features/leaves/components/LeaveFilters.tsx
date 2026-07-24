import { useState } from 'react';
import { Button } from '@/shared/components/ui/button';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/shared/components/ui/select';
import { useEmployees } from '@/features/employees/hooks';
import { LeaveStatus, type LeaveFilter } from '../types';

interface LeaveFiltersProps {
  onFilterChange: (filters: Partial<LeaveFilter>) => void;
}

export function LeaveFilters({ onFilterChange }: LeaveFiltersProps) {
  const [employeeId, setEmployeeId] = useState<string>('all');
  const [status, setStatus] = useState<string>('all');
  
  // Use pagination params to load employees for the dropdown. 
  // For a real app, this might be a searchable combobox.
  const { data: employeesData } = useEmployees({ pageNumber: 1, pageSize: 100 });

  const handleApplyFilters = () => {
    onFilterChange({
      employeeId: employeeId === 'all' ? undefined : employeeId,
      status: status === 'all' ? undefined : (parseInt(status) as LeaveStatus),
    });
  };

  const handleClearFilters = () => {
    setEmployeeId('all');
    setStatus('all');
    onFilterChange({
      employeeId: undefined,
      status: undefined,
    });
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6 items-end">
      <div className="w-full sm:w-64">
        <label className="block text-sm font-medium text-gray-700 mb-1">Employee</label>
        <Select value={employeeId} onValueChange={(val) => setEmployeeId(val || 'all')}>
          <SelectTrigger>
            <SelectValue placeholder="All Employees" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Employees</SelectItem>
            {employeesData?.items.map((emp) => (
              <SelectItem key={emp.id} value={emp.id}>
                {emp.firstName} {emp.lastName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="w-full sm:w-48">
        <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
        <Select value={status} onValueChange={(val) => setStatus(val || 'all')}>
          <SelectTrigger>
            <SelectValue placeholder="All Statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value={LeaveStatus.Pending.toString()}>Pending</SelectItem>
            <SelectItem value={LeaveStatus.Approved.toString()}>Approved</SelectItem>
            <SelectItem value={LeaveStatus.Rejected.toString()}>Rejected</SelectItem>
            <SelectItem value={LeaveStatus.Cancelled.toString()}>Cancelled</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div className="flex gap-2 w-full sm:w-auto">
        <Button onClick={handleApplyFilters} className="w-full sm:w-auto">Apply</Button>
        <Button onClick={handleClearFilters} variant="outline" className="w-full sm:w-auto">Clear</Button>
      </div>
    </div>
  );
}
