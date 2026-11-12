import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
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
import { useEmployees } from '@/features/employees/hooks';
import { useShifts } from '@/features/shifts/hooks';
import { useAttendancePolicies } from '@/features/attendance-policies/hooks';
import { useDevices } from '@/features/devices/hooks';
import { manualAttendanceSchema, type ManualAttendanceFormValues } from '../schemas';
import { CheckInMethod } from '../types';
import { useTranslation } from '@/shared/i18n';

interface AttendanceFormProps {
  onSubmit: (values: ManualAttendanceFormValues) => void;
  isLoading?: boolean;
}

function getLocalDatetimeString(date: Date = new Date()): string {
  const offset = date.getTimezoneOffset() * 60000;
  const localDate = new Date(date.getTime() - offset);
  return localDate.toISOString().slice(0, 16);
}

const defaultValues: ManualAttendanceFormValues = {
  employeeId: '',
  attendancePolicyId: '',
  shiftId: '',
  attendanceDeviceId: '',
  checkInTime: getLocalDatetimeString(),
  checkOutTime: '',
  checkInMethod: CheckInMethod.Manual,
};

export function AttendanceForm({ onSubmit, isLoading }: AttendanceFormProps) {
  const { t, isRtl } = useTranslation();
  const form = useForm<ManualAttendanceFormValues>({
    resolver: zodResolver(manualAttendanceSchema),
    defaultValues: defaultValues,
  });

  const { data: employeesData, isLoading: isLoadingEmployees } = useEmployees({
    pageNumber: 1,
    pageSize: 100,
  });

  const { data: shiftsData, isLoading: isLoadingShifts } = useShifts({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const { data: policiesData, isLoading: isLoadingPolicies } = useAttendancePolicies({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const { data: devicesData, isLoading: isLoadingDevices } = useDevices({
    pageNumber: 1,
    pageSize: 100,
  });

  const employees = employeesData?.items || [];
  const shifts = shiftsData?.items || [];
  const policies = policiesData?.items || [];
  const devices = devicesData?.items || [];

  // Auto-select first shift when loaded if not yet selected
  useEffect(() => {
    if (shifts.length > 0 && !form.getValues('shiftId')) {
      form.setValue('shiftId', shifts[0].id);
    }
  }, [shifts, form]);

  // Auto-select first policy when loaded if not yet selected
  useEffect(() => {
    if (policies.length > 0 && !form.getValues('attendancePolicyId')) {
      form.setValue('attendancePolicyId', policies[0].id);
    }
  }, [policies, form]);

  // Auto-select first device when loaded if not yet selected
  useEffect(() => {
    if (devices.length > 0 && !form.getValues('attendanceDeviceId')) {
      form.setValue('attendanceDeviceId', devices[0].id);
    }
  }, [devices, form]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 bg-card text-card-foreground shadow-sm border border-border px-6 py-6 sm:rounded-lg">
        <FormField
          control={form.control}
          name="employeeId"
          render={({ field }) => {
            const selectedEmployee = employees.find((e) => e.id === field.value);
            return (
              <FormItem>
                <FormLabel>{t('attendance.employee')} *</FormLabel>
                <Select
                  onValueChange={(val) => {
                    field.onChange(val);
                  }}
                  value={field.value}
                  disabled={isLoadingEmployees}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder={isLoadingEmployees ? (isRtl ? 'جاري تحميل الموظفين...' : 'Loading employees...') : (isRtl ? 'اختر الموظف' : 'Select Employee')}>
                        {selectedEmployee ? `${selectedEmployee.firstName} ${selectedEmployee.lastName} (${selectedEmployee.employeeNumber})` : undefined}
                      </SelectValue>
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {employees.map((emp) => (
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

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField
            control={form.control}
            name="shiftId"
            render={({ field }) => {
              const selectedShift = shifts.find((s) => s.id === field.value);
              return (
                <FormItem>
                  <FormLabel>{isRtl ? 'وردية العمل' : 'Shift'}</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                    disabled={isLoadingShifts}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={isLoadingShifts ? (isRtl ? 'جاري تحميل الورديات...' : 'Loading shifts...') : (isRtl ? 'اختر الوردية' : 'Select Shift')}>
                          {selectedShift ? `${selectedShift.name} (${selectedShift.startTime.slice(0, 5)} - ${selectedShift.endTime.slice(0, 5)})` : undefined}
                        </SelectValue>
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {shifts.map((shift) => (
                        <SelectItem key={shift.id} value={shift.id}>
                          {shift.name} ({shift.startTime.slice(0, 5)} - {shift.endTime.slice(0, 5)})
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
            name="attendancePolicyId"
            render={({ field }) => {
              const selectedPolicy = policies.find((p) => p.id === field.value);
              return (
                <FormItem>
                  <FormLabel>{isRtl ? 'سياسة الحضور' : 'Attendance Policy'}</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                    disabled={isLoadingPolicies}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={isLoadingPolicies ? (isRtl ? 'جاري تحميل السياسات...' : 'Loading policies...') : (isRtl ? 'اختر سياسة الحضور' : 'Select Policy')}>
                          {selectedPolicy ? `${selectedPolicy.name} (${selectedPolicy.code})` : undefined}
                        </SelectValue>
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {policies.map((policy) => (
                        <SelectItem key={policy.id} value={policy.id}>
                          {policy.name} ({policy.code})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              );
            }}
          />
        </div>

        <FormField
          control={form.control}
          name="attendanceDeviceId"
          render={({ field }) => {
            const selectedDevice = devices.find((d) => d.id === field.value);
            return (
              <FormItem>
                <FormLabel>{isRtl ? 'جهاز التسجيل / البوابة (اختياري)' : 'Device / Terminal (Optional)'}</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={isLoadingDevices || devices.length === 0}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue
                        placeholder={
                          isLoadingDevices
                            ? (isRtl ? 'جاري تحميل الأجهزة...' : 'Loading devices...')
                            : devices.length === 0
                              ? (isRtl ? 'لا توجد أجهزة مسجلة (تسجيل عبر بوابة الويب)' : 'No physical devices registered (Web / Portal Entry)')
                              : (isRtl ? 'اختر الجهاز (اختياري)' : 'Select Device (Optional)')
                        }
                      >
                        {selectedDevice ? `${selectedDevice.name} (${selectedDevice.deviceType})` : undefined}
                      </SelectValue>
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {devices.map((device) => (
                      <SelectItem key={device.id} value={device.id}>
                        {device.name} ({device.deviceType})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <p className="text-xs text-muted-foreground mt-1">
                  {isRtl ? 'اختياري. اتركه فارغاً لتوثيق التسجيل عبر بوابة الويب يدوياً.' : 'Optional. Leave empty to record as Web Portal / Manual entry.'}
                </p>
                <FormMessage />
              </FormItem>
            );
          }}
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField
            control={form.control}
            name="checkInTime"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t('attendance.checkIn')} *</FormLabel>
                <FormControl>
                  <Input type="datetime-local" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="checkOutTime"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{t('attendance.checkOut')} ({isRtl ? 'اختياري' : 'Optional'})</FormLabel>
                <FormControl>
                  <Input
                    type="datetime-local"
                    value={field.value || ''}
                    onChange={(e) => field.onChange(e.target.value)}
                  />
                </FormControl>
                <p className="text-xs text-muted-foreground mt-1">
                  {isRtl ? 'اتركه فارغاً إذا كان الموظف ما زال في فترة العمل.' : 'Leave empty if employee is currently checked in.'}
                </p>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t border-border">
          <Button
            type="button"
            variant="outline"
            onClick={() => window.history.back()}
            disabled={isLoading}
          >
            {t('common.cancel')}
          </Button>
          <Button
            type="submit"
            disabled={isLoading}
          >
            {isLoading ? t('common.loading') : (isRtl ? 'حفظ السجل' : 'Save Record')}
          </Button>
        </div>
      </form>
    </Form>
  );
}
