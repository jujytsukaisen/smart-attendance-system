import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTheme } from 'next-themes';
import { toast } from 'sonner';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { Input } from '@/shared/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { SettingsSaveBar } from '../components';
import { useGeneralSettings, useUpdateGeneralSettings } from '../hooks';
import { generalSettingsSchema } from '../schemas';
import type { GeneralSettingsDto } from '../types';
import { useTranslation, useLanguageStore } from '@/shared/i18n';

export function GeneralSettingsPage() {
  const { t, language } = useTranslation();
  const { data, isLoading, isError, error } = useGeneralSettings();
  const updateMutation = useUpdateGeneralSettings();
  const { theme, setTheme } = useTheme();

  const form = useForm<GeneralSettingsDto>({
    resolver: zodResolver(generalSettingsSchema),
    values: data || {
      companyName: '',
      theme: 'System',
      dateFormat: 'YYYY-MM-DD',
      timeFormat: '24h',
      timeZone: '',
      language: 'en'
    },
  });

  useEffect(() => {
    if (theme) {
      const formattedTheme = (theme.charAt(0).toUpperCase() + theme.slice(1)) as 'Light' | 'Dark' | 'System';
      if (['Light', 'Dark', 'System'].includes(formattedTheme)) {
        form.setValue('theme', formattedTheme, { shouldDirty: false });
      }
    }
  }, [theme, form]);

  useEffect(() => {
    if (language && (language === 'ar' || language === 'en')) {
      form.setValue('language', language, { shouldDirty: false });
    }
  }, [language, form]);

  const onSubmit = async (values: GeneralSettingsDto) => {
    await updateMutation.mutateAsync(values);
    form.reset(values);
    toast.success(t('settings.savedSuccess'));
  };

  if (isLoading) return <Skeleton className="h-[400px] w-full" />;
  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : t('common.error')}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{t('settings.generalTitle')}</CardTitle>
          <CardDescription>{t('settings.generalSubtitle')}</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form id="general-settings-form" onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <FormField
                  control={form.control}
                  name="companyName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.companyName')}</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="theme"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.theme')}</FormLabel>
                      <Select
                        onValueChange={(val) => {
                          field.onChange(val);
                          setTheme(val.toLowerCase());
                        }}
                        value={field.value}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={t('settings.selectTheme')} />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="Light">{t('settings.light')}</SelectItem>
                          <SelectItem value="Dark">{t('settings.dark')}</SelectItem>
                          <SelectItem value="System">{t('settings.system')}</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="dateFormat"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.dateFormat')}</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value}>
                        <FormControl>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="YYYY-MM-DD">YYYY-MM-DD</SelectItem>
                          <SelectItem value="DD/MM/YYYY">DD/MM/YYYY</SelectItem>
                          <SelectItem value="MM/DD/YYYY">MM/DD/YYYY</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="timeFormat"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.timeFormat')}</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value}>
                        <FormControl>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="12h">{language === 'ar' ? '12 ساعة (ص/م)' : '12-hour (AM/PM)'}</SelectItem>
                          <SelectItem value="24h">{language === 'ar' ? '24 ساعة' : '24-hour'}</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="timeZone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.timeZone')}</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="language"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{t('settings.language')}</FormLabel>
                      <Select
                        onValueChange={(val) => {
                          field.onChange(val);
                          if (val === 'ar' || val === 'en') {
                            useLanguageStore.getState().setLanguage(val);
                          }
                        }}
                        value={field.value}
                      >
                        <FormControl>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="ar">🇸🇦 العربية</SelectItem>
                          <SelectItem value="en">🇺🇸 English</SelectItem>
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
