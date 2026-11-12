import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CheckCircle2, XCircle, Loader2, LogOut, ArrowRight } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { toast } from 'sonner';
import { FaceCapture } from '../components/FaceCapture';
import { useVerifyFace } from '../hooks/useFaceProfile';
import { employeesApi } from '@/features/employees/api';
import { attendanceApi } from '@/features/attendance/api';
import { CheckInMethod } from '@/features/attendance/types';
import { useGeolocation } from '@/shared/hooks';

// ⚠️ يرسل الوقت المحلي كما هو (بدون تحويل UTC عبر toISOString)
// لأن الباك إند يفقد علامة الـ 'Z' عند الإرجاع، فيعرض المتصفح الوقت الخام كأنه محلي
// هذا يسبب فرق ساعات كامل (مثلاً فرق التوقيت بين بغداد و UTC)
function toLocalIsoString(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

type CheckOutState = 'idle' | 'checking-out' | 'success' | 'error';

// صفحة مستقلة (Kiosk) توضع عند مدخل الشركة لتسجيل الانصراف بالوجه
// التدفق: إدخال رقم الموظف -> التحقق من الوجه -> إيجاد سجل الحضور المفتوح اليوم -> تسجيل الانصراف
export function FaceCheckOutPage() {
    const navigate = useNavigate();
    const [employeeId, setEmployeeId] = useState('');
    const [checkOutState, setCheckOutState] = useState<CheckOutState>('idle');
    const [checkOutMessage, setCheckOutMessage] = useState<string | null>(null);
    const { getCurrentCoords } = useGeolocation(true);

    const verifyMutation = useVerifyFace();

    const performCheckOut = async (resolvedEmployeeId: string) => {
        setCheckOutState('checking-out');
        setCheckOutMessage(null);
        try {
            const employee = await employeesApi.getEmployeeById(resolvedEmployeeId);
            const userCoords = await getCurrentCoords();

            const result = await attendanceApi.recordScan({
                employeeId: resolvedEmployeeId,
                attendancePolicyId: employee.attendancePolicyId || 'c4468cf3-8ecc-4a22-98fa-f623e35ec01f',
                checkInMethod: CheckInMethod.FaceRecognition,
                latitude: userCoords.latitude,
                longitude: userCoords.longitude,
                timestamp: toLocalIsoString(new Date()),
                isExplicitCheckOut: true,
            });

            setCheckOutState('success');
            const timeStr = new Date(result.timestamp).toLocaleTimeString('ar-IQ');
            setCheckOutMessage(
                `تم تسجيل الانصراف النهائي بنجاح الساعة ${timeStr}`
            );
            toast.success('تم تسجيل الانصراف بنجاح');
        } catch (error: any) {
            setCheckOutState('error');
            const msg = error?.response?.data?.errors?.[0] || error?.response?.data?.title || error?.message || 'فشل تسجيل الانصراف';
            setCheckOutMessage(msg);
            toast.error(msg);
        }
    };

    // ⚠️ حقل "Employee ID" يُدخله المستخدم كرقم موظف مقروء (مثل EMP-001)
    // بينما الباك إند يتطلب الـ GUID الحقيقي، لذا نبحث عنه أولًا
    const resolveEmployeeGuid = async (): Promise<string | null> => {
        try {
            const result = await employeesApi.getEmployees({
                searchTerm: employeeId,
                pageNumber: 1,
                pageSize: 1,
            });
            const match = result.items[0];
            if (!match) {
                toast.error('لم يتم العثور على موظف بهذا الرقم');
                return null;
            }
            return match.id;
        } catch {
            toast.error('حدث خطأ أثناء البحث عن الموظف');
            return null;
        }
    };

    return (
        <div className="mx-auto flex max-w-lg flex-col gap-6 p-6">
            <div className="flex items-center justify-between">
                <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="gap-2 text-muted-foreground hover:text-foreground"
                    onClick={() => navigate('/dashboard')}
                >
                    <ArrowRight className="h-4 w-4" />
                    العودة للوحة التحكم
                </Button>
            </div>

            <div className="text-center">
                <h1 className="text-2xl font-bold tracking-tight">Face Check-Out</h1>
                <p className="text-muted-foreground text-sm">تسجيل انصراف الموظف بالوجه</p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                        <LogOut className="h-4 w-4" /> Employee Verification
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="employeeId">Employee ID</Label>
                        <Input
                            id="employeeId"
                            placeholder="Enter your employee ID (e.g. EMP-001)"
                            value={employeeId}
                            onChange={(e) => setEmployeeId(e.target.value)}
                        />
                    </div>

                    <FaceCapture
                        disabled={!employeeId || verifyMutation.isPending || checkOutState === 'checking-out'}
                        totalSteps={1}
                        onCapture={async (descriptors) => {
                            const descriptor = descriptors[0];
                            if (!employeeId) return;
                            setCheckOutState('idle');
                            setCheckOutMessage(null);

                            const resolvedId = await resolveEmployeeGuid();
                            if (!resolvedId) return;

                            verifyMutation.mutate(
                                { employeeId: resolvedId, descriptor },
                                {
                                    onSuccess: (result) => {
                                        if (result.isMatch) {
                                            performCheckOut(resolvedId);
                                        }
                                    },
                                }
                            );
                        }}
                    />

                    {verifyMutation.data && !verifyMutation.data.isMatch && (
                        <div className="flex items-center gap-2 rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
                            <XCircle className="h-5 w-5" />
                            Face does not match ({verifyMutation.data.confidence}% confidence)
                        </div>
                    )}

                    {verifyMutation.data?.isMatch && checkOutState === 'checking-out' && (
                        <div className="flex items-center gap-2 rounded-lg bg-blue-50 p-4 text-sm text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">
                            <Loader2 className="h-5 w-5 animate-spin" />
                            تم التحقق من الوجه ({verifyMutation.data.confidence}%)، جاري تسجيل الانصراف...
                        </div>
                    )}

                    {checkOutState === 'success' && (
                        <div className="flex items-center gap-2 rounded-lg bg-green-50 p-4 text-sm text-green-700 dark:bg-green-900/30 dark:text-green-400">
                            <CheckCircle2 className="h-5 w-5" />
                            {checkOutMessage}
                        </div>
                    )}

                    {checkOutState === 'error' && (
                        <div className="flex items-center gap-2 rounded-lg bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/30 dark:text-red-400">
                            <XCircle className="h-5 w-5" />
                            {checkOutMessage}
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
