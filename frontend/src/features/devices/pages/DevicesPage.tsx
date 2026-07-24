import { useState } from 'react';
import { Plus, Wifi, WifiOff, AlertTriangle, RefreshCw } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { DeviceTable, DeviceFilters } from '../components';
import { useDevices } from '../hooks';
import type { DeviceFilter } from '../types';
import { ConnectionStatus } from '../types';
import { Skeleton } from '@/shared/components/ui/skeleton';

export function DevicesPage() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<DeviceFilter>({
    pageNumber: 1,
    pageSize: 10,
  });

  const { data, isLoading, isError, error } = useDevices(filters);

  const handleFilterChange = (newFilters: Partial<DeviceFilter>) => {
    setFilters(prev => ({ ...prev, ...newFilters, pageNumber: 1 }));
  };

  // Calculate dashboard stats
  const devices = data?.items || [];
  const onlineCount = devices.filter(d => d.connectionStatus === ConnectionStatus.Online).length;
  const offlineCount = devices.filter(d => d.connectionStatus === ConnectionStatus.Offline).length;
  const failedCount = devices.filter(d => !d.isActive).length;
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }



  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Devices</h1>
          <p className="mt-1 text-sm text-gray-500">Manage biometric devices and terminals.</p>
        </div>
        <Button onClick={() => navigate('/devices/new')}>
          <Plus className="h-4 w-4 mr-2" />
          Add Device
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Devices</CardTitle>
            <RefreshCw className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data?.pagination?.totalCount || 0}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Online</CardTitle>
            <Wifi className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{onlineCount}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Offline</CardTitle>
            <WifiOff className="h-4 w-4 text-gray-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{offlineCount}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Failed/Deactivated</CardTitle>
            <AlertTriangle className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{failedCount}</div>
          </CardContent>
        </Card>
      </div>

      <div className="bg-white p-6 rounded-lg shadow">
        <DeviceFilters onFilterChange={handleFilterChange} />
        
        {isLoading ? (
          <div className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
          </div>
        ) : devices.length === 0 ? (
          <div className="text-center py-10">
            <p className="text-sm text-gray-500">No devices found matching the criteria.</p>
          </div>
        ) : (
          <DeviceTable data={devices} />
        )}
      </div>
    </div>
  );
}
