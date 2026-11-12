import { useState } from 'react';
import { ScanFace, CheckCircle2, AlertCircle, X, Sparkles } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import { FaceCapture } from './FaceCapture';
import { useFaceProfile, useRegisterFace } from '../hooks/useFaceProfile';
import { useTranslation } from '@/shared/i18n';

interface FaceRegistrationCardProps {
    employeeId: string;
}

export function FaceRegistrationCard({ employeeId }: FaceRegistrationCardProps) {
    const { isRtl } = useTranslation();
    const { data: profile, isLoading } = useFaceProfile(employeeId);
    const registerMutation = useRegisterFace(employeeId);
    const [showCapture, setShowCapture] = useState(false);

    const isRegistered = !isLoading && profile?.registrationStatus === 'Registered';

    return (
        <Card className="border-border bg-card text-card-foreground shadow-sm">
            <CardHeader className="pb-3 border-b border-border/50">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
                    <div>
                        <CardTitle className="text-base font-semibold text-foreground flex items-center gap-2">
                            <ScanFace className="h-5 w-5 text-primary" />
                            {isRtl ? 'بصمة الوجه والتعرف الذكي' : 'Face Biometric Profile'}
                        </CardTitle>
                        <CardDescription className="text-xs text-muted-foreground mt-1">
                            {isRtl
                                ? 'تسجيل وحفظ بصمة الوجه للاستخدام في تسجيل الحضور والانصراف الذكي'
                                : 'Enroll biometric face profile for automated kiosk and face check-in'}
                        </CardDescription>
                    </div>
                    <div>
                        {isLoading ? (
                            <div className="h-6 w-20 bg-muted animate-pulse rounded-full" />
                        ) : isRegistered ? (
                            <Badge className="bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30 flex items-center gap-1">
                                <CheckCircle2 className="h-3.5 w-3.5" />
                                {isRtl ? 'البصمة مسجلة' : 'Face Registered'}
                            </Badge>
                        ) : (
                            <Badge variant="outline" className="text-muted-foreground border-border flex items-center gap-1">
                                <AlertCircle className="h-3.5 w-3.5 text-amber-500" />
                                {isRtl ? 'غير مسجلة' : 'Not Registered'}
                            </Badge>
                        )}
                    </div>
                </div>
            </CardHeader>
            <CardContent className="pt-4 space-y-4">
                {profile?.registeredAt && isRegistered && (
                    <div className="text-xs text-muted-foreground">
                        <span>{isRtl ? 'تاريخ آخر تسجيل للبصمة:' : 'Last enrolled:'} </span>
                        <span className="font-medium font-mono text-foreground">
                            {new Date(profile.registeredAt).toLocaleDateString(isRtl ? 'ar-SA' : 'en-US')} - {new Date(profile.registeredAt).toLocaleTimeString(isRtl ? 'ar-SA' : 'en-US', { hour: '2-digit', minute: '2-digit' })}
                        </span>
                    </div>
                )}

                {!showCapture ? (
                    <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 rounded-lg bg-muted/30 border border-border/50">
                        <div className="space-y-1">
                            <p className="text-sm font-medium text-foreground">
                                {isRegistered
                                    ? (isRtl ? 'بصمة الوجه نشطة وجاهزة للاستخدام' : 'Biometric profile active')
                                    : (isRtl ? 'لم يتم تسجيل بصمة وجه لهذا الموظف بعد' : 'No biometric face enrolled for this employee')}
                            </p>
                            <p className="text-xs text-muted-foreground">
                                {isRtl
                                    ? 'يمكن استخدام كاميرا الحاسوب أو كاميرا الهاتف لالتقاط بصمة الوجه بدقة'
                                    : 'You can use web camera or phone camera to capture and enroll face'}
                            </p>
                        </div>
                        <Button
                            type="button"
                            variant={isRegistered ? 'outline' : 'default'}
                            onClick={() => setShowCapture(true)}
                            className="gap-2 shrink-0"
                        >
                            <ScanFace className="h-4 w-4" />
                            {isRegistered
                                ? (isRtl ? 'إعادة تسجيل البصمة' : 'Re-enroll Face')
                                : (isRtl ? 'تسجيل بصمة الوجه الآن' : 'Enroll Face Now')}
                        </Button>
                    </div>
                ) : (
                    <div className="space-y-4 pt-2">
                        <div className="flex items-center justify-between pb-2 border-b border-border/50">
                            <span className="text-sm font-medium text-foreground flex items-center gap-2">
                                <Sparkles className="h-4 w-4 text-primary" />
                                {isRtl ? 'التقاط بصمة الوجه' : 'Capture Face Biometrics'}
                            </span>
                            <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                onClick={() => setShowCapture(false)}
                                className="h-8 px-2 text-muted-foreground hover:text-foreground gap-1"
                            >
                                <X className="h-4 w-4" />
                                {isRtl ? 'إلغاء' : 'Cancel'}
                            </Button>
                        </div>
                        <FaceCapture
                            disabled={registerMutation.isPending}
                            onCapture={(descriptors) => {
                                registerMutation.mutate(descriptors, {
                                    onSuccess: () => setShowCapture(false),
                                });
                            }}
                        />
                    </div>
                )}
            </CardContent>
        </Card>
    );
}