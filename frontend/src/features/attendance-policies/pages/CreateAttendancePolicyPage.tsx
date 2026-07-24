import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { AttendancePolicyForm } from '../components';
import { useCreateAttendancePolicy } from '../hooks';
import { toast } from 'sonner';
import type { AttendancePolicyFormValues } from '../schemas';

export function CreateAttendancePolicyPage() {
  const navigate = useNavigate();
  const createMutation = useCreateAttendancePolicy();

  const handleSubmit = (values: AttendancePolicyFormValues) => {
    createMutation.mutate(values, {
      onSuccess: () => {
        toast.success('Attendance policy created successfully');
        navigate('/attendance-policies');
      },
      onError: () => {
        toast.error('Failed to create attendance policy');
      },
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => navigate('/attendance-policies')} className="p-2">
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Create Attendance Policy</h1>
          <p className="mt-1 text-sm text-gray-500">
            Define new attendance rules and verification requirements.
          </p>
        </div>
      </div>

      <AttendancePolicyForm
        onSubmit={handleSubmit}
        isLoading={createMutation.isPending}
      />
    </div>
  );
}
