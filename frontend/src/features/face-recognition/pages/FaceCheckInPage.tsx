import { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    CheckCircle2,
    XCircle,
    ScanFace,
    Loader2,
    MapPin,
    ArrowRight,
    ArrowLeft,
    UserCheck,
    Coffee,
    Briefcase,
    LogOut,
    Sparkles,
    ShieldAlert,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Label } from '@/shared/components/ui/label';
import { Badge } from '@/shared/components/ui/badge';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/components/ui/select';
import { toast } from 'sonner';
import { Input } from '@/shared/components/ui/input';
import { FaceCapture } from '../components/FaceCapture';
import { useVerifyFace } from '../hooks/useFaceProfile';
import { useShifts } from '@/features/shifts/hooks';
import { detectBestShift } from '@/features/shifts';
import { useEmployees } from '@/features/employees/hooks';
import { employeesApi } from '@/features/employees/api';
import { attendanceApi } from '@/features/attendance/api';
import { CheckInMethod } from '@/features/attendance/types';
import { useBranch } from '@/features/branches/hooks';
import { useTranslation } from '@/shared/i18n';
import { useGeolocation } from '@/shared/hooks';

function toLocalIsoString(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

const AUTO_DETECT_SHIFT = 'auto';

type ActionState =
    | 'idle'
    | 'detecting'
    | 'processing'
    | 'check-in-success'
    | 'break-start-success'
    | 'break-end-success'
    | 'check-out-success'
    | 'already-done'
    | 'error';

export function FaceCheckInPage() {
    const navigate = useNavigate();
    const { t, isRtl, language } = useTranslation();
    const { status: gpsStatus, getCurrentCoords, setManualCoords, retry: retryGps } = useGeolocation(true);
    const [selectedEmployeeId, setSelectedEmployeeId] = useState<string>('');
    const [employeeSearch, setEmployeeSearch] = useState<string>('');
    const [isEmployeeDropdownOpen, setIsEmployeeDropdownOpen] = useState(false);
    const employeeComboboxRef = useRef<HTMLDivElement>(null);
    const [shiftId, setShiftId] = useState<string>(AUTO_DETECT_SHIFT);
    const [actionState, setActionState] = useState<ActionState>('idle');
    const [actionMessage, setActionMessage] = useState<string>('');
    const [employeeCurrentState, setEmployeeCurrentState] = useState<string | null>(null);
    const [isCheckOutMode, setIsCheckOutMode] = useState<boolean>(false);
    const [currentTime, setCurrentTime] = useState<Date>(new Date());

    const verifyMutation = useVerifyFace();

    // Fetch active shifts
    const { data: shiftsData, isLoading: isLoadingShifts } = useShifts({
        pageNumber: 1,
        pageSize: 50,
        isActive: true,
    });

    // Fetch employees for instant selection & auto-resolution
    const { data: employeesData, isLoading: isLoadingEmployees } = useEmployees({
        pageNumber: 1,
        pageSize: 200,
    });

    const activeEmployees = useMemo(() => {
        return employeesData?.items ?? [];
    }, [employeesData]);

    const filteredEmployees = useMemo(() => {
        if (!employeeSearch.trim()) return activeEmployees;
        const q = employeeSearch.toLowerCase();
        return activeEmployees.filter(
            (e) =>
                e.firstName.toLowerCase().includes(q) ||
                e.lastName.toLowerCase().includes(q) ||
                `${e.firstName} ${e.lastName}`.toLowerCase().includes(q) ||
                e.employeeNumber.toLowerCase().includes(q) ||
                (e.jobTitle && e.jobTitle.toLowerCase().includes(q))
        );
    }, [activeEmployees, employeeSearch]);

    // Close dropdown on outside click
    useEffect(() => {
        const handler = (e: MouseEvent) => {
            if (employeeComboboxRef.current && !employeeComboboxRef.current.contains(e.target as Node)) {
                setIsEmployeeDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handler);
        return () => document.removeEventListener('mousedown', handler);
    }, []);

    const selectedEmployee = useMemo(() => {
        if (!selectedEmployeeId) return undefined;
        return activeEmployees.find(
            (e) => e.id.toLowerCase() === selectedEmployeeId.toLowerCase()
        );
    }, [activeEmployees, selectedEmployeeId]);

    // Fetch branch details for fallback coordinate resolution
    const { data: employeeBranch } = useBranch(selectedEmployee?.branchId ?? '');

    // Update real-time clock for auto shift detection
    useEffect(() => {
        const timer = setInterval(() => setCurrentTime(new Date()), 10000);
        return () => clearInterval(timer);
    }, []);

    // Automatically detect best matching shift based on current time
    const autoDetectedShift = useMemo(() => {
        if (!shiftsData?.items || shiftsData.items.length === 0) return undefined;
        return detectBestShift(shiftsData.items, currentTime);
    }, [shiftsData?.items, currentTime]);

    const effectiveShift = useMemo(() => {
        if (shiftId === AUTO_DETECT_SHIFT || !shiftId) {
            return autoDetectedShift;
        }
        return shiftsData?.items.find((s) => s.id.toLowerCase() === shiftId.toLowerCase());
    }, [shiftsData, shiftId, autoDetectedShift]);

    // جلب الحالة المباشرة للموظف المختار اليوم
    useEffect(() => {
        if (!selectedEmployeeId) {
            setEmployeeCurrentState(null);
            return;
        }
        attendanceApi.getCurrentState(selectedEmployeeId)
            .then((res) => setEmployeeCurrentState(res.state))
            .catch(() => setEmployeeCurrentState(null));
    }, [selectedEmployeeId]);

    const KIOSK_DEVICE_ID = '0607309e-ca4d-446b-83e5-05c7d36fd6e3';

    const performSmartAttendance = async (empId: string) => {
        setActionState('processing');
        setActionMessage('');
        try {
            const employee = await employeesApi.getEmployeeById(empId);
            const userCoords = await getCurrentCoords();
            const timeFormatted = new Date().toLocaleTimeString(language === 'ar' ? 'ar-IQ' : 'en-US');

            const resolvedShiftId = shiftId === AUTO_DETECT_SHIFT ? (effectiveShift?.id ?? undefined) : (shiftId || undefined);

            const scanResult = await attendanceApi.recordScan({
                employeeId: empId,
                shiftId: resolvedShiftId,
                attendancePolicyId: employee.attendancePolicyId || 'c4468cf3-8ecc-4a22-98fa-f623e35ec01f',
                attendanceDeviceId: KIOSK_DEVICE_ID,
                checkInMethod: CheckInMethod.FaceRecognition,
                latitude: userCoords.latitude,
                longitude: userCoords.longitude,
                timestamp: toLocalIsoString(new Date()),
                isExplicitCheckOut: isCheckOutMode,
            });

            setEmployeeCurrentState(scanResult.currentState);

            switch (scanResult.actionTaken) {
                case 'CheckIn':
                    setActionState('check-in-success');
                    setActionMessage(`${t('faceRecognition.checkInSuccess')} (${timeFormatted})`);
                    toast.success(t('faceRecognition.checkInSuccess'));
                    break;
                case 'BreakStart':
                    setActionState('break-start-success');
                    setActionMessage(`${t('faceRecognition.breakStartSuccess')} (${timeFormatted})`);
                    toast.warning(t('faceRecognition.breakStartSuccess'));
                    break;
                case 'BreakEnd':
                    setActionState('break-end-success');
                    setActionMessage(`${t('faceRecognition.breakEndSuccess')} (${timeFormatted})`);
                    toast.info(t('faceRecognition.breakEndSuccess'));
                    break;
                case 'CheckOut':
                    setActionState('check-out-success');
                    setActionMessage(`${t('faceRecognition.checkOutSuccess')} (${timeFormatted})`);
                    toast.success(t('faceRecognition.checkOutSuccess'));
                    break;
                default:
                    setActionState('check-in-success');
                    setActionMessage(`${t('faceRecognition.checkInSuccess')} (${timeFormatted})`);
                    break;
            }
        } catch (error: any) {
            const msg =
                error?.response?.data?.errors?.[0] ||
                error?.response?.data?.title ||
                error?.message ||
                t('common.error');

            const isAlreadyClosed = msg.includes('already closed') || msg.includes('already been completed');
            if (isAlreadyClosed) {
                setActionState('already-done');
                setActionMessage(t('faceRecognition.alreadyCompleted'));
                toast.info(t('faceRecognition.alreadyCompleted'));
                setEmployeeCurrentState('Finished');
            } else {
                setActionState('error');
                setActionMessage(msg);
                toast.error(msg);
            }
        }
    };

    const isProcessing = verifyMutation.isPending || actionState === 'processing';
    const BackIcon = isRtl ? ArrowRight : ArrowLeft;

    const renderCurrentStateBadge = () => {
        if (!employeeCurrentState) return null;
        switch (employeeCurrentState) {
            case 'NotCheckedIn':
                return (
                    <Badge variant="outline" className="text-muted-foreground border-muted-foreground/40 gap-1 text-[11px]">
                        <span className="h-1.5 w-1.5 rounded-full bg-slate-400" />
                        {language === 'ar' ? 'لم يسجل دخول اليوم' : 'Not Checked In'}
                    </Badge>
                );
            case 'Working':
                return (
                    <Badge className="bg-emerald-600 hover:bg-emerald-700 text-white gap-1 text-[11px]">
                        <span className="h-1.5 w-1.5 rounded-full bg-white animate-pulse" />
                        {language === 'ar' ? 'على رأس العمل (حاضر)' : 'Working (Active)'}
                    </Badge>
                );
            case 'OnBreak':
                return (
                    <Badge className="bg-amber-600 hover:bg-amber-700 text-white gap-1 text-[11px]">
                        <Coffee className="h-3 w-3" />
                        {language === 'ar' ? 'في استراحة مؤقتة' : 'On Break'}
                    </Badge>
                );
            case 'Finished':
                return (
                    <Badge className="bg-slate-700 hover:bg-slate-800 text-white gap-1 text-[11px]">
                        <LogOut className="h-3 w-3" />
                        {language === 'ar' ? 'انصراف مكتمل لليوم' : 'Finished For Today'}
                    </Badge>
                );
            default:
                return null;
        }
    };

    return (
        <div className="mx-auto max-w-lg space-y-6">
            {/* أزرار التنقل السريع */}
            <div className="flex items-center justify-between">
                <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="gap-2 text-muted-foreground hover:text-foreground"
                    onClick={() => navigate('/dashboard')}
                >
                    <BackIcon className="h-4 w-4" />
                    {t('common.backToDashboard')}
                </Button>
                <div className="flex items-center gap-2">
                    <Button
                        type="button"
                        variant={isCheckOutMode ? 'destructive' : 'outline'}
                        size="sm"
                        className="gap-1.5 text-xs font-semibold"
                        onClick={() => setIsCheckOutMode(!isCheckOutMode)}
                    >
                        {isCheckOutMode ? (
                            <>
                                <LogOut className="h-3.5 w-3.5" />
                                {t('faceRecognition.kioskModeCheckOut')}
                            </>
                        ) : (
                            <>
                                <Sparkles className="h-3.5 w-3.5 text-primary" />
                                {t('faceRecognition.kioskModeSmart')}
                            </>
                        )}
                    </Button>
                    <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        className="gap-1.5 text-xs"
                        onClick={() => navigate('/auto-kiosk')}
                    >
                        <ScanFace className="h-3.5 w-3.5" />
                        {t('dashboard.autoKioskAction')}
                    </Button>
                </div>
            </div>

            <div className="text-center">
                <h1 className="text-2xl font-bold tracking-tight">{t('faceRecognition.title')}</h1>
                <p className="text-muted-foreground text-sm">
                    {isCheckOutMode
                        ? (language === 'ar' ? 'وضع الانصراف النهائي: سيتم تسجيل انصراف الموظف وإغلاق دوام اليوم' : 'Final Check-Out Mode: Will record final check-out and close daily record')
                        : t('faceRecognition.subtitle')}
                </p>
            </div>

            <Card className="border-border/60 shadow-sm">
                <CardHeader>
                    <CardTitle className="flex items-center justify-between">
                        <span className="flex items-center gap-2">
                            <ScanFace className="h-4 w-4 text-primary" /> {t('faceRecognition.verifyEmployee')}
                        </span>
                        {renderCurrentStateBadge()}
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                    {/* اختيار الموظف — Combobox قابل للبحث */}
                    <div className="space-y-2">
                        <Label htmlFor="employeeSearch">{t('faceRecognition.employeeId')}</Label>
                        <div ref={employeeComboboxRef} className="relative">
                            <div className="relative">
                                <Input
                                    id="employeeSearch"
                                    autoComplete="off"
                                    disabled={isLoadingEmployees || isProcessing}
                                    placeholder={
                                        isLoadingEmployees
                                            ? t('common.loading')
                                            : language === 'ar'
                                            ? 'ابحث باسم الموظف أو رقمه...'
                                            : 'Search by name or employee number...'
                                    }
                                    value={
                                        isEmployeeDropdownOpen
                                            ? employeeSearch
                                            : selectedEmployee
                                            ? `${selectedEmployee.firstName} ${selectedEmployee.lastName} (${selectedEmployee.employeeNumber})`
                                            : employeeSearch
                                    }
                                    onFocus={() => {
                                        setIsEmployeeDropdownOpen(true);
                                        setEmployeeSearch('');
                                    }}
                                    onChange={(e) => {
                                        setEmployeeSearch(e.target.value);
                                        setIsEmployeeDropdownOpen(true);
                                        if (!e.target.value) {
                                            setSelectedEmployeeId('');
                                        }
                                    }}
                                    className="pr-8"
                                />
                                {selectedEmployee && !isEmployeeDropdownOpen && (
                                    <button
                                        type="button"
                                        className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                        onClick={() => {
                                            setSelectedEmployeeId('');
                                            setEmployeeSearch('');
                                            setActionState('idle');
                                            setActionMessage('');
                                        }}
                                    >
                                        <XCircle className="h-4 w-4" />
                                    </button>
                                )}
                            </div>

                            {/* قائمة النتائج */}
                            {isEmployeeDropdownOpen && (
                                <div className="absolute z-50 mt-1 w-full rounded-md border border-border bg-popover shadow-lg max-h-60 overflow-auto">
                                    {filteredEmployees.length === 0 ? (
                                        <div className="p-3 text-sm text-center text-muted-foreground">
                                            {language === 'ar' ? 'لا توجد نتائج' : 'No employees found'}
                                        </div>
                                    ) : (
                                        filteredEmployees.map((emp) => (
                                            <button
                                                key={emp.id}
                                                type="button"
                                                className={`w-full flex items-center gap-2 px-3 py-2.5 text-sm text-left hover:bg-accent hover:text-accent-foreground transition-colors ${
                                                    emp.id === selectedEmployeeId ? 'bg-primary/10 text-primary' : ''
                                                }`}
                                                onMouseDown={(e) => {
                                                    e.preventDefault();
                                                    setSelectedEmployeeId(emp.id);
                                                    setEmployeeSearch('');
                                                    setIsEmployeeDropdownOpen(false);
                                                    setActionState('idle');
                                                    setActionMessage('');
                                                }}
                                            >
                                                <span className="font-mono text-xs font-bold text-primary bg-primary/10 px-1.5 py-0.5 rounded shrink-0">
                                                    {emp.employeeNumber}
                                                </span>
                                                <span className="font-medium truncate">
                                                    {emp.firstName} {emp.lastName}
                                                </span>
                                                {emp.jobTitle && (
                                                    <span className="text-xs text-muted-foreground truncate">
                                                        ({emp.jobTitle})
                                                    </span>
                                                )}
                                                {emp.id === selectedEmployeeId && (
                                                    <CheckCircle2 className="h-3.5 w-3.5 text-primary ml-auto shrink-0" />
                                                )}
                                            </button>
                                        ))
                                    )}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* بطاقة الموظف المختار للتأكيد البصري */}
                    {selectedEmployee && (
                        <div className="flex items-center justify-between p-3 rounded-lg border bg-muted/40 text-xs">
                            <div className="flex items-center gap-2.5">
                                <div className="p-1.5 rounded-full bg-primary/10 text-primary">
                                    <UserCheck className="h-4 w-4" />
                                </div>
                                <div>
                                    <div className="font-semibold text-foreground">{selectedEmployee.firstName} {selectedEmployee.lastName}</div>
                                    <div className="text-muted-foreground">
                                        {selectedEmployee.jobTitle || selectedEmployee.employeeNumber}
                                    </div>
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                {renderCurrentStateBadge()}
                                <span className="font-mono bg-background border px-2 py-0.5 rounded text-[11px]">
                                    {selectedEmployee.employeeNumber}
                                </span>
                            </div>
                        </div>
                    )}

                    {/* اختيار الوردية */}
                    <div className="space-y-2">
                        <div className="flex items-center justify-between">
                            <Label>{t('faceRecognition.shift')}</Label>
                            {shiftId === AUTO_DETECT_SHIFT && effectiveShift && (
                                <span className="text-xs text-primary font-medium flex items-center gap-1">
                                    <Sparkles className="h-3 w-3" />
                                    {t('faceRecognition.autoDetectedShiftLabel')}: {effectiveShift.name}
                                </span>
                            )}
                        </div>
                        <Select
                            value={shiftId}
                            onValueChange={(v) => setShiftId(v ?? AUTO_DETECT_SHIFT)}
                            disabled={isLoadingShifts || isProcessing}
                        >
                            <SelectTrigger>
                                <SelectValue
                                    placeholder={isLoadingShifts ? t('common.loading') : t('faceRecognition.selectShift')}
                                >
                                    {shiftId === AUTO_DETECT_SHIFT ? (
                                        <div className="flex items-center gap-2">
                                            <span className="font-semibold text-primary">{t('faceRecognition.autoDetectShift')}</span>
                                            {effectiveShift && (
                                                <span className="text-xs text-muted-foreground">
                                                    ({effectiveShift.name} {effectiveShift.startTime?.slice(0, 5)} - {effectiveShift.endTime?.slice(0, 5)})
                                                </span>
                                            )}
                                        </div>
                                    ) : effectiveShift ? (
                                        `${effectiveShift.name} (${effectiveShift.startTime?.slice(0, 5)} - ${effectiveShift.endTime?.slice(0, 5)})`
                                    ) : undefined}
                                </SelectValue>
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value={AUTO_DETECT_SHIFT}>
                                    <div className="flex items-center gap-2 py-0.5">
                                        <span className="font-semibold text-primary">{t('faceRecognition.autoDetectShift')}</span>
                                        {autoDetectedShift && (
                                            <span className="text-xs text-muted-foreground">
                                                ({autoDetectedShift.name} {autoDetectedShift.startTime.slice(0, 5)} – {autoDetectedShift.endTime.slice(0, 5)})
                                            </span>
                                        )}
                                    </div>
                                </SelectItem>
                                {shiftsData?.items.map((s) => (
                                    <SelectItem key={s.id} value={s.id}>
                                        {s.name} ({s.startTime?.slice(0, 5)} - {s.endTime?.slice(0, 5)})
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {/* تبديل وضع العملية وتوضيح الإجراء القادم */}
                    <div className="rounded-xl border bg-muted/20 p-3 space-y-2.5 shadow-sm">
                        <div className="flex items-center justify-between text-xs font-semibold text-muted-foreground">
                            <span>{language === 'ar' ? 'حدد نوع الحركة المراد تسجيلها:' : 'Select Scan Operation:'}</span>
                            {renderCurrentStateBadge()}
                        </div>

                        {/* زران لاختيار نوع العملية */}
                        <div className="grid grid-cols-2 gap-2">
                            <button
                                type="button"
                                onClick={() => setIsCheckOutMode(false)}
                                className={`flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg text-xs font-semibold border transition-all ${
                                    !isCheckOutMode
                                        ? 'bg-primary text-primary-foreground border-primary shadow-sm'
                                        : 'bg-background hover:bg-muted text-muted-foreground border-border'
                                }`}
                            >
                                <Sparkles className="h-3.5 w-3.5" />
                                <span>{language === 'ar' ? 'الوضع الذكي (حضور / استراحة / عودة)' : 'Smart Mode (In / Break / Back)'}</span>
                            </button>

                            <button
                                type="button"
                                onClick={() => setIsCheckOutMode(true)}
                                className={`flex items-center justify-center gap-1.5 py-2 px-3 rounded-lg text-xs font-semibold border transition-all ${
                                    isCheckOutMode
                                        ? 'bg-orange-600 text-white border-orange-600 shadow-sm'
                                        : 'bg-background hover:bg-muted text-muted-foreground border-border'
                                }`}
                            >
                                <LogOut className="h-3.5 w-3.5" />
                                <span>{language === 'ar' ? 'انصراف نهائي (Check-Out)' : 'Final Check-Out'}</span>
                            </button>
                        </div>

                        {/* صندوق التوضيح الفوري للإجراء التالي */}
                        {selectedEmployee && (
                            <div className={`p-2.5 rounded-lg text-xs flex items-center gap-2 border ${
                                employeeCurrentState === 'Finished'
                                    ? 'bg-slate-500/10 border-slate-500/30 text-slate-700 dark:text-slate-300'
                                    : isCheckOutMode
                                    ? 'bg-orange-500/10 border-orange-500/30 text-orange-700 dark:text-orange-300'
                                    : employeeCurrentState === 'Working'
                                    ? 'bg-amber-500/10 border-amber-500/30 text-amber-700 dark:text-amber-300'
                                    : employeeCurrentState === 'OnBreak'
                                    ? 'bg-blue-500/10 border-blue-500/30 text-blue-700 dark:text-blue-300'
                                    : 'bg-emerald-500/10 border-emerald-500/30 text-emerald-700 dark:text-emerald-300'
                            }`}>
                                {employeeCurrentState === 'Finished' ? (
                                    <>
                                        <ShieldAlert className="h-4 w-4 shrink-0 text-slate-600" />
                                        <span className="font-medium">
                                            {language === 'ar'
                                                ? 'تم إغلاق دوام اليوم لهذا الموظف بالفعل (سجل انصراف نهائي).'
                                                : 'This employee has already finished their shift for today.'}
                                        </span>
                                    </>
                                ) : isCheckOutMode ? (
                                    <>
                                        <LogOut className="h-4 w-4 shrink-0 text-orange-600" />
                                        <span className="font-medium">
                                            {language === 'ar'
                                                ? 'عند التقاط الوجه الآن ⬅ سيتم تسجيل: [انصراف نهائي لليوم] وإغلاق الدوام.'
                                                : 'Next face scan ⬅ will record: [Final Check-Out] and close shift.'}
                                        </span>
                                    </>
                                ) : employeeCurrentState === 'Working' ? (
                                    <>
                                        <Coffee className="h-4 w-4 shrink-0 text-amber-600" />
                                        <span className="font-medium">
                                            {language === 'ar'
                                                ? 'الموظف على رأس العمل ⬅ المسح الآن سيسجل: [بدء استراحة / خروج مؤقت].'
                                                : 'Employee is working ⬅ Next scan will record: [Break Start].'}
                                        </span>
                                    </>
                                ) : employeeCurrentState === 'OnBreak' ? (
                                    <>
                                        <Briefcase className="h-4 w-4 shrink-0 text-blue-600" />
                                        <span className="font-medium">
                                            {language === 'ar'
                                                ? 'الموظف في استراحة ⬅ المسح الآن سيسجل: [انتهاء الاستراحة والعودة للعمل].'
                                                : 'Employee on break ⬅ Next scan will record: [Break End / Resume Work].'}
                                        </span>
                                    </>
                                ) : (
                                    <>
                                        <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-600" />
                                        <span className="font-medium">
                                            {language === 'ar'
                                                ? 'الموظف لم يسجل بعد ⬅ المسح الآن سيسجل: [حضور بداية الدوام (Check-In)].'
                                                : 'Employee not checked in ⬅ Next scan will record: [Initial Check-In].'}
                                        </span>
                                    </>
                                )}
                            </div>
                        )}
                    </div>

                    {/* كاميرا التعرف على الوجه */}
                    <FaceCapture
                        disabled={!selectedEmployeeId || !shiftId || isProcessing}
                        totalSteps={1}
                        onCapture={async (descriptors) => {
                            const descriptor = descriptors[0];
                            if (!selectedEmployeeId) {
                                toast.error(language === 'ar' ? 'يرجى اختيار الموظف أولاً' : 'Please select an employee first');
                                return;
                            }
                            try {
                                const response = await verifyMutation.mutateAsync({
                                    employeeId: selectedEmployeeId,
                                    descriptor,
                                });

                                if (response.isMatch) {
                                    await performSmartAttendance(selectedEmployeeId);
                                } else {
                                    setActionState('error');
                                    const msg = `${t('faceRecognition.verifyFailed')} (${(response.confidence * 100).toFixed(1)}%)`;
                                    setActionMessage(msg);
                                    toast.error(msg);
                                }
                            } catch (err: any) {
                                setActionState('error');
                                const msg =
                                    err?.response?.data?.title ||
                                    err?.response?.data?.errors?.[0] ||
                                    err?.message ||
                                    t('faceRecognition.verifyFailed');
                                setActionMessage(msg);
                                toast.error(msg);
                            }
                        }}
                    />

                    {/* مؤشر الموقع الجغرافي */}
                    <div className="flex items-center justify-center gap-2 text-xs text-muted-foreground pt-1 flex-wrap">
                        <MapPin className={`h-3.5 w-3.5 shrink-0 ${gpsStatus === 'granted' ? 'text-green-600' : 'text-amber-500'}`} />
                        <span>
                            {gpsStatus === 'granted'
                                ? (language === 'ar' ? 'تم تحديد موقع الجهاز بنجاح' : 'Device location acquired')
                                : gpsStatus === 'loading'
                                ? (language === 'ar' ? 'جاري تحديد موقع GPS...' : 'Acquiring GPS location...')
                                : gpsStatus === 'denied'
                                ? (language === 'ar' ? 'تم رفض إذن الوصول للموقع (GPS)' : 'Location permission denied')
                                : gpsStatus === 'insecure'
                                ? (language === 'ar' ? 'يتطلب تشغيل الـ GPS اتصال HTTPS أو localhost' : 'GPS requires HTTPS or localhost')
                                : (language === 'ar' ? 'الموقع الجغرافي غير مفعل' : 'GPS location unavailable')}
                        </span>
                        {gpsStatus !== 'granted' && (
                            <button
                                type="button"
                                onClick={() => retryGps()}
                                className="text-primary underline ml-1 font-medium hover:text-primary/80 cursor-pointer"
                            >
                                {language === 'ar' ? 'إعادة المحاولة' : 'Retry'}
                            </button>
                        )}
                        {employeeBranch && (employeeBranch.latitude !== 0 || employeeBranch.longitude !== 0) && (
                            <button
                                type="button"
                                onClick={() => {
                                    setManualCoords(employeeBranch.latitude, employeeBranch.longitude);
                                    toast.success(language === 'ar' ? `تم تفعيل إحداثيات فرع (${employeeBranch.name})` : `Set coordinates to branch (${employeeBranch.name})`);
                                }}
                                className="text-emerald-600 dark:text-emerald-400 underline font-medium hover:text-emerald-700 cursor-pointer ml-1"
                            >
                                {language === 'ar' ? `📍 استخدام إحداثيات فرع (${employeeBranch.name})` : `📍 Use (${employeeBranch.name}) Coordinates`}
                            </button>
                        )}
                    </div>

                    {/* حالة المعالجة */}
                    {isProcessing && (
                        <div className="flex items-center justify-center gap-2 text-sm text-muted-foreground py-2">
                            <Loader2 className="h-4 w-4 animate-spin text-primary" />
                            <span>
                                {verifyMutation.isPending
                                    ? t('faceRecognition.verifyingFace')
                                    : (language === 'ar' ? 'جاري فحص وتسجيل الحدث...' : 'Processing attendance action...')}
                            </span>
                        </div>
                    )}

                    {/* رسالة النتيجة */}
                    {actionState !== 'idle' && actionState !== 'processing' && actionMessage && (
                        <div
                            className={`flex flex-col gap-1.5 rounded-xl border p-4 text-sm transition-all duration-300 ${
                                actionState === 'check-in-success'
                                    ? 'border-green-500/40 bg-green-500/10 text-green-700 dark:text-green-300'
                                    : actionState === 'break-start-success'
                                    ? 'border-amber-500/40 bg-amber-500/10 text-amber-700 dark:text-amber-300'
                                    : actionState === 'break-end-success'
                                    ? 'border-blue-500/40 bg-blue-500/10 text-blue-700 dark:text-blue-300'
                                    : actionState === 'check-out-success'
                                    ? 'border-orange-500/40 bg-orange-500/10 text-orange-700 dark:text-orange-300'
                                    : actionState === 'already-done'
                                    ? 'border-slate-500/40 bg-slate-500/10 text-slate-700 dark:text-slate-300'
                                    : 'border-destructive/40 bg-destructive/10 text-destructive'
                            }`}
                        >
                            <div className="flex items-center gap-2 font-semibold">
                                {actionState === 'check-in-success' && <CheckCircle2 className="h-5 w-5 shrink-0 text-green-600" />}
                                {actionState === 'break-start-success' && <Coffee className="h-5 w-5 shrink-0 text-amber-600" />}
                                {actionState === 'break-end-success' && <Briefcase className="h-5 w-5 shrink-0 text-blue-600" />}
                                {actionState === 'check-out-success' && <LogOut className="h-5 w-5 shrink-0 text-orange-600" />}
                                {actionState === 'already-done' && <ShieldAlert className="h-5 w-5 shrink-0 text-slate-600" />}
                                {actionState === 'error' && <XCircle className="h-5 w-5 shrink-0 text-destructive" />}

                                <span>
                                    {actionState === 'check-in-success'
                                        ? t('faceRecognition.checkInSuccess')
                                        : actionState === 'break-start-success'
                                        ? t('faceRecognition.breakStartSuccess')
                                        : actionState === 'break-end-success'
                                        ? t('faceRecognition.breakEndSuccess')
                                        : actionState === 'check-out-success'
                                        ? t('faceRecognition.checkOutSuccess')
                                        : actionState === 'already-done'
                                        ? t('faceRecognition.alreadyCompleted')
                                        : t('faceRecognition.verifyFailed')}
                                </span>
                            </div>
                            <p className="text-xs opacity-90 mt-0.5">{actionMessage}</p>
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                className="mt-2 self-end text-xs"
                                onClick={() => {
                                    setActionState('idle');
                                    setActionMessage('');
                                }}
                            >
                                {t('common.refresh')}
                            </Button>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}

export default FaceCheckInPage;
