import { useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { EmployeeForm } from '../components';
import { useCreateEmployee } from '../hooks';
import { useTranslation } from '@/shared/i18n';

export const CreateEmployeePage = () => {
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const createMutation = useCreateEmployee();

  const handleSubmit = async (data: any) => {
    try {
      await createMutation.mutateAsync(data);
      navigate('/employees');
    } catch (error) {
      // Error is handled in the hook
    }
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/employees')}>
          <BackIcon className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            {isRtl ? 'إضافة موظف جديد' : 'Add New Employee'}
          </h1>
          <p className="text-muted-foreground mt-1">
            {isRtl ? 'أدخل تفاصيل وبيانات الموظف لإضافته للنظام.' : 'Fill in the details to onboard a new employee.'}
          </p>
        </div>
      </div>

      <div className="bg-card text-card-foreground border border-border rounded-lg p-6 shadow-sm">
        <EmployeeForm
          onSubmit={handleSubmit}
          isLoading={createMutation.isPending}
        />
      </div>
    </div>
  );
};
