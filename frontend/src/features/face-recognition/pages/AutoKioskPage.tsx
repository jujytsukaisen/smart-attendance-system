import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import * as faceapi from 'face-api.js';
import {
    CheckCircle2,
    XCircle,
    Loader2,
    ScanFace,
    Users,
    Wifi,
    WifiOff,
    ArrowRight,
    ArrowLeft,
    Coffee,
    Briefcase,
    LogOut,
    Sparkles,
    ShieldAlert,
    MapPin,
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/components/ui/select';
import { Label } from '@/shared/components/ui/label';
import { toast } from 'sonner';
import { employeesApi } from '@/features/employees/api';
import { attendanceApi } from '@/features/attendance/api';
import { getAllFaceEncodings } from '../api';
import { useShifts } from '@/features/shifts/hooks';
import { detectBestShift } from '@/features/shifts';
import { CheckInMethod } from '@/features/attendance/types';
import { useFaceApiModels } from '../hooks/useFaceApiModels';
import { useGeolocation } from '@/shared/hooks';
import { useTranslation } from '@/shared/i18n';
import type { FaceEncodingDto } from '../types';

const AUTO_DETECT_SHIFT = 'auto';

const KIOSK_DEVICE_ID = '0607309e-ca4d-446b-83e5-05c7d36fd6e3';
const POLICY_ID = 'c4468cf3-8ecc-4a22-98fa-f623e35ec01f';

// مدة الانتظار بين كل فحص (ملليثانية)
const DETECTION_INTERVAL_MS = 2000;
// حد الثقة
const MATCH_THRESHOLD = 0.5;
// مدة حظر نفس الموظف بعد تسجيل الحدث (30 ثانية)
const COOLDOWN_MS = 30_000;

type KioskStatus = 'loading-models' | 'loading-profiles' | 'ready' | 'detecting' | 'processing' | 'success' | 'error' | 'no-shift';

function toLocalIsoString(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

interface LastResult {
    name: string;
    action: 'check-in' | 'break-start' | 'break-end' | 'check-out' | 'already-done';
    currentState?: string;
    time: string;
    success: boolean;
    message?: string;
}

export function AutoKioskPage() {
    const navigate = useNavigate();
    const { t, isRtl, language } = useTranslation();
    const { isReady: modelsReady } = useFaceApiModels();
    const { data: shiftsData } = useShifts({ pageNumber: 1, pageSize: 50, isActive: true });
    const { status: gpsStatus, getCurrentCoords } = useGeolocation(true);

    const [shiftId, setShiftId] = useState<string>(AUTO_DETECT_SHIFT);
    const [status, setStatus] = useState<KioskStatus>('loading-models');
    const [profilesCount, setProfilesCount] = useState<number>(0);
    const [lastResult, setLastResult] = useState<LastResult | null>(null);
    const [isCheckOutMode, setIsCheckOutMode] = useState<boolean>(false);
    const [currentTime, setCurrentTime] = useState<Date>(new Date());

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

    const videoRef = useRef<HTMLVideoElement>(null);
    const streamRef = useRef<MediaStream | null>(null);
    const faceMatcherRef = useRef<faceapi.FaceMatcher | null>(null);
    const profilesRef = useRef<FaceEncodingDto[]>([]);
    const lastSeenRef = useRef<Map<string, number>>(new Map());
    const isProcessingRef = useRef<boolean>(false);
    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

    const BackIcon = isRtl ? ArrowRight : ArrowLeft;

    // 1. تشغيل الكاميرا
    const startCamera = useCallback(async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'user', width: { ideal: 640 }, height: { ideal: 480 } },
            });
            streamRef.current = stream;
            if (videoRef.current) {
                videoRef.current.srcObject = stream;
            }
        } catch (err) {
            console.error('Camera error:', err);
            toast.error(t('faceRecognition.cameraBlocked'));
        }
    }, [t]);

    // 2. تحميل بصمات وجوه الموظفين وبناء FaceMatcher
    const loadFaceProfiles = useCallback(async () => {
        setStatus('loading-profiles');
        try {
            const data = await getAllFaceEncodings();
            profilesRef.current = data;
            setProfilesCount(data.length);

            if (!data || data.length === 0) {
                toast.warning(isRtl ? 'لا توجد بصمات وجوه مسجلة في النظام بعد' : 'No registered face profiles found');
                setStatus('ready');
                return;
            }

            const labeledDescriptors: faceapi.LabeledFaceDescriptors[] = [];
            for (const item of data) {
                try {
                    const parsed: number[] = typeof item.faceEncoding === 'string' ? JSON.parse(item.faceEncoding) : item.faceEncoding;
                    if (Array.isArray(parsed) && parsed.length > 0) {
                        const descriptor = new Float32Array(parsed);
                        labeledDescriptors.push(
                            new faceapi.LabeledFaceDescriptors(item.employeeId, [descriptor])
                        );
                    }
                } catch {
                    // skip invalid encoding
                }
            }

            if (labeledDescriptors.length > 0) {
                faceMatcherRef.current = new faceapi.FaceMatcher(labeledDescriptors, MATCH_THRESHOLD);
            }
            setStatus('ready');
        } catch (err: any) {
            console.error('Failed to load profiles:', err);
            setStatus('error');
            toast.error(isRtl ? 'فشل تحميل بصمات الوجوه' : 'Failed to load face encodings');
        }
    }, [isRtl]);

    // 3. فحص الحضور/الاستراحة/الانصراف التلقائي الذكي
    const handleEmployeeDetected = useCallback(async (empId: string) => {
        const now = Date.now();
        const lastSeen = lastSeenRef.current.get(empId) ?? 0;
        if (now - lastSeen < COOLDOWN_MS) {
            return;
        }

        const profile = profilesRef.current.find((p) => p.employeeId === empId);
        const name = profile?.employeeName ?? empId;
        let policyId = profile?.attendancePolicyId;
        if (!policyId || policyId === '00000000-0000-0000-0000-000000000000') {
            try {
                const emp = await employeesApi.getEmployeeById(empId);
                policyId = emp.attendancePolicyId;
            } catch {
                policyId = POLICY_ID;
            }
        }

        isProcessingRef.current = true;
        setStatus('processing');

        try {
            const timeFormatted = new Date().toLocaleTimeString(language === 'ar' ? 'ar-IQ' : 'en-US');

            const resolvedShiftId = shiftId === AUTO_DETECT_SHIFT ? (effectiveShift?.id ?? undefined) : (shiftId || undefined);

            const userCoords = await getCurrentCoords();

            const scanResult = await attendanceApi.recordScan({
                employeeId: empId,
                shiftId: resolvedShiftId,
                attendancePolicyId: policyId,
                attendanceDeviceId: KIOSK_DEVICE_ID,
                checkInMethod: CheckInMethod.FaceRecognition,
                latitude: userCoords.latitude,
                longitude: userCoords.longitude,
                timestamp: toLocalIsoString(new Date()),
                isExplicitCheckOut: isCheckOutMode,
            });

            lastSeenRef.current.set(empId, now);

            let actionType: LastResult['action'] = 'check-in';
            let messageText = '';

            switch (scanResult.actionTaken) {
                case 'CheckIn':
                    actionType = 'check-in';
                    messageText = t('faceRecognition.checkInSuccess');
                    toast.success(`${name}: ${t('faceRecognition.checkInSuccess')}`);
                    break;
                case 'BreakStart':
                    actionType = 'break-start';
                    messageText = t('faceRecognition.breakStartSuccess');
                    toast.warning(`${name}: ${t('faceRecognition.breakStartSuccess')}`);
                    break;
                case 'BreakEnd':
                    actionType = 'break-end';
                    messageText = t('faceRecognition.breakEndSuccess');
                    toast.info(`${name}: ${t('faceRecognition.breakEndSuccess')}`);
                    break;
                case 'CheckOut':
                    actionType = 'check-out';
                    messageText = t('faceRecognition.checkOutSuccess');
                    toast.success(`${name}: ${t('faceRecognition.checkOutSuccess')}`);
                    break;
                default:
                    actionType = 'check-in';
                    messageText = t('faceRecognition.checkInSuccess');
                    break;
            }

            setLastResult({
                name,
                action: actionType,
                currentState: scanResult.currentState,
                time: timeFormatted,
                success: true,
                message: messageText,
            });

            setStatus('success');
            setTimeout(() => {
                setStatus('detecting');
            }, 3000);
        } catch (err: any) {
            console.error('Kiosk attendance error:', err);
            const msg = err?.response?.data?.errors?.[0] || err?.response?.data?.title || err?.message || t('common.error');
            const isAlreadyClosed = msg.includes('already closed') || msg.includes('already been completed');
            const timeFormatted = new Date().toLocaleTimeString(language === 'ar' ? 'ar-IQ' : 'en-US');

            lastSeenRef.current.set(empId, now);

            if (isAlreadyClosed) {
                setLastResult({
                    name,
                    action: 'already-done',
                    currentState: 'Finished',
                    time: timeFormatted,
                    success: true,
                    message: t('faceRecognition.alreadyCompleted'),
                });
                toast.info(`${name}: ${t('faceRecognition.alreadyCompleted')}`);
            } else {
                setLastResult({
                    name,
                    action: 'check-in',
                    time: timeFormatted,
                    success: false,
                    message: msg,
                });
                toast.error(msg);
            }

            setStatus('detecting');
        } finally {
            isProcessingRef.current = false;
        }
    }, [shiftId, isCheckOutMode, language, t]);

    // 4. حلقة الكشف المستمرة عن الوجوه
    const detectLoop = useCallback(async () => {
        if (!videoRef.current || !faceMatcherRef.current || isProcessingRef.current) {
            return;
        }
        if (videoRef.current.readyState < 2) {
            return;
        }

        try {
            const detection = await faceapi
                .detectSingleFace(videoRef.current, new faceapi.TinyFaceDetectorOptions({ inputSize: 224, scoreThreshold: 0.5 }))
                .withFaceLandmarks()
                .withFaceDescriptor();

            if (!detection) {
                return;
            }

            const match = faceMatcherRef.current.findBestMatch(detection.descriptor);
            if (match.label !== 'unknown' && match.distance < MATCH_THRESHOLD) {
                await handleEmployeeDetected(match.label);
            }
        } catch (err) {
            console.error('Detection error:', err);
        }
    }, [handleEmployeeDetected]);

    // بدء العملية عند جاهزية النماذج
    useEffect(() => {
        if (modelsReady) {
            startCamera().then(() => {
                loadFaceProfiles();
            });
        }
    }, [modelsReady, startCamera, loadFaceProfiles]);

    // تشغيل وإيقاف حلقة الكشف
    useEffect(() => {
        if (status === 'ready') {
            setStatus('detecting');
        }
    }, [status]);

    useEffect(() => {
        if (status === 'detecting') {
            intervalRef.current = setInterval(detectLoop, DETECTION_INTERVAL_MS);
        } else {
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
                intervalRef.current = null;
            }
        }
        return () => {
            if (intervalRef.current) clearInterval(intervalRef.current);
        };
    }, [status, detectLoop]);

    // تنظيف الموارد عند مغادرة الصفحة
    useEffect(() => {
        return () => {
            streamRef.current?.getTracks().forEach((t) => t.stop());
            if (intervalRef.current) clearInterval(intervalRef.current);
        };
    }, []);

    const getResultLabel = (result: LastResult) => {
        if (result.message) return result.message;
        switch (result.action) {
            case 'check-in':
                return t('faceRecognition.checkInSuccess');
            case 'break-start':
                return t('faceRecognition.breakStartSuccess');
            case 'break-end':
                return t('faceRecognition.breakEndSuccess');
            case 'check-out':
                return t('faceRecognition.checkOutSuccess');
            case 'already-done':
                return t('faceRecognition.alreadyCompleted');
            default:
                return t('faceRecognition.checkInSuccess');
        }
    };

    const getResultBadgeStyle = (result: LastResult) => {
        if (!result.success) return 'bg-red-600/95 border-red-500';
        switch (result.action) {
            case 'check-in':
                return 'bg-emerald-600/95 border-emerald-400 shadow-emerald-900/30';
            case 'break-start':
                return 'bg-amber-600/95 border-amber-400 shadow-amber-900/30';
            case 'break-end':
                return 'bg-blue-600/95 border-blue-400 shadow-blue-900/30';
            case 'check-out':
                return 'bg-orange-600/95 border-orange-400 shadow-orange-900/30';
            case 'already-done':
                return 'bg-slate-700/95 border-slate-500 shadow-slate-900/30';
            default:
                return 'bg-emerald-600/95 border-emerald-400';
        }
    };

    const getResultIcon = (result: LastResult) => {
        if (!result.success) return <XCircle className="h-6 w-6 shrink-0" />;
        switch (result.action) {
            case 'check-in':
                return <CheckCircle2 className="h-6 w-6 shrink-0" />;
            case 'break-start':
                return <Coffee className="h-6 w-6 shrink-0" />;
            case 'break-end':
                return <Briefcase className="h-6 w-6 shrink-0" />;
            case 'check-out':
                return <LogOut className="h-6 w-6 shrink-0" />;
            case 'already-done':
                return <ShieldAlert className="h-6 w-6 shrink-0" />;
            default:
                return <CheckCircle2 className="h-6 w-6 shrink-0" />;
        }
    };

    const statusBadge = () => {
        switch (status) {
            case 'loading-models':
                return <Badge variant="secondary"><Loader2 className="mr-1 h-3 w-3 animate-spin" />{t('faceRecognition.loadingModels')}</Badge>;
            case 'loading-profiles':
                return <Badge variant="secondary"><Loader2 className="mr-1 h-3 w-3 animate-spin" />{t('common.loading')}</Badge>;
            case 'ready':
                return <Badge variant="outline"><WifiOff className="mr-1 h-3 w-3" />{isRtl ? 'في انتظار الشفت' : 'Waiting for shift'}</Badge>;
            case 'no-shift':
                return <Badge variant="destructive">{t('faceRecognition.selectShift')}</Badge>;
            case 'detecting':
                return <Badge className="bg-blue-500"><ScanFace className="mr-1 h-3 w-3 animate-pulse" />{isRtl ? 'يراقب...' : 'Scanning...'}</Badge>;
            case 'processing':
                return <Badge className="bg-yellow-500"><Loader2 className="mr-1 h-3 w-3 animate-spin" />{isRtl ? 'يسجّل...' : 'Recording...'}</Badge>;
            case 'success':
                return <Badge className="bg-green-600"><Wifi className="mr-1 h-3 w-3" />{isRtl ? 'نشط' : 'Active'}</Badge>;
            case 'error':
                return <Badge variant="destructive"><XCircle className="mr-1 h-3 w-3" />{t('common.error')}</Badge>;
        }
    };

    return (
        <div className="mx-auto flex max-w-2xl flex-col gap-4 p-6">
            {/* شريط التنقل العلوي */}
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
                        className="gap-1.5 text-xs font-semibold shadow-sm transition-all"
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
                        onClick={() => navigate('/face-check-in')}
                    >
                        <ScanFace className="h-3.5 w-3.5" />
                        {t('faceRecognition.title')}
                    </Button>
                </div>
            </div>

            {/* الرأس */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
                        <ScanFace className="h-6 w-6 text-primary" />
                        {isRtl ? 'Kiosk — كشك الحضور والانصراف التلقائي' : 'Auto Kiosk Station'}
                    </h1>
                    <p className="text-muted-foreground text-sm">
                        {isCheckOutMode
                            ? (isRtl ? '⚠️ مفعل وضع الانصراف النهائي: سيتم تسجيل انصراف اليوم للموظف فور التعرف عليه' : '⚠️ Final Check-Out Mode Active: Will conclude workday on match')
                            : (isRtl ? 'المسح الذكي: حضور أولاً ➔ استراحة / خروج مؤقت ➔ عودة من الاستراحة تلقائياً' : 'Smart Workflow: Check-In ➔ Break Start ➔ Break Return seamlessly')}
                    </p>
                </div>
                {statusBadge()}
            </div>

            {/* شاشة الكاميرا */}
            <div className="relative overflow-hidden rounded-2xl border-2 border-primary/20 bg-black shadow-2xl"
                style={{ aspectRatio: '4/3' }}>
                <video
                    ref={videoRef}
                    autoPlay
                    muted
                    playsInline
                    className="h-full w-full object-cover"
                />
                
                {/* مؤشر الفحص النشط */}
                {(status === 'detecting' || status === 'processing') && (
                    <div className={`absolute inset-0 border-4 rounded-2xl animate-pulse pointer-events-none ${
                        isCheckOutMode ? 'border-orange-500/60' : 'border-primary/60'
                    }`} />
                )}

                {/* وضع العمل والوردية الحالية أعلى الكاميرا */}
                <div className="absolute top-3 left-3 right-3 flex justify-between items-center pointer-events-none gap-2 flex-wrap">
                    <div className="flex items-center gap-2">
                        <span className={`px-3 py-1 rounded-full text-xs font-bold text-white shadow-md backdrop-blur-md ${
                            isCheckOutMode ? 'bg-orange-600/90' : 'bg-primary/90'
                        }`}>
                            {isCheckOutMode ? t('faceRecognition.kioskModeCheckOut') : t('faceRecognition.kioskModeSmart')}
                        </span>
                        {effectiveShift && (
                            <span className="px-2.5 py-1 rounded-full text-[11px] bg-slate-900/80 text-white font-medium backdrop-blur-md flex items-center gap-1 border border-white/10 shadow">
                                <Sparkles className="h-3 w-3 text-amber-400" />
                                <span>{effectiveShift.name} ({effectiveShift.startTime.slice(0, 5)} - {effectiveShift.endTime.slice(0, 5)})</span>
                                {shiftId === AUTO_DETECT_SHIFT && (
                                    <span className="text-[10px] bg-primary/80 px-1 rounded text-white font-normal">
                                        {t('faceRecognition.autoDetected')}
                                    </span>
                                )}
                            </span>
                        )}
                    </div>
                    <div className="flex items-center gap-1.5">
                        <span className={`px-2.5 py-1 rounded-full text-[11px] font-medium backdrop-blur-md flex items-center gap-1 shadow ${
                            gpsStatus === 'granted'
                                ? 'bg-emerald-700/80 text-white'
                                : gpsStatus === 'loading'
                                ? 'bg-amber-600/80 text-white'
                                : 'bg-rose-700/80 text-white'
                        }`}>
                            <MapPin className="h-3 w-3" />
                            <span>
                                {gpsStatus === 'granted'
                                    ? (isRtl ? 'الموقع الجغرافي نشط' : 'GPS Active')
                                    : gpsStatus === 'loading'
                                    ? (isRtl ? 'جاري تحديد الموقع...' : 'Locating GPS...')
                                    : (isRtl ? 'الموقع غير متاح' : 'GPS Offline')}
                            </span>
                        </span>
                        <span className="px-2.5 py-1 rounded-full text-[11px] bg-black/60 text-white font-mono backdrop-blur-md">
                            {new Date().toLocaleTimeString(language === 'ar' ? 'ar-IQ' : 'en-US')}
                        </span>
                    </div>
                </div>

                {/* بطاقة النتيجة المنبثقة */}
                {lastResult && (
                    <div className={`absolute bottom-4 left-4 right-4 rounded-xl p-4 flex items-center gap-3.5 text-white border shadow-lg backdrop-blur-md transition-all duration-300 ${getResultBadgeStyle(lastResult)}`}>
                        {getResultIcon(lastResult)}
                        <div className="flex-1 min-w-0">
                            <div className="flex items-center justify-between gap-2">
                                <p className="font-bold text-base truncate">{lastResult.name}</p>
                                <span className="text-xs opacity-90 font-mono shrink-0">{lastResult.time}</span>
                            </div>
                            <p className="text-xs font-medium opacity-95 mt-0.5 leading-snug">
                                {getResultLabel(lastResult)}
                            </p>
                        </div>
                    </div>
                )}
            </div>

            {/* الإعدادات */}
            <Card className="border-border/60 shadow-sm">
                <CardHeader className="pb-2">
                    <CardTitle className="text-base flex items-center gap-2">
                        <Users className="h-4 w-4 text-primary" />
                        {isRtl ? 'إعدادات الـ Kiosk والوردية' : 'Kiosk & Shift Settings'}
                    </CardTitle>
                </CardHeader>
                <CardContent className="flex flex-col gap-3">
                    <div className="space-y-1">
                        <div className="flex items-center justify-between">
                            <Label>{t('faceRecognition.shift')}</Label>
                            {shiftId === AUTO_DETECT_SHIFT && effectiveShift && (
                                <span className="text-xs text-primary font-medium flex items-center gap-1">
                                    <Sparkles className="h-3 w-3" />
                                    {t('faceRecognition.autoDetectedShiftLabel')}: {effectiveShift.name}
                                </span>
                            )}
                        </div>
                        <Select value={shiftId} onValueChange={(v) => setShiftId(v ?? AUTO_DETECT_SHIFT)}>
                            <SelectTrigger>
                                <SelectValue placeholder={t('faceRecognition.selectShift')}>
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
                                        {s.name} ({s.startTime.slice(0, 5)} – {s.endTime.slice(0, 5)})
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="flex flex-wrap gap-4 text-xs sm:text-sm text-muted-foreground pt-1 border-t border-border/40">
                        <span>{isRtl ? 'موظفون مسجلون:' : 'Registered profiles:'} <strong>{profilesCount}</strong></span>
                        <span>{isRtl ? 'فحص كل:' : 'Interval:'} <strong>2s</strong></span>
                        <span>{isRtl ? 'دقة الكشف:' : 'Accuracy:'} <strong>{Math.round((1 - MATCH_THRESHOLD) * 100)}%</strong></span>
                        <span>{isRtl ? 'فترة التهدئة:' : 'Cooldown:'} <strong>30s</strong></span>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

export default AutoKioskPage;
