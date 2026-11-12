import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { Input } from '@/shared/components/ui/input';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { SettingsSaveBar } from '../components';
import { useDeviceSettings, useUpdateDeviceSettings } from '../hooks';
import { deviceSettingsSchema } from '../schemas';
import type { DeviceSettingsDto } from '../types';

export function DeviceSettingsPage() {
  const { data, isLoading, isError, error } = useDeviceSettings();
  const updateMutation = useUpdateDeviceSettings();

  const form = useForm<DeviceSettingsDto>({
    resolver: zodResolver(deviceSettingsSchema),
    values: data || {} as any,
  });

  const onSubmit = async (values: DeviceSettingsDto) => {
    await updateMutation.mutateAsync(values);
    form.reset(values);
    toast.success('Device settings updated successfully');
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
          <CardTitle>Biometric Devices</CardTitle>
          <CardDescription>Configure syncing and heartbeat monitoring for physical devices.</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form id="device-settings-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              
              <FormField
                control={form.control}
                name="autoSyncEnabled"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 mb-6">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Automatic Syncing</FormLabel>
                      <CardDescription>Automatically pull attendance logs from devices.</CardDescription>
                    </div>
                    <FormControl>
                      <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                    </FormControl>
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <FormField
                  control={form.control}
                  name="defaultDeviceTimeoutSeconds"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Default Timeout (Seconds)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="heartbeatIntervalSeconds"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Heartbeat Interval (Seconds)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="connectionTimeoutSeconds"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Connection Timeout (Seconds)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="retryAttempts"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Sync Retry Attempts</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
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
