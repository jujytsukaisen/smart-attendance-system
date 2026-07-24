import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { DeviceForm } from '../components';
import { useDeviceDetails, useUpdateDevice } from '../hooks';
import { toast } from 'sonner';

export function EditDevicePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading, isError, error } = useDeviceDetails(id!);
  const updateMutation = useUpdateDevice();

  const handleSubmit = (values: any) => {
    const payload = {
      ...values,
      id: id!,
    };

    updateMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Device updated successfully');
        navigate('/devices');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to update device');
      },
    });
  };

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

  if (isLoading) {


    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-1/4" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  if (!data) return null;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/devices')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Edit Device</h1>
          <p className="mt-1 text-sm text-gray-500">Update configuration for {data.name}</p>
        </div>
      </div>

      <DeviceForm 
        initialValues={{
          ...data,
          // Handling potential nulls by casting to empty string or required primitive
          serialNumber: data.serialNumber || '',
          operatingSystem: data.operatingSystem || '',
          applicationVersion: data.applicationVersion || '',
          branchId: data.branchId || '',
          ipAddress: data.ipAddress || '',
          port: data.port || 80,
          publicKey: data.publicKey || '',
          deviceFingerprint: data.deviceFingerprint || '',
        }} 
        onSubmit={handleSubmit} 
        isLoading={updateMutation.isPending} 
      />
    </div>
  );
}
