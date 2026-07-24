import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { LeaveForm } from '../components';
import { useCreateLeave } from '../hooks';
import type { LeaveFormValues } from '../schemas';

export function CreateLeavePage() {
  const navigate = useNavigate();
  const mutation = useCreateLeave();

  const handleSubmit = (values: LeaveFormValues) => {
    mutation.mutate(values, {
      onSuccess: () => {
        toast.success('Leave request created successfully');
        navigate('/leaves');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to create leave request');
      },
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">New Leave Request</h1>
        <p className="mt-1 text-sm text-gray-500">
          Submit a new leave request for an employee.
        </p>
      </div>

      <LeaveForm onSubmit={handleSubmit} isLoading={mutation.isPending} />
    </div>
  );
}
