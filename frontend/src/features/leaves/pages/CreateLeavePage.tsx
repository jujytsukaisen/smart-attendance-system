import { useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { toast } from 'sonner';
import { LeaveForm } from '../components';
import { useCreateLeave } from '../hooks';
import type { LeaveFormValues } from '../schemas';
import { useAuthStore } from '@/features/auth/store';
import { useTranslation } from '@/shared/i18n';

export function CreateLeavePage() {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const mutation = useCreateLeave();

  const { user } = useAuthStore();
  const isAdmin = user?.roles?.includes('Admin');
  const isEmployeeOnly = !isAdmin;

  const handleSubmit = async (values: LeaveFormValues) => {
    try {
      await mutation.mutateAsync({
        ...values,
        employeeId: values.employeeId || user?.id || ''
      } as any);
      toast.success(isRtl ? 'تم إنشاء طلب الإجازة بنجاح' : 'Leave request created successfully');
      setTimeout(() => {
        navigate(-1);
      }, 500);
    } catch (error: any) {
      toast.error(error.response?.data?.title || (isRtl ? 'فشل إنشاء طلب الإجازة' : 'Failed to create leave request'));
    }
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate(-1)}>
          <BackIcon className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            {t('leaves.requestLeave')}
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {isRtl ? 'تقديم طلب إجازة جديد للموظف وتحديد الفترة الزمنية والسبب.' : 'Submit a new leave request for an employee.'}
          </p>
        </div>
      </div>

      <LeaveForm onSubmit={handleSubmit} isLoading={mutation.isPending} isEmployeeView={isEmployeeOnly} />
    </div>
  );
}
