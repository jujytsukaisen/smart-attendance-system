import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { toast } from 'sonner';
import { 
  useDeviceDetails, 
  useTestConnection, 
  useRestartDevice, 
  useUploadUsers, 
  useUploadPolicies,
  useDeviceLogs
} from '../hooks';
import { 
  DeviceConnectionCard, 
  DeviceStatisticsCard, 
  DeviceLogsTable,
  DeviceStatusBadge
} from '../components';

export function DeviceDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: device, isLoading, isError: isError1, error: error1 } = useDeviceDetails(id!);
  const { data: logs, isLoading: logsLoading, isError: isError2, error: error2 } = useDeviceLogs(id!);
  
  const testConnectionMutation = useTestConnection();
  const restartMutation = useRestartDevice();
  const uploadUsersMutation = useUploadUsers();
  const uploadPoliciesMutation = useUploadPolicies();

  const handleTestConnection = () => {
    toast.promise(testConnectionMutation.mutateAsync(id!), {
      loading: 'Testing connection...',
      success: (result) => result ? 'Connection successful!' : 'Connection failed. Device might be offline.',
      error: 'Failed to test connection.',
    });
  };

  const handleRestart = () => {
    toast.promise(restartMutation.mutateAsync(id!), {
      loading: 'Sending restart command...',
      success: 'Restart command sent successfully.',
      error: 'Failed to restart device.',
    });
  };

  const handleUploadUsers = () => {
    toast.promise(uploadUsersMutation.mutateAsync(id!), {
      loading: 'Uploading users to device...',
      success: 'Users uploaded successfully.',
      error: 'Failed to upload users.',
    });
  };

  const handleUploadPolicies = () => {
    toast.promise(uploadPoliciesMutation.mutateAsync(id!), {
      loading: 'Uploading policies to device...',
      success: 'Policies uploaded successfully.',
      error: 'Failed to upload policies.',
    });
  };

  if (isError1 || isError2) {
    const currentError = error1 ?? error2;
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {currentError instanceof Error && !currentError.message.toLowerCase().includes('internal server error') && !currentError.message.includes('status code')
            ? currentError.message
            : "Failed to load"}
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-1/4" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton className="h-64" />
          <Skeleton className="h-64" />
        </div>
      </div>
    );
  }

  if (!device) return null;

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/devices')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold text-gray-900 flex items-center gap-3">
              {device.name}
              <DeviceStatusBadge status={device.deviceStatus} />
            </h1>
            <p className="mt-1 text-sm text-gray-500">Code: {device.code} • SN: {device.serialNumber || 'N/A'}</p>
          </div>
        </div>
        <Button variant="outline" onClick={() => navigate(`/devices/${device.id}/edit`)}>
          Edit Configuration
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <DeviceConnectionCard 
          device={device} 
          onTestConnection={handleTestConnection}
          onRestart={handleRestart}
          onUploadUsers={handleUploadUsers}
          onUploadPolicies={handleUploadPolicies}
          isTesting={testConnectionMutation.isPending}
          isRestarting={restartMutation.isPending}
        />
        <DeviceStatisticsCard device={device} />
      </div>

      <div className="mt-8">
        <h2 className="text-lg font-medium text-gray-900 mb-4">Device Logs</h2>
        {logsLoading ? (
          <Skeleton className="h-64 w-full" />
        ) : (
          <DeviceLogsTable logs={logs || []} />
        )}
      </div>
    </div>
  );
}
