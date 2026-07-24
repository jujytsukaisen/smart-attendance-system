import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DeviceForm } from '../components';
import { useCreateDevice } from '../hooks';
import { toast } from 'sonner';

export function CreateDevicePage() {
  const navigate = useNavigate();
  const createMutation = useCreateDevice();

  const handleSubmit = (values: any) => {
    // Provide a default empty GUID for companyId since it's required by the DTO but not the User store
    const payload = {
      ...values,
      companyId: '00000000-0000-0000-0000-000000000000',
    };

    createMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Device created successfully');
        navigate('/devices');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to create device');
      },
    });
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/devices')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Add New Device</h1>
          <p className="mt-1 text-sm text-gray-500">Configure a new biometric device for the system.</p>
        </div>
      </div>

      <DeviceForm 
        onSubmit={handleSubmit} 
        isLoading={createMutation.isPending} 
      />
    </div>
  );
}
