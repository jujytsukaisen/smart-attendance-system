import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { Input } from '@/shared/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { SettingsSaveBar } from '../components';
import { useAttendanceSettings, useUpdateAttendanceSettings } from '../hooks';
import { attendanceSettingsSchema } from '../schemas';
import type { AttendanceSettingsDto } from '../types';

export function AttendanceSettingsPage() {
  const { data, isLoading, isError, error } = useAttendanceSettings();
  const updateMutation = useUpdateAttendanceSettings();

  const form = useForm<AttendanceSettingsDto>({
    resolver: zodResolver(attendanceSettingsSchema),
    defaultValues: data,
  });

  useEffect(() => {
    if (data) {
      form.reset(data);
    }
  }, [data, form]);

  const onSubmit = async (values: AttendanceSettingsDto) => {
    await updateMutation.mutateAsync(values);
    form.reset(values);
    toast.success('Attendance settings updated successfully');
  };

  if (isLoading) return <Skeleton className="h-[400px] w-full" />;
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }



  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Attendance Rules</CardTitle>
          <CardDescription>Configure organizational tracking rules.</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form id="attendance-settings-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <FormField
                  control={form.control}
                  name="workingHoursStart"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Working Hours Start</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="workingHoursEnd"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Working Hours End</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} />
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
                      <FormLabel>Grace Period (Minutes)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="lateThresholdMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Late Threshold (Minutes)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="minimumHoursForHalfDay"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Min Hours for Half Day</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="minimumHoursForFullDay"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Min Hours for Full Day</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="maximumOvertimeHours"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Max Overtime Hours</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="weekendRules"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Weekend Rules</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value}>
                        <FormControl>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="Strict">Strict (No attendance allowed)</SelectItem>
                          <SelectItem value="Overtime">Treated as Overtime</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
      
      <SettingsSaveBar 
        isDirty={form.formState.isDirty} 
        isSaving={updateMutation.isPending}
        onReset={() => form.reset(data)}
        onSave={form.handleSubmit(onSubmit)}
      />
    </div>
  );
}
