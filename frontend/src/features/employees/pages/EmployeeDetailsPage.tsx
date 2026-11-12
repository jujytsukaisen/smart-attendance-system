import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, ArrowRight, Edit, Trash, Activity, PowerOff, User, MapPin, Briefcase } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { EmployeeStatusBadge, ActionDialog } from '../components';
import { useEmployee, useActivateEmployee, useDeactivateEmployee, useDeleteEmployee } from '../hooks';
import { FaceRegistrationCard } from '@/features/face-recognition';
import { useTranslation } from '@/shared/i18n';

export const EmployeeDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const [dialogAction, setDialogAction] = useState<'delete' | 'activate' | 'deactivate' | null>(null);

  const { data: employee, isLoading, isError, error } = useEmployee(id!);
  const activateMutation = useActivateEmployee();
  const deactivateMutation = useDeactivateEmployee();
  const deleteMutation = useDeleteEmployee();

  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : (isRtl ? 'فشل تحميل بيانات الموظف' : 'Failed to load')}
        </p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex justify-center p-8">
        <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!employee) {
    return (
      <div className="p-8 text-center text-muted-foreground">
        {isRtl ? 'لم يتم العثور على الموظف' : 'Employee not found'}
      </div>
    );
  }

  const handleAction = async () => {
    try {
      if (dialogAction === 'delete') {
        await deleteMutation.mutateAsync(id!);
        navigate('/employees');
      } else if (dialogAction === 'activate') {
        await activateMutation.mutateAsync(id!);
        setDialogAction(null);
      } else if (dialogAction === 'deactivate') {
        await deactivateMutation.mutateAsync(id!);
        setDialogAction(null);
      }
    } catch (error) {
      // Error handled by hook
    }
  };

  const getDialogConfig = () => {
    switch (dialogAction) {
      case 'delete':
        return {
          title: t('employees.deleteEmployee'),
          description: isRtl
            ? `هل أنت متأكد من رغبتك في حذف هذا الموظف؟ يمكن التراجع عن هذا الإجراء لاحقاً باستعادته.`
            : 'Are you sure you want to delete this employee? This can be undone later via restore.',
          confirmText: t('common.delete'),
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'activate':
        return {
          title: t('employees.activateEmployee'),
          description: isRtl
            ? 'هل أنت متأكد من رغبتك في تفعيل هذا الموظف؟'
            : 'Are you sure you want to activate this employee?',
          confirmText: t('employees.activate'),
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: t('employees.deactivateEmployee'),
          description: isRtl
            ? 'هل أنت متأكد من رغبتك في تعطيل هذا الموظف؟ لن يتمكن من تسجيل الدخول.'
            : 'Are you sure you want to deactivate this employee? They will not be able to log in.',
          confirmText: t('employees.deactivate'),
          variant: 'danger' as const,
          isLoading: deactivateMutation.isPending,
        };
      default:
        return null;
    }
  };

  const dialogConfig = getDialogConfig();
  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  const getGenderLabel = (g: any) => {
    if (g === 1 || g === 'Male') return isRtl ? 'ذكر' : 'Male';
    if (g === 2 || g === 'Female') return isRtl ? 'أنثى' : 'Female';
    if (g === 3 || g === 'Other') return isRtl ? 'أخرى' : 'Other';
    if (g === 4 || g === 'PreferNotToSay') return isRtl ? 'أفضل عدم التحديد' : 'Prefer not to say';
    return isRtl ? 'غير محدد' : 'Unknown';
  };

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/employees')}>
            <BackIcon className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold tracking-tight text-foreground">
                {employee.firstName} {employee.lastName}
              </h1>
              <EmployeeStatusBadge status={employee.employeeStatus} />
            </div>
            <p className="text-muted-foreground mt-1 font-mono text-sm">
              {employee.employeeNumber} | {employee.jobTitle || '—'}
            </p>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          {employee.employeeStatus === 'Active' ? (
            <Button variant="outline" onClick={() => setDialogAction('deactivate')} className="gap-2">
              <PowerOff className="h-4 w-4" /> <span>{t('employees.deactivate')}</span>
            </Button>
          ) : (
            <Button variant="outline" onClick={() => setDialogAction('activate')} className="gap-2">
              <Activity className="h-4 w-4" /> <span>{t('employees.activate')}</span>
            </Button>
          )}
          <Button variant="outline" onClick={() => navigate(`/employees/${id}/edit`)} className="gap-2">
            <Edit className="h-4 w-4" /> <span>{t('common.edit')}</span>
          </Button>
          <Button variant="destructive" onClick={() => setDialogAction('delete')} className="gap-2">
            <Trash className="h-4 w-4" /> <span>{t('common.delete')}</span>
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* Personal Information */}
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="pb-3 border-b border-border/50">
            <CardTitle className="text-sm font-semibold text-foreground flex items-center gap-2">
              <User className="h-4 w-4 text-primary" /> {isRtl ? 'المعلومات الشخصية' : 'Personal Information'}
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4 space-y-3">
            <div className="grid grid-cols-2 text-sm gap-y-2.5">
              <div className="text-muted-foreground">{t('employees.email')}</div>
              <div className="font-medium font-mono text-xs truncate" title={employee.email}>{employee.email}</div>

              <div className="text-muted-foreground">{t('employees.phone')}</div>
              <div className="font-medium font-mono text-xs">{employee.phoneNumber || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'تاريخ الميلاد' : 'Date of Birth'}</div>
              <div className="font-medium">
                {employee.dateOfBirth ? new Date(employee.dateOfBirth).toLocaleDateString(isRtl ? 'ar-SA' : 'en-US') : '—'}
              </div>

              <div className="text-muted-foreground">{isRtl ? 'الجنس' : 'Gender'}</div>
              <div className="font-medium">{getGenderLabel(employee.gender)}</div>

              {employee.nationalId && (
                <>
                  <div className="text-muted-foreground">{isRtl ? 'الهوية الوطنية / الإقامة' : 'National ID'}</div>
                  <div className="font-medium font-mono">{employee.nationalId}</div>
                </>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Job Information */}
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="pb-3 border-b border-border/50">
            <CardTitle className="text-sm font-semibold text-foreground flex items-center gap-2">
              <Briefcase className="h-4 w-4 text-primary" /> {isRtl ? 'المعلومات الوظيفية' : 'Job Information'}
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4 space-y-3">
            <div className="grid grid-cols-2 text-sm gap-y-2.5">
              <div className="text-muted-foreground">{isRtl ? 'الشركة' : 'Company'}</div>
              <div className="font-medium">{employee.companyName || employee.companyId || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'الفرع' : 'Branch'}</div>
              <div className="font-medium">{employee.branchName || employee.branchId || '—'}</div>

              <div className="text-muted-foreground">{t('employees.department')}</div>
              <div className="font-medium">{employee.departmentName || employee.departmentId || '—'}</div>

              <div className="text-muted-foreground">{t('employees.hireDate')}</div>
              <div className="font-medium">
                {employee.hireDate ? new Date(employee.hireDate).toLocaleDateString(isRtl ? 'ar-SA' : 'en-US') : '—'}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Address */}
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="pb-3 border-b border-border/50">
            <CardTitle className="text-sm font-semibold text-foreground flex items-center gap-2">
              <MapPin className="h-4 w-4 text-primary" /> {isRtl ? 'العنوان والموقع' : 'Address'}
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-4 space-y-3">
            <div className="grid grid-cols-2 text-sm gap-y-2.5">
              <div className="text-muted-foreground">{isRtl ? 'الشارع' : 'Street'}</div>
              <div className="font-medium truncate" title={employee.street}>{employee.street || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'المدينة' : 'City'}</div>
              <div className="font-medium">{employee.city || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'المنطقة / المحافظة' : 'State'}</div>
              <div className="font-medium">{employee.state || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'الدولة' : 'Country'}</div>
              <div className="font-medium">{employee.country || '—'}</div>

              <div className="text-muted-foreground">{isRtl ? 'الرمز البريدي' : 'Zip Code'}</div>
              <div className="font-medium font-mono">{employee.zipCode || '—'}</div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Face Biometric Registration Card */}
      <FaceRegistrationCard employeeId={id!} />

      {dialogConfig && (
        <ActionDialog
          isOpen={!!dialogAction}
          onClose={() => setDialogAction(null)}
          onConfirm={handleAction}
          title={dialogConfig.title}
          description={dialogConfig.description}
          confirmText={dialogConfig.confirmText}
          variant={dialogConfig.variant}
          isLoading={dialogConfig.isLoading}
        />
      )}
    </div>
  );
};