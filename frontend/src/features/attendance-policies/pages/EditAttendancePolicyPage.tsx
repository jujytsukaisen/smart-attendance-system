import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { AttendancePolicyForm } from '../components';
import { useAttendancePolicy, useUpdateAttendancePolicy } from '../hooks';
import { toast } from 'sonner';
import type { AttendancePolicyFormValues } from '../schemas';

export function EditAttendancePolicyPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: policy, isLoading } = useAttendancePolicy(id!);
  const updateMutation = useUpdateAttendancePolicy();

  const handleSubmit = (values: AttendancePolicyFormValues) => {
    // ⚠️ الباك إند يتوقع null (وليس نص فارغ) لحقل الوقت عندما لا يكون محدّدًا
    updateMutation.mutate(
      { ...values, id: id!, autoCheckoutTime: values.autoCheckoutTime || undefined },
      {
        onSuccess: () => {
          toast.success('Attendance policy updated successfully');
          navigate(`/attendance-policies/${id}`);
        },
        onError: () => {
          toast.error('Failed to update attendance policy');
        },
      }
    );
  };

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 bg-gray-200 rounded w-1/4"></div>
        <div className="h-96 bg-gray-100 rounded w-full"></div>
      </div>
    );
  }

  if (!policy) {
    return (
      <div className="text-center py-12 bg-red-50 rounded-lg border border-red-200">
        <p className="text-red-500">Failed to load attendance policy for editing.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/attendance-policies')}>
          Go Back
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => navigate(`/attendance-policies/${id}`)} className="p-2">
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Edit Attendance Policy</h1>
          <p className="mt-1 text-sm text-gray-500">
            Modify attendance rules for {policy.name}.
          </p>
        </div>
      </div>

      <AttendancePolicyForm
        initialValues={{
          companyId: policy.companyId,
          name: policy.name,
          code: policy.code,
          description: policy.description || '',
          attendanceMode: policy.attendanceMode,
          requireFaceRecognition: policy.requireFaceRecognition,
          requireGPS: policy.requireGPS,
          requireApprovedDevice: policy.requireApprovedDevice,
          requireCompanyNetwork: policy.requireCompanyNetwork,
          allowMobileCheckIn: policy.allowMobileCheckIn,
          allowDesktopCheckIn: policy.allowDesktopCheckIn,
          allowWebCheckIn: policy.allowWebCheckIn,
          allowOfflineAttendance: policy.allowOfflineAttendance,
          allowedRadiusMeters: policy.allowedRadiusMeters,
          allowMultipleCheckIn: policy.allowMultipleCheckIn,
          allowManualAttendance: policy.allowManualAttendance,
          requireManagerApproval: policy.requireManagerApproval,
          maximumDistanceMeters: policy.maximumDistanceMeters,
          lateToleranceMinutes: policy.lateToleranceMinutes,
          earlyLeaveToleranceMinutes: policy.earlyLeaveToleranceMinutes,
          gracePeriodMinutes: policy.gracePeriodMinutes,
          overtimeThresholdMinutes: policy.overtimeThresholdMinutes,
          minimumOvertimeMinutes: policy.minimumOvertimeMinutes,
          maximumOvertimeMinutes: policy.maximumOvertimeMinutes,
          autoCheckoutEnabled: policy.autoCheckoutEnabled,
          autoCheckoutTime: policy.autoCheckoutTime || undefined,
          multipleShiftsSupport: policy.multipleShiftsSupport,
          roundTimeRule: policy.roundTimeRule,
          missingPunchPolicy: policy.missingPunchPolicy,
          weekendPolicy: policy.weekendPolicy,
          holidayPolicy: policy.holidayPolicy,
        }}
        onSubmit={handleSubmit}
        isLoading={updateMutation.isPending}
        isEdit
      />
    </div>
  );
}