import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { ShiftForm } from '../components';
import { useShift, useUpdateShift } from '../hooks';
import type { UpdateShiftCommand } from '../types';
import { useTranslation } from '@/shared/i18n';

export const EditShiftPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const { data: shift, isLoading: isLoadingShift, isError } = useShift(id || '');
  const updateMutation = useUpdateShift();

  const handleSubmit = async (data: UpdateShiftCommand) => {
    try {
      await updateMutation.mutateAsync(data);
      setTimeout(() => {
        window.location.href = '/shifts';
      }, 500);
    } catch {
      // Error handled by mutation toast
    }
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  if (isLoadingShift) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (isError || !shift) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
        <p className="text-destructive font-medium">
          {isRtl ? 'لم يتم العثور على الوردية' : 'Shift not found or failed to load'}
        </p>
        <Button variant="outline" onClick={() => navigate('/shifts')}>
          {isRtl ? 'العودة للورديات' : 'Back to Shifts'}
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/shifts')}>
          <BackIcon className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {isRtl ? 'تعديل الوردية' : 'Edit Shift'}
          </h1>
          <p className="text-muted-foreground">
            {isRtl ? `تعديل بيانات: ${shift.name}` : `Update configuration for: ${shift.name}`}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{isRtl ? 'تفاصيل الوردية' : 'Shift Information'}</CardTitle>
          <CardDescription>
            {isRtl ? 'قم بتعديل الحقول المطلوبة واضغط حفظ' : 'Modify the details below and save your changes.'}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ShiftForm
            initialData={shift}
            onSubmit={handleSubmit}
            isLoading={updateMutation.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
};
