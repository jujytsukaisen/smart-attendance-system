import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  ArrowRight,
  Edit,
  Trash,
  Clock,
  CheckCircle2,
  XCircle,
  Hourglass,
  PowerOff,
  Activity,
} from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { ActionDialog } from '../components/ActionDialog';
import {
  useShift,
  useDeleteShift,
  useActivateShift,
  useDeactivateShift,
} from '../hooks';
import { ShiftTypeLabels, ShiftTypeArabicLabels } from '../types';
import { useTranslation } from '@/shared/i18n';

export const ShiftDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const { data: shift, isLoading, isError } = useShift(id || '');

  const [dialogAction, setDialogAction] = useState<'delete' | 'activate' | 'deactivate' | null>(null);

  const deleteMutation = useDeleteShift();
  const activateMutation = useActivateShift();
  const deactivateMutation = useDeactivateShift();

  const handleAction = async () => {
    if (!shift || !dialogAction) return;

    try {
      if (dialogAction === 'delete') {
        await deleteMutation.mutateAsync(shift.id);
        navigate('/shifts');
      } else if (dialogAction === 'activate') {
        await activateMutation.mutateAsync(shift.id);
      } else if (dialogAction === 'deactivate') {
        await deactivateMutation.mutateAsync(shift.id);
      }
    } finally {
      setDialogAction(null);
    }
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  if (isLoading) {
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
          {isRtl ? 'لم يتم العثور على بيانات الوردية' : 'Shift details could not be found'}
        </p>
        <Button variant="outline" onClick={() => navigate('/shifts')}>
          {isRtl ? 'العودة للورديات' : 'Back to Shifts'}
        </Button>
      </div>
    );
  }

  const shiftTypeName = isRtl
    ? ShiftTypeArabicLabels[shift.shiftType] || 'أخرى'
    : ShiftTypeLabels[shift.shiftType] || 'Other';

  return (
    <div className="flex flex-col gap-6 p-6 max-w-5xl mx-auto w-full">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/shifts')}>
            <BackIcon className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold tracking-tight">{shift.name}</h1>
              <Badge variant="outline" className="font-mono text-xs">
                {shift.code}
              </Badge>
              {shift.isActive ? (
                <Badge variant="outline" className="bg-emerald-500/10 text-emerald-600 border-emerald-200">
                  {isRtl ? 'نشط' : 'Active'}
                </Badge>
              ) : (
                <Badge variant="outline" className="bg-muted text-muted-foreground">
                  {isRtl ? 'غير نشط' : 'Inactive'}
                </Badge>
              )}
            </div>
            <p className="text-muted-foreground mt-0.5">
              {shift.description || (isRtl ? 'لا يوجد وصف مضاف' : 'No description provided')}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => navigate(`/shifts/${shift.id}/edit`)} className="gap-2">
            <Edit className="h-4 w-4" />
            <span>{isRtl ? 'تعديل' : 'Edit'}</span>
          </Button>
          {shift.isActive ? (
            <Button
              variant="outline"
              onClick={() => setDialogAction('deactivate')}
              className="gap-2 text-amber-600 hover:text-amber-700"
            >
              <PowerOff className="h-4 w-4" />
              <span>{isRtl ? 'إلغاء التفعيل' : 'Deactivate'}</span>
            </Button>
          ) : (
            <Button
              variant="outline"
              onClick={() => setDialogAction('activate')}
              className="gap-2 text-emerald-600 hover:text-emerald-700"
            >
              <Activity className="h-4 w-4" />
              <span>{isRtl ? 'تفعيل' : 'Activate'}</span>
            </Button>
          )}
          <Button
            variant="outline"
            onClick={() => setDialogAction('delete')}
            className="gap-2 text-destructive hover:text-destructive"
          >
            <Trash className="h-4 w-4" />
            <span>{isRtl ? 'حذف' : 'Delete'}</span>
          </Button>
        </div>
      </div>

      {/* Grid of details */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Schedule & Timing Card */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Clock className="h-5 w-5 text-primary" />
              <span>{isRtl ? 'أوقات وساعات العمل' : 'Timing & Working Hours'}</span>
            </CardTitle>
            <CardDescription>
              {isRtl ? 'تفاصيل جدول الدوام والحضور' : 'Shift working times and constraints.'}
            </CardDescription>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'نوع الوردية' : 'Shift Type'}</span>
              <p className="text-base font-semibold">{shiftTypeName}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'فترة الدوام' : 'Working Time'}</span>
              <p className="text-base font-semibold font-mono">{shift.startTime} - {shift.endTime}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'الحد الأدنى لساعات العمل' : 'Min Working Hours'}</span>
              <p className="text-base font-semibold">{shift.minimumWorkingHours} {isRtl ? 'ساعات' : 'hours'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'الحد الأقصى لساعات العمل' : 'Max Working Hours'}</span>
              <p className="text-base font-semibold">{shift.maximumWorkingHours} {isRtl ? 'ساعات' : 'hours'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'دوام مرن' : 'Flexible Shift'}</span>
              <p className="text-base font-semibold">
                {shift.isFlexible ? (
                  <span className="text-emerald-600 flex items-center gap-1">
                    <CheckCircle2 className="h-4 w-4" /> {isRtl ? 'نعم' : 'Yes'}
                  </span>
                ) : (
                  <span className="text-muted-foreground flex items-center gap-1">
                    <XCircle className="h-4 w-4" /> {isRtl ? 'لا' : 'No'}
                  </span>
                )}
              </p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'وردية ليلية' : 'Night Shift'}</span>
              <p className="text-base font-semibold">
                {shift.isNightShift ? (
                  <span className="text-indigo-600 flex items-center gap-1">
                    <CheckCircle2 className="h-4 w-4" /> {isRtl ? 'نعم' : 'Yes'}
                  </span>
                ) : (
                  <span className="text-muted-foreground flex items-center gap-1">
                    <XCircle className="h-4 w-4" /> {isRtl ? 'لا' : 'No'}
                  </span>
                )}
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Rules & Grace Periods Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Hourglass className="h-5 w-5 text-primary" />
              <span>{isRtl ? 'قواعد السماحية' : 'Grace Periods'}</span>
            </CardTitle>
            <CardDescription>
              {isRtl ? 'فترات السماح للتسجيل' : 'Check-in and check-out rules.'}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'سماح التأخير' : 'Grace Period'}</span>
              <p className="text-base font-semibold">{shift.gracePeriodMinutes} {isRtl ? 'دقيقة' : 'minutes'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'سماح الحضور المبكر' : 'Early Check-In'}</span>
              <p className="text-base font-semibold">{shift.allowEarlyCheckInMinutes} {isRtl ? 'دقيقة' : 'minutes'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'سماح الانصراف المتأخر' : 'Late Check-Out'}</span>
              <p className="text-base font-semibold">{shift.allowLateCheckOutMinutes} {isRtl ? 'دقيقة' : 'minutes'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm font-medium text-muted-foreground">{isRtl ? 'أقصى مدة للراحة' : 'Max Break Duration'}</span>
              <p className="text-base font-semibold">{shift.maxBreakDurationMinutes} {isRtl ? 'دقيقة' : 'minutes'}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Action Dialog */}
      <ActionDialog
        isOpen={!!dialogAction}
        onClose={() => setDialogAction(null)}
        onConfirm={handleAction}
        title={
          dialogAction === 'delete'
            ? (isRtl ? 'حذف الوردية' : 'Delete Shift')
            : dialogAction === 'activate'
            ? (isRtl ? 'تفعيل الوردية' : 'Activate Shift')
            : (isRtl ? 'إلغاء تفعيل الوردية' : 'Deactivate Shift')
        }
        description={
          dialogAction === 'delete'
            ? (isRtl ? `هل أنت متأكد من حذف وردية "${shift.name}"؟` : `Are you sure you want to delete "${shift.name}"?`)
            : dialogAction === 'activate'
            ? (isRtl ? `هل تريد تفعيل وردية "${shift.name}"؟` : `Are you sure you want to activate "${shift.name}"?`)
            : (isRtl ? `هل تريد إلغاء تفعيل وردية "${shift.name}"؟` : `Are you sure you want to deactivate "${shift.name}"?`)
        }
        confirmText={
          dialogAction === 'delete'
            ? (isRtl ? 'حذف' : 'Delete')
            : dialogAction === 'activate'
            ? (isRtl ? 'تفعيل' : 'Activate')
            : (isRtl ? 'إلغاء التفعيل' : 'Deactivate')
        }
        variant={dialogAction === 'activate' ? 'default' : 'danger'}
        isLoading={deleteMutation.isPending || activateMutation.isPending || deactivateMutation.isPending}
      />
    </div>
  );
};
