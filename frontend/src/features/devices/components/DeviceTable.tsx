import { useState } from 'react';
import { Eye, Edit2, Play, Square, Trash2, RefreshCw } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog } from '@/features/leaves/components/ActionDialog';
import { useActivateDevice, useDeactivateDevice, useDeleteDevice, useSyncDevice } from '../hooks';
import type { AttendanceDeviceListDto } from '../types';
import { toast } from 'sonner';
import { DeviceStatusBadge, ConnectionStatusBadge, DeviceTypeBadge } from './DeviceStatusBadge';

interface DeviceTableProps {
  data: AttendanceDeviceListDto[];
}

export function DeviceTable({ data }: DeviceTableProps) {
  const navigate = useNavigate();
  const activateMutation = useActivateDevice();
  const deactivateMutation = useDeactivateDevice();
  const deleteMutation = useDeleteDevice();
  const syncMutation = useSyncDevice();

  const [activateId, setActivateId] = useState<string | null>(null);
  const [deactivateId, setDeactivateId] = useState<string | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleActivate = () => {
    if (!activateId) return;
    activateMutation.mutate(activateId, {
      onSuccess: () => {
        toast.success('Device activated successfully');
        setActivateId(null);
      },
      onError: () => {
        toast.error('Failed to activate device');
        setActivateId(null);
      },
    });
  };

  const handleDeactivate = () => {
    if (!deactivateId) return;
    deactivateMutation.mutate(deactivateId, {
      onSuccess: () => {
        toast.success('Device deactivated successfully');
        setDeactivateId(null);
      },
      onError: () => {
        toast.error('Failed to deactivate device');
        setDeactivateId(null);
      },
    });
  };

  const handleDelete = () => {
    if (!deleteId) return;
    deleteMutation.mutate(deleteId, {
      onSuccess: () => {
        toast.success('Device deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error('Failed to delete device');
        setDeleteId(null);
      },
    });
  };

  const handleSync = (id: string) => {
    toast.promise(syncMutation.mutateAsync(id), {
      loading: 'Syncing device...',
      success: (data) => `Sync completed: ${data.recordsSynced} records synced.`,
      error: 'Failed to sync device.',
    });
  };

  return (
    <>
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Device Info</th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Connection</th>
              <th scope="col" className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {data.map((device) => (
              <tr key={device.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center">
                    <div>
                      <div className="text-sm font-medium text-gray-900">{device.name}</div>
                      <div className="text-sm text-gray-500">Code: {device.code}</div>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <DeviceTypeBadge type={device.deviceType} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <DeviceStatusBadge status={device.deviceStatus} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <ConnectionStatusBadge status={device.connectionStatus} />
                </td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <div className="flex justify-end gap-2">
                    <Button variant="ghost" size="icon" onClick={() => handleSync(device.id)} title="Sync Device">
                      <RefreshCw className="h-4 w-4 text-blue-500" />
                    </Button>
                    {device.isActive ? (
                      <Button variant="ghost" size="icon" onClick={() => setDeactivateId(device.id)} title="Deactivate Device">
                        <Square className="h-4 w-4 text-yellow-600" />
                      </Button>
                    ) : (
                      <Button variant="ghost" size="icon" onClick={() => setActivateId(device.id)} title="Activate Device">
                        <Play className="h-4 w-4 text-green-600" />
                      </Button>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/devices/${device.id}/edit`)}>
                      <Edit2 className="h-4 w-4 text-gray-500" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/devices/${device.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setDeleteId(device.id)}>
                      <Trash2 className="h-4 w-4 text-red-500" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!activateId}
        onClose={() => setActivateId(null)}
        onConfirm={handleActivate}
        title="Activate Device"
        description="Are you sure you want to activate this device?"
        confirmText="Activate"
        variant="default"
        isLoading={activateMutation.isPending}
      />

      <ActionDialog
        isOpen={!!deactivateId}
        onClose={() => setDeactivateId(null)}
        onConfirm={handleDeactivate}
        title="Deactivate Device"
        description="Are you sure you want to deactivate this device? It will stop communicating with the server."
        confirmText="Deactivate"
        variant="destructive"
        isLoading={deactivateMutation.isPending}
      />

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete Device"
        description="Are you sure you want to delete this device? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
    </>
  );
}
