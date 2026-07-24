import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { Input } from '@/shared/components/ui/input';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { SettingsSaveBar } from '../components';
import { useFaceRecognitionSettings, useUpdateFaceRecognitionSettings } from '../hooks';
import { faceRecognitionSettingsSchema } from '../schemas';
import type { FaceRecognitionSettingsDto } from '../types';

export function FaceRecognitionSettingsPage() {
  const { data, isLoading, isError, error } = useFaceRecognitionSettings();
  const updateMutation = useUpdateFaceRecognitionSettings();

  const form = useForm<FaceRecognitionSettingsDto>({
    resolver: zodResolver(faceRecognitionSettingsSchema),
    defaultValues: data,
  });

  useEffect(() => {
    if (data) {
      form.reset(data);
    }
  }, [data, form]);

  const onSubmit = async (values: FaceRecognitionSettingsDto) => {
    await updateMutation.mutateAsync(values);
    form.reset(values);
    toast.success('Face recognition settings updated successfully');
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
          <CardTitle>Face Recognition</CardTitle>
          <CardDescription>Adjust AI thresholds for biometric matching.</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form id="face-settings-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                <FormField
                  control={form.control}
                  name="recognitionThreshold"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Recognition Confidence Threshold (%)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="minimumFaceQuality"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Minimum Face Quality (%)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={e => field.onChange(Number(e.target.value))} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="space-y-4">
                <FormField
                  control={form.control}
                  name="livenessDetectionEnabled"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 bg-muted/20">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Liveness Detection</FormLabel>
                        <CardDescription>Require active blink or movement to prevent photo spoofing.</CardDescription>
                      </div>
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="spoofDetectionEnabled"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 bg-muted/20">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Anti-Spoofing Model</FormLabel>
                        <CardDescription>Enable advanced deepfake and mask detection AI models.</CardDescription>
                      </div>
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="maskDetectionEnabled"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4 bg-muted/20">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Mask Support</FormLabel>
                        <CardDescription>Allow recognition of employees wearing face masks.</CardDescription>
                      </div>
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
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
