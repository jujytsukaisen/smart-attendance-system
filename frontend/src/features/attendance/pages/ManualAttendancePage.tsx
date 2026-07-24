import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { AttendanceForm } from '../components';
import { useManualAttendance } from '../hooks';
import type { ManualAttendanceFormValues } from '../schemas';
import { CheckInMethod } from '../types';

export function ManualAttendancePage() {
  const navigate = useNavigate();
  const mutation = useManualAttendance();

  const handleSubmit = (values: ManualAttendanceFormValues) => {
    mutation.mutate({
      ...values,
      checkInMethod: CheckInMethod.Manual
    }, {
      onSuccess: () => {
        toast.success('Attendance record created successfully');
        navigate('/attendance');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to create attendance record');
      },
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">Manual Attendance Entry</h1>
        <p className="mt-1 text-sm text-gray-500">
          Create an attendance record manually. This action will be audited.
        </p>
      </div>

      <AttendanceForm onSubmit={handleSubmit} isLoading={mutation.isPending} />
    </div>
  );
}
