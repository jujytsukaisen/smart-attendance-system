import { useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { ShiftForm } from '../components';
import { useCreateShift } from '../hooks';
import type { CreateShiftCommand } from '../types';
import { useTranslation } from '@/shared/i18n';

export const CreateShiftPage = () => {
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const createMutation = useCreateShift();

  const handleSubmit = async (data: CreateShiftCommand) => {
    try {
      await createMutation.mutateAsync(data);
      setTimeout(() => {
        window.location.href = '/shifts';
      }, 500);
    } catch {
      // Error is handled and shown via toast by mutation
    }
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/shifts')}>
          <BackIcon className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {isRtl ? 'إضافة وردية عمل جديدة' : 'Create New Shift'}
          </h1>
          <p className="text-muted-foreground">
            {isRtl ? 'أدخل بيانات الوردية وأوقات الحضور والانصراف.' : 'Fill in the details to add a new shift to the system.'}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isRtl ? 'تفاصيل الوردية' : 'Shift Information'}</CardTitle>
          <CardDescription>
            {isRtl ? 'يرجى ملء جميع الحقول المطلوبة' : 'Please provide the required configuration for this work shift.'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ShiftForm
            onSubmit={handleSubmit}
            isLoading={createMutation.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
};
