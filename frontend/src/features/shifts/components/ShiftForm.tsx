import { useEffect } from 'react';
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
  SelectValue,
} from '@/shared/components/ui/select';
import { createShiftSchema, updateShiftSchema } from '../schemas/shiftSchema';
import { useCompanies } from '@/features/companies/hooks';
import { useWorkSchedules } from '../hooks';
import { ShiftType, ShiftTypeLabels, ShiftTypeArabicLabels } from '../types';
import type { ShiftDetailsDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface ShiftFormProps {
  initialData?: ShiftDetailsDto;
  onSubmit: (data: any) => void;
  isLoading: boolean;
}

export const ShiftForm = ({ initialData, onSubmit, isLoading }: ShiftFormProps) => {
  const { isRtl } = useTranslation();
  const isEditMode = !!initialData;

  const form = useForm<any>({
    resolver: zodResolver(isEditMode ? updateShiftSchema : createShiftSchema) as any,
    defaultValues: isEditMode
      ? {
          shiftId: initialData.id,
          name: initialData.name,
          code: initialData.code,
          description: initialData.description || '',
          startTime: initialData.startTime,
          endTime: initialData.endTime,
          shiftType: initialData.shiftType,
          isFlexible: initialData.isFlexible,
          minimumWorkingHours: initialData.minimumWorkingHours,
          maximumWorkingHours: initialData.maximumWorkingHours,
          gracePeriodMinutes: initialData.gracePeriodMinutes,
          allowEarlyCheckInMinutes: initialData.allowEarlyCheckInMinutes,
          allowLateCheckOutMinutes: initialData.allowLateCheckOutMinutes,
          maxBreakDurationMinutes: initialData.maxBreakDurationMinutes,
        }
      : {
          companyId: '',
          workScheduleId: '',
          name: '',
          code: '',
          description: '',
          startTime: '08:00',
          endTime: '16:00',
          shiftType: ShiftType.Morning,
          isFlexible: false,
          minimumWorkingHours: 8,
          maximumWorkingHours: 8,
          gracePeriodMinutes: 15,
          allowEarlyCheckInMinutes: 30,
          allowLateCheckOutMinutes: 30,
          maxBreakDurationMinutes: 60,
        },
  });

  const { data: companiesData, isLoading: isLoadingCompanies } = useCompanies({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const { data: schedulesData, isLoading: isLoadingSchedules } = useWorkSchedules();

  // Auto-select first company and schedule if creating
  useEffect(() => {
    if (!isEditMode) {
      if (companiesData?.items && companiesData.items.length > 0 && !form.getValues('companyId')) {
        form.setValue('companyId', companiesData.items[0].id);
      }
      if (schedulesData?.items && schedulesData.items.length > 0 && !form.getValues('workScheduleId')) {
        form.setValue('workScheduleId', schedulesData.items[0].id);
      }
    }
  }, [companiesData, schedulesData, isEditMode, form]);

  const companyItems = (companiesData?.items || []).map((c) => ({
    value: c.id,
    label: c.name,
  }));

  const scheduleItems = (schedulesData?.items || []).map((s) => ({
    value: s.id,
    label: s.name,
  }));

  const shiftTypeOptions = [
    { value: String(ShiftType.Morning), label: isRtl ? ShiftTypeArabicLabels[ShiftType.Morning] : ShiftTypeLabels[ShiftType.Morning] },
    { value: String(ShiftType.Afternoon), label: isRtl ? ShiftTypeArabicLabels[ShiftType.Afternoon] : ShiftTypeLabels[ShiftType.Afternoon] },
    { value: String(ShiftType.Night), label: isRtl ? ShiftTypeArabicLabels[ShiftType.Night] : ShiftTypeLabels[ShiftType.Night] },
    { value: String(ShiftType.FullDay), label: isRtl ? ShiftTypeArabicLabels[ShiftType.FullDay] : ShiftTypeLabels[ShiftType.FullDay] },
    { value: String(ShiftType.Flexible), label: isRtl ? ShiftTypeArabicLabels[ShiftType.Flexible] : ShiftTypeLabels[ShiftType.Flexible] },
  ];

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {!isEditMode && (
            <>
              <FormField
                control={form.control}
                name="companyId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'الشركة' : 'Company'}</FormLabel>
                    <Select
                      items={companyItems}
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isLoadingCompanies}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={isLoadingCompanies ? (isRtl ? 'جاري التحميل...' : 'Loading companies...') : (isRtl ? 'اختر الشركة' : 'Select a company')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {companiesData?.items.map((company) => (
                          <SelectItem key={company.id} value={company.id}>
                            {company.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="workScheduleId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'جدول العمل' : 'Work Schedule'}</FormLabel>
                    <Select
                      items={scheduleItems}
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isLoadingSchedules}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={isLoadingSchedules ? (isRtl ? 'جاري التحميل...' : 'Loading schedules...') : (isRtl ? 'اختر جدول العمل' : 'Select a work schedule')} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {schedulesData?.items.map((schedule) => (
                          <SelectItem key={schedule.id} value={schedule.id}>
                            {schedule.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </>
          )}

          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{isRtl ? 'اسم الوردية' : 'Shift Name'}</FormLabel>
                <FormControl>
                  <Input placeholder={isRtl ? 'مثال: الوردية الصباحية' : 'e.g. Morning Shift'} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{isRtl ? 'كود الوردية' : 'Shift Code'}</FormLabel>
                <FormControl>
                  <Input placeholder={isRtl ? 'مثال: SHIFT-MORN' : 'e.g. SHIFT-MORN'} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />


              <FormField
                control={form.control}
                name="shiftType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'نوع الوردية' : 'Shift Type'}</FormLabel>
                    <Select
                      items={shiftTypeOptions}
                      onValueChange={(val) => field.onChange(Number(val))}
                      value={String(field.value)}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={isRtl ? 'اختر نوع الوردية' : 'Select shift type'} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {shiftTypeOptions.map((opt) => (
                          <SelectItem key={opt.value} value={opt.value}>
                            {opt.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="startTime"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'وقت البدء' : 'Start Time'}</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="endTime"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'وقت الانتهاء' : 'End Time'}</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="minimumWorkingHours"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'الحد الأدنى لساعات العمل' : 'Min Working Hours'}</FormLabel>
                    <FormControl>
                      <Input type="number" step="0.5" min="1" max="24" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="maximumWorkingHours"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'الحد الأقصى لساعات العمل' : 'Max Working Hours'}</FormLabel>
                    <FormControl>
                      <Input type="number" step="0.5" min="1" max="24" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="gracePeriodMinutes"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'فترة السماح للتأخير (بالدقائق)' : 'Grace Period (Minutes)'}</FormLabel>
                    <FormControl>
                      <Input type="number" min="0" max="180" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="allowEarlyCheckInMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'سماح تسجيل مبكر (دقيقة)' : 'Early Check-In (Mins)'}</FormLabel>
                      <FormControl>
                        <Input type="number" min="0" max="240" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="allowLateCheckOutMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'سماح تسجيل خروج متأخر (دقيقة)' : 'Late Check-Out (Mins)'}</FormLabel>
                      <FormControl>
                        <Input type="number" min="0" max="240" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <FormField
                control={form.control}
                name="maxBreakDurationMinutes"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'الحد الأقصى لمدة الاستراحة (دقيقة)' : 'Max Break Duration (Mins)'}</FormLabel>
                    <FormControl>
                      <Input type="number" min="0" max="240" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem className="col-span-1 md:col-span-2">
                <FormLabel>{isRtl ? 'الوصف' : 'Description'}</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder={isRtl ? 'أدخل وصفاً للوردية...' : 'Enter shift description...'}
                    className="resize-none"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="flex justify-end gap-4 border-t pt-6">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? (isRtl ? 'جاري الحفظ...' : 'Saving...') : (isRtl ? 'حفظ الوردية' : 'Save Shift')}
          </Button>
        </div>
      </form>
    </Form>
  );
};
