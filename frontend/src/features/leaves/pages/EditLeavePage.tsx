import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { LeaveForm } from '../components';
import { useLeaveDetails, useUpdateLeave } from '../hooks';
import type { LeaveFormValues } from '../schemas';
import { useAuthStore } from '@/features/auth/store';

export function EditLeavePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data, isLoading: isLoadingDetails } = useLeaveDetails(id!);
  const mutation = useUpdateLeave();

  const { user } = useAuthStore();
  const isAdmin = user?.roles?.includes('Admin');
  const isEmployeeOnly = !isAdmin;

  const handleSubmit = async (values: LeaveFormValues) => {
    try {
      await mutation.mutateAsync({
        leaveId: id!,
        ...values,
      });
      toast.success('Leave request updated successfully');
      setTimeout(() => {
        window.location.href = '/leaves';
      }, 500);
    } catch (error: any) {
      toast.error(error.response?.data?.title || 'Failed to update leave request');
    }
  };

  if (isLoadingDetails) {
    return (
      <div className="max-w-3xl mx-auto space-y-6 animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/3"></div>
        <div className="h-64 bg-gray-100 rounded w-full"></div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500">Failed to load leave details.</p>
        <button onClick={() => navigate('/leaves')} className="mt-4 text-blue-500 hover:underline">
          Go back
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">Edit Leave Request</h1>
        <p className="mt-1 text-sm text-gray-500">
          Update the details of an existing leave request.
        </p>
      </div>

      <LeaveForm 
        initialValues={{
          companyId: data.companyId,
          employeeId: data.employeeId,
          leaveType: data.leaveType,
          startDate: new Date(data.startDate).toISOString().slice(0, 10),
          endDate: new Date(data.endDate).toISOString().slice(0, 10),
          reason: data.reason,
        }} 
        onSubmit={handleSubmit} 
        isLoading={mutation.isPending} 
        isEmployeeView={isEmployeeOnly}
      />
    </div>
  );
}
