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
import { DeviceType, DeviceStatus, ConnectionStatus, type DeviceFilter } from '../types';

interface DeviceFiltersProps {
  onFilterChange: (filters: Partial<DeviceFilter>) => void;
}

export function DeviceFilters({ onFilterChange }: DeviceFiltersProps) {
  const [searchTerm, setSearchTerm] = useState('');

  // Debounce search term
  useEffect(() => {
    const timer = setTimeout(() => {
      onFilterChange({ searchTerm: searchTerm || undefined });
    }, 500);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      <div className="relative flex-1">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <Search className="h-5 w-5 text-gray-400" />
        </div>
        <Input
          type="text"
          placeholder="Search devices..."
          className="pl-10 w-full"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>
      <div className="w-full sm:w-40">
        <Select onValueChange={(val) => onFilterChange({ deviceType: val === 'all' ? undefined : Number(val) as DeviceType })}>
          <SelectTrigger>
            <SelectValue placeholder="Device Type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Types</SelectItem>
            <SelectItem value={DeviceType.WallMounted.toString()}>Wall Mounted</SelectItem>
            <SelectItem value={DeviceType.Mobile.toString()}>Mobile</SelectItem>
            <SelectItem value={DeviceType.Desktop.toString()}>Desktop</SelectItem>
            <SelectItem value={DeviceType.Kiosk.toString()}>Kiosk</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div className="w-full sm:w-40">
        <Select onValueChange={(val) => onFilterChange({ deviceStatus: val === 'all' ? undefined : Number(val) as DeviceStatus })}>
          <SelectTrigger>
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value={DeviceStatus.Approved.toString()}>Approved</SelectItem>
            <SelectItem value={DeviceStatus.PendingApproval.toString()}>Pending</SelectItem>
            <SelectItem value={DeviceStatus.Maintenance.toString()}>Maintenance</SelectItem>
            <SelectItem value={DeviceStatus.Retired.toString()}>Retired</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div className="w-full sm:w-40">
        <Select onValueChange={(val) => onFilterChange({ connectionStatus: val === 'all' ? undefined : Number(val) as ConnectionStatus })}>
          <SelectTrigger>
            <SelectValue placeholder="Connection" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Connections</SelectItem>
            <SelectItem value={ConnectionStatus.Online.toString()}>Online</SelectItem>
            <SelectItem value={ConnectionStatus.Offline.toString()}>Offline</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
