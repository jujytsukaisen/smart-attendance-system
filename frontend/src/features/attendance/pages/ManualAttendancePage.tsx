import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { AttendanceForm } from '../components';
import { useManualAttendance } from '../hooks';
import type { ManualAttendanceFormValues } from '../schemas';
import { CheckInMethod } from '../types';
import { useTranslation } from '@/shared/i18n';

export function ManualAttendancePage() {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const mutation = useManualAttendance();

  const handleSubmit = (values: ManualAttendanceFormValues) => {
    mutation.mutate({
      employeeId: values.employeeId,
      attendanceDeviceId: values.attendanceDeviceId || undefined,
      attendancePolicyId: values.attendancePolicyId || undefined,
      shiftId: values.shiftId || undefined,
      checkInTime: new Date(values.checkInTime).toISOString(),
      checkOutTime: values.checkOutTime && values.checkOutTime.trim() !== '' ? new Date(values.checkOutTime).toISOString() : undefined,
      checkInMethod: CheckInMethod.Manual,
    }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم إنشاء سجل الحضور بنجاح' : 'Attendance record created successfully');
        navigate('/attendance');
      },
      onError: (error: any) => {
        const errorMsg = error.response?.data?.detail || error.response?.data?.message || error.response?.data?.title || (isRtl ? 'فشل إنشاء سجل الحضور' : 'Failed to create attendance record');
        toast.error(errorMsg);
      },
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">{t('attendance.manualEntryTitle')}</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {t('attendance.manualEntrySubtitle')}
        </p>
      </div>

      <AttendanceForm onSubmit={handleSubmit} isLoading={mutation.isPending} />
    </div>
  );
}
