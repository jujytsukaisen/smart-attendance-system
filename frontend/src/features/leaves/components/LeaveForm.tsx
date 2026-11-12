import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Textarea } from '@/shared/components/ui/textarea';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/shared/components/ui/form';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/shared/components/ui/select';
import { useEmployees } from '@/features/employees/hooks';
import { leaveSchema, type LeaveFormValues } from '../schemas';
import { LeaveType } from '../types';
import { useTranslation } from '@/shared/i18n';

interface LeaveFormProps {
  initialValues?: Partial<LeaveFormValues>;
  onSubmit: (values: LeaveFormValues) => void;
  isLoading?: boolean;
  isEmployeeView?: boolean;
}

const defaultValues: LeaveFormValues = {
  employeeId: '',
  leaveType: LeaveType.Annual.toString(),
  startDate: new Date().toISOString().slice(0, 10),
  endDate: new Date().toISOString().slice(0, 10),
  reason: '',
};

export function LeaveForm({ initialValues, onSubmit, isLoading, isEmployeeView = false }: LeaveFormProps) {
  const { t, isRtl } = useTranslation();
  const { data: employeesData, isLoading: isLoadingEmployees } = useEmployees({ pageNumber: 1, pageSize: 100 }, !isEmployeeView);

  const LEAVE_TYPES = [
    { value: LeaveType.Annual.toString(), label: t('leaves.annual') },
    { value: LeaveType.Sick.toString(), label: t('leaves.sick') },
    { value: LeaveType.Maternity.toString(), label: t('leaves.maternity') },
    { value: LeaveType.Paternity.toString(), label: t('leaves.paternity') },
    { value: LeaveType.Unpaid.toString(), label: t('leaves.unpaid') },
    { value: LeaveType.Other.toString(), label: t('leaves.other') },
  ];

  const form = useForm<LeaveFormValues>({
    resolver: zodResolver(leaveSchema),
    defaultValues: {
      ...defaultValues,
      ...initialValues,
      leaveType: initialValues?.leaveType?.toString() || defaultValues.leaveType,
    },
  });

  const handleFormSubmit = (values: LeaveFormValues) => {
    const selectedEmp = employeesData?.items.find(
      (e) => e.id.toLowerCase() === values.employeeId?.toLowerCase()
    );
    
    const payload = {
      ...values,
      companyId: selectedEmp?.companyId || values.companyId,
    };
    
    if (!payload.employeeId) {
      delete payload.employeeId;
    }
    
    onSubmit(payload);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-6 bg-card text-card-foreground p-6 rounded-lg shadow-sm border border-border">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {!isEmployeeView && (
            <FormField
              control={form.control}
              name="employeeId"
              render={({ field }) => {
                const selectedEmp = employeesData?.items.find(
                  (e) => e.id.toLowerCase() === field.value?.toLowerCase()
                );

                return (
                  <FormItem>
                    <FormLabel>{t('attendance.employee')}</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={isLoadingEmployees ? (isRtl ? 'جاري التحميل...' : 'Loading employees...') : (isRtl ? 'اختر الموظف' : 'Select an employee')}>
                            {selectedEmp
                              ? `${selectedEmp.firstName} ${selectedEmp.lastName} (${selectedEmp.employeeNumber})`
                              : undefined}
                          </SelectValue>
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {employeesData?.items.map((emp) => (
                          <SelectItem key={emp.id} value={emp.id}>
                            {emp.firstName} {emp.lastName} ({emp.employeeNumber})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                );
              }}
            />
          )}

          <FormField
            control={form.control}
            name="leaveType"
            render={({ field }) => {
              const selectedType = LEAVE_TYPES.find(
                (t) => t.value === String(field.value)
              );

              return (
                <FormItem>
                  <FormLabel>{t('leaves.leaveType')}</FormLabel>
                  <Select onValueChange={field.onChange} value={String(field.value)}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={isRtl ? 'اختر نوع الإجازة' : 'Select leave type'}>
                          {selectedType ? selectedType.label : field.value}
                        </SelectValue>
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {LEAVE_TYPES.map((t) => (
                        <SelectItem key={t.value} value={t.value}>
                          {t.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              );
            }}
          />

          <FormField
            control={form.control}
            name="startDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t('leaves.startDate')}</FormLabel>
                <FormControl>
                  <Input type="date" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="endDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t('leaves.endDate')}</FormLabel>
                <FormControl>
                  <Input type="date" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="reason"
          render={({ field }) => (
            <FormItem>
              <FormLabel>{t('leaves.reason')}</FormLabel>
              <FormControl>
                <Textarea 
                  placeholder={isRtl ? 'يرجى كتابة سبب طلب الإجازة بالتفصيل...' : 'Please provide a reason for your leave request...'} 
                  className="resize-none h-24"
                  {...field} 
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end pt-4">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? (isRtl ? 'جاري الحفظ...' : 'Saving...') : (isRtl ? 'حفظ طلب الإجازة' : 'Save Leave Request')}
          </Button>
        </div>
      </form>
    </Form>
  );
}
