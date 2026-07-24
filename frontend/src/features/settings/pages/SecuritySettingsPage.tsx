import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { Input } from '@/shared/components/ui/input';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { SettingsSaveBar } from '../components';
import { useSecuritySettings, useUpdateSecuritySettings } from '../hooks';
import { securitySettingsSchema } from '../schemas';
import type { SecuritySettingsDto } from '../types';

export function SecuritySettingsPage() {
  const { data, isLoading, isError, error } = useSecuritySettings();
  const updateMutation = useUpdateSecuritySettings();

  const form = useForm<SecuritySettingsDto>({
    resolver: zodResolver(securitySettingsSchema),
    defaultValues: data,
  });

  useEffect(() => {
    if (data) {
      form.reset(data);
    }
  }, [data, form]);

  const onSubmit = async (values: SecuritySettingsDto) => {
    await updateMutation.mutateAsync(values);
    form.reset(values);
    toast.success('Security settings updated successfully');
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
          <CardTitle>Security Configuration</CardTitle>
          <CardDescription>Manage password policies and session lifetimes.</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form id="security-settings-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                
                <FormField
                  control={form.control}
                  name="twoFactorAuthEnabled"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 col-span-1 md:col-span-2">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Two-Factor Authentication</FormLabel>
                        <CardDescription>Require users to verify their identity via 2FA on login.</CardDescription>
                      </div>
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="passwordPolicy"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Password Policy</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value}>
                        <FormControl>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="Standard">Standard (8 chars, 1 number)</SelectItem>
                          <SelectItem value="Strong">Strong (12 chars, special, uppercase)</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="sessionTimeoutMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Session Timeout (Minutes)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="jwtLifetimeMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>JWT Lifetime (Minutes)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="refreshTokenLifetimeDays"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Refresh Token Lifetime (Days)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="maxLoginAttempts"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Max Login Attempts</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="lockoutDurationMinutes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Lockout Duration (Minutes)</FormLabel>
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
