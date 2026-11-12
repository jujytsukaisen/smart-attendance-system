import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { LoginSchemaType } from '../schemas';
import { loginSchema } from '../schemas';
import { useLogin } from '../hooks';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/components/ui/form';
import { useTranslation } from '@/shared/i18n';

export const LoginForm = () => {
  const { mutate: login, isPending, isError, error } = useLogin();
  const { t } = useTranslation();

  const form = useForm<LoginSchemaType>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = (data: LoginSchemaType) => {
    login(data);
  };

  return (
    <Card className="w-full shadow-lg border">
      <CardHeader className="space-y-1">
        <CardTitle className="text-2xl text-center font-bold">
          {t('common.appName')}
        </CardTitle>
        <CardDescription className="text-center">
          {t('auth.loginSubtitle')}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t('auth.email')}</FormLabel>
                  <FormControl>
                    <Input placeholder="admin@smartattendance.com" type="email" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t('auth.password')}</FormLabel>
                  <FormControl>
                    <Input type="password" placeholder="••••••••" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            
            {isError && (
              <div className="text-sm font-medium text-destructive text-center p-2 rounded-md bg-destructive/10">
                {t('common.error')}: {(error as any)?.response?.data?.detail || (error as any)?.response?.data?.title || t('auth.invalidCredentials', 'Invalid credentials')}
              </div>
            )}

            <Button className="w-full font-semibold shadow-xs" type="submit" disabled={isPending}>
              {isPending ? t('auth.loggingIn') : t('auth.submitLogin')}
            </Button>
          </form>
        </Form>
      </CardContent>
      <CardFooter className="flex justify-center border-t p-4 text-center">
        <p className="text-xs text-muted-foreground">
          {t('common.appSubtitle')}
        </p>
      </CardFooter>
    </Card>
  );
};
