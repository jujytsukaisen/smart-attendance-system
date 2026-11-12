import { useCallback, useEffect, useRef, useState } from 'react';
import * as faceapi from 'face-api.js';
import { Camera, Loader2, RefreshCw, CheckCircle2, Smartphone, AlertCircle, ArrowUp, ArrowDown, ArrowLeft, ArrowRight, Crosshair } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useFaceApiModels } from '../hooks/useFaceApiModels';
import { useTranslation } from '@/shared/i18n';

interface FaceCaptureProps {
    onCapture: (descriptors: Float32Array[]) => void;
    disabled?: boolean;
    totalSteps?: number;
}

const STEP_INSTRUCTIONS_AR = [
    { text: 'انظر للأمام مباشرةً', icon: 'center' },
    { text: 'أمِل رأسك للأعلى قليلاً', icon: 'up' },
    { text: 'أمِل رأسك للأسفل قليلاً', icon: 'down' },
    { text: 'التفت قليلاً لليمين', icon: 'right' },
    { text: 'التفت قليلاً لليسار', icon: 'left' },
];

const STEP_INSTRUCTIONS_EN = [
    { text: 'Look straight ahead', icon: 'center' },
    { text: 'Tilt your head up slightly', icon: 'up' },
    { text: 'Tilt your head down slightly', icon: 'down' },
    { text: 'Turn slightly to the right', icon: 'right' },
    { text: 'Turn slightly to the left', icon: 'left' },
];

function DirectionIcon({ direction }: { direction: string }) {
    const cls = 'h-6 w-6 text-primary';
    switch (direction) {
        case 'up': return <ArrowUp className={cls} />;
        case 'down': return <ArrowDown className={cls} />;
        case 'left': return <ArrowLeft className={cls} />;
        case 'right': return <ArrowRight className={cls} />;
        default: return <Crosshair className={cls} />;
    }
}

// مكوّن التقاط وتحليل الوجه: يدعم التقاط 5 صور من اتجاهات مختلفة لزيادة الدقة
export function FaceCapture({ onCapture, disabled, totalSteps = 5 }: FaceCaptureProps) {
    const { isReady, error: modelsError } = useFaceApiModels();
    const { t, isRtl } = useTranslation();
    const videoRef = useRef<HTMLVideoElement>(null);
    const imageRef = useRef<HTMLImageElement>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const streamRef = useRef<MediaStream | null>(null);

    const [isCameraOn, setIsCameraOn] = useState(false);
    const [isDetecting, setIsDetecting] = useState(false);
    const [status, setStatus] = useState<'idle' | 'no-face' | 'multiple-faces' | 'step-captured' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [capturedImageUrl, setCapturedImageUrl] = useState<string | null>(null);
    const [isHttpMobile, setIsHttpMobile] = useState(false);

    // Multi-step states
    const [currentStep, setCurrentStep] = useState(0); // 0-indexed
    const [capturedDescriptors, setCapturedDescriptors] = useState<Float32Array[]>([]);

    const instructions = isRtl ? STEP_INSTRUCTIONS_AR : STEP_INSTRUCTIONS_EN;
    const currentInstruction = instructions[currentStep] ?? instructions[0];

    useEffect(() => {
        const isNotSecure = window.location.protocol !== 'https:' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1';
        setIsHttpMobile(isNotSecure);
    }, []);

    const startCamera = useCallback(async () => {
        try {
            setErrorMessage(null);
            setCapturedImageUrl(null);

            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                setErrorMessage(
                    isRtl
                        ? 'متصفح الهاتف يحجب البث المباشر عبر HTTP. يرجى استخدام زر "التقاط صورة بكاميرا الهاتف" أدناه.'
                        : 'Camera live stream restricted on HTTP mobile browser. Please use the phone camera capture button below.'
                );
                setStatus('error');
                return;
            }

            const stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'user',
                    width: { ideal: 640 },
                    height: { ideal: 480 },
                },
            });
            streamRef.current = stream;
            if (videoRef.current) {
                videoRef.current.srcObject = stream;
            }
            setIsCameraOn(true);
            setStatus('idle');
        } catch (err: any) {
            const isPermissionDenied = err?.name === 'NotAllowedError' || err?.name === 'PermissionDeniedError';
            if (isPermissionDenied) {
                setErrorMessage(
                    isRtl
                        ? 'تم رفض إذن الكاميرا. يرجى السماح بالوصول للكاميرا من إعدادات المتصفح.'
                        : 'Camera permission was denied. Please allow camera access in browser settings.'
                );
            } else {
                setErrorMessage(
                    isRtl
                        ? 'تعذر تشغيل الكاميرا. يرجى تجربة زر "التقاط صورة بالهاتف" أدناه.'
                        : 'Could not start camera. Please try the phone camera button below.'
                );
            }
            setStatus('error');
            setIsCameraOn(false);
        }
    }, [isRtl]);

    const stopCamera = useCallback(() => {
        if (streamRef.current) {
            streamRef.current.getTracks().forEach((track) => track.stop());
            streamRef.current = null;
        }
        setIsCameraOn(false);
    }, []);

    useEffect(() => {
        return () => {
            stopCamera();
            if (capturedImageUrl) {
                URL.revokeObjectURL(capturedImageUrl);
            }
        };
    }, [stopCamera, capturedImageUrl]);

    const handleStepCaptured = useCallback((descriptor: Float32Array) => {
        const newDescriptors = [...capturedDescriptors, descriptor];
        setCapturedDescriptors(newDescriptors);
        setStatus('step-captured');

        if (newDescriptors.length >= totalSteps) {
            // All steps done — fire the final callback
            stopCamera();
            onCapture(newDescriptors);
        } else {
            // Move to next step after brief delay
            setTimeout(() => {
                setCurrentStep(newDescriptors.length);
                setCapturedImageUrl(null);
                setStatus('idle');
                if (fileInputRef.current) fileInputRef.current.value = '';
                // Re-start camera if it was stopped between steps (e.g., phone capture)
            }, 800);
        }
    }, [capturedDescriptors, totalSteps, onCapture, stopCamera]);

    // تحليل الوجه من عنصر الصورة الثابتة
    const processImageElement = async (imgEl: HTMLImageElement) => {
        setIsDetecting(true);
        setErrorMessage(null);
        try {
            const detection = await faceapi
                .detectSingleFace(imgEl, new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.5 }))
                .withFaceLandmarks()
                .withFaceDescriptor();

            if (!detection) {
                setStatus('no-face');
                setErrorMessage(t('faceRecognition.noFaceDetected'));
                return;
            }

            handleStepCaptured(detection.descriptor);
        } catch (err: any) {
            console.error('Error processing image:', err);
            setStatus('error');
            setErrorMessage(t('faceRecognition.modelsError'));
        } finally {
            setIsDetecting(false);
        }
    };

    // معالجة الصورة المأخوذة من كاميرا الهاتف
    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        stopCamera();
        if (capturedImageUrl) URL.revokeObjectURL(capturedImageUrl);

        const url = URL.createObjectURL(file);
        setCapturedImageUrl(url);
        setStatus('idle');
        setErrorMessage(null);
    };

    // معالجة الوجه من البث المباشر للفيديو
    const handleCaptureFromVideo = async () => {
        if (!videoRef.current || !isCameraOn) return;
        setIsDetecting(true);
        setErrorMessage(null);

        try {
            const detection = await faceapi
                .detectSingleFace(videoRef.current, new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.5 }))
                .withFaceLandmarks()
                .withFaceDescriptor();

            if (!detection) {
                setStatus('no-face');
                setErrorMessage(t('faceRecognition.noFaceDetected'));
                return;
            }

            handleStepCaptured(detection.descriptor);
        } catch (err: any) {
            console.error('Error detecting face from video:', err);
            setStatus('error');
            setErrorMessage(t('faceRecognition.modelsError'));
        } finally {
            setIsDetecting(false);
        }
    };

    const handleReset = () => {
        setCapturedDescriptors([]);
        setCurrentStep(0);
        setCapturedImageUrl(null);
        setStatus('idle');
        setErrorMessage(null);
        if (fileInputRef.current) fileInputRef.current.value = '';
        stopCamera();
    };

    if (modelsError) {
        return (
            <div className="flex flex-col items-center justify-center p-6 text-center text-destructive border border-destructive/20 rounded-lg bg-destructive/5 gap-2">
                <AlertCircle className="h-8 w-8" />
                <p className="text-sm font-medium">{t('faceRecognition.modelsError')}</p>
                <p className="text-xs text-muted-foreground">{modelsError.message}</p>
            </div>
        );
    }

    const completedSteps = capturedDescriptors.length;
    const progressPercent = (completedSteps / totalSteps) * 100;
    const allDone = completedSteps >= totalSteps;

    return (
        <div className="flex flex-col items-center gap-4 w-full">

            {/* شريط التقدم وعداد الخطوات — يظهر فقط في وضع التسجيل متعدد الخطوات */}
            {totalSteps > 1 && (
                <div className="w-full space-y-1.5">
                    <div className="flex justify-between items-center text-xs">
                        <span className="font-medium text-foreground">
                            {isRtl
                                ? `الخطوة ${Math.min(completedSteps + 1, totalSteps)} من ${totalSteps}`
                                : `Step ${Math.min(completedSteps + 1, totalSteps)} of ${totalSteps}`}
                        </span>
                        <span className="text-muted-foreground">
                            {completedSteps}/{totalSteps} {isRtl ? 'مكتملة' : 'captured'}
                        </span>
                    </div>
                    <div className="w-full h-2 bg-muted rounded-full overflow-hidden">
                        <div
                            className="h-full bg-primary transition-all duration-500 ease-out rounded-full"
                            style={{ width: `${progressPercent}%` }}
                        />
                    </div>
                    {/* أيقونات الخطوات */}
                    <div className="flex justify-between mt-1">
                        {instructions.map((step, idx) => (
                            <div
                                key={idx}
                                className={`flex flex-col items-center gap-0.5 transition-all duration-300 ${
                                    idx < completedSteps
                                        ? 'opacity-100'
                                        : idx === completedSteps
                                        ? 'opacity-100 scale-110'
                                        : 'opacity-30'
                                }`}
                            >
                                <div className={`p-1 rounded-full ${
                                    idx < completedSteps
                                        ? 'bg-green-500/20'
                                        : idx === completedSteps
                                        ? 'bg-primary/20'
                                        : 'bg-muted'
                                }`}>
                                    {idx < completedSteps ? (
                                        <CheckCircle2 className="h-3.5 w-3.5 text-green-500" />
                                    ) : (
                                        <DirectionIcon direction={step.icon} />
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* التعليم الحالي — يظهر فقط في وضع التسجيل متعدد الخطوات */}
            {!allDone && totalSteps > 1 && (
                <div className="flex items-center gap-2 w-full px-3 py-2 rounded-lg bg-primary/10 border border-primary/20 text-sm font-medium text-primary">
                    <DirectionIcon direction={currentInstruction.icon} />
                    <span>{currentInstruction.text}</span>
                </div>
            )}

            {/* نافذة الكاميرا / عرض الصورة الملتقطة */}
            <div className="relative aspect-video w-full max-w-sm rounded-xl overflow-hidden bg-muted/60 border shadow-inner flex items-center justify-center">
                {capturedImageUrl ? (
                    <img
                        ref={imageRef}
                        src={capturedImageUrl}
                        alt="Captured Face"
                        className="h-full w-full object-cover"
                        onLoad={() => {
                            if (imageRef.current) processImageElement(imageRef.current);
                        }}
                    />
                ) : (
                    <video
                        ref={videoRef}
                        autoPlay
                        muted
                        playsInline
                        className="h-full w-full object-cover"
                        style={{ display: isCameraOn ? 'block' : 'none' }}
                    />
                )}

                {!isCameraOn && !capturedImageUrl && (
                    <div className="flex flex-col items-center justify-center text-muted-foreground p-4 text-center">
                        <Camera className="h-12 w-12 mb-2 stroke-1 opacity-60" />
                        <p className="text-xs">
                            {isRtl ? 'اضغط على زر فتح الكاميرا أو التقاط صورة بالهاتف' : 'Click start camera or capture photo using phone'}
                        </p>
                    </div>
                )}

                {isDetecting && (
                    <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/60 text-white gap-2">
                        <Loader2 className="h-10 w-10 animate-spin text-primary" />
                        <span className="text-sm font-medium">{t('faceRecognition.analyzing')}</span>
                    </div>
                )}

                {status === 'step-captured' && !allDone && (
                    <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/50 text-white gap-2">
                        <CheckCircle2 className="h-14 w-14 text-green-400 animate-in zoom-in-50 duration-200" />
                        <span className="text-sm font-semibold bg-green-950/80 px-3 py-1 rounded-full text-green-300 border border-green-500/40">
                            {isRtl ? 'تم التقاط الصورة ✓' : 'Captured ✓'}
                        </span>
                    </div>
                )}

                {allDone && (
                    <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/60 text-white gap-2">
                        <CheckCircle2 className="h-16 w-16 text-green-400 animate-in zoom-in-50 duration-300" />
                        <span className="text-base font-bold text-green-300">
                            {isRtl ? 'تم تسجيل الوجه بنجاح!' : 'Face Enrolled Successfully!'}
                        </span>
                        <span className="text-xs text-green-400/80">
                            {isRtl ? `${totalSteps} صور تم التقاطها` : `${totalSteps} images captured`}
                        </span>
                    </div>
                )}
            </div>

            {/* رسائل التنبيه والخطأ */}
            {errorMessage && (
                <div className="flex items-start gap-2 p-3 text-xs rounded-lg border border-destructive/30 bg-destructive/10 text-destructive w-full">
                    <AlertCircle className="h-4 w-4 shrink-0 mt-0.5" />
                    <span>{errorMessage}</span>
                </div>
            )}

            {isHttpMobile && !isCameraOn && !capturedImageUrl && (
                <div className="flex items-center gap-2 p-2.5 text-xs rounded-lg border border-amber-500/30 bg-amber-500/10 text-amber-600 dark:text-amber-400 w-full">
                    <Smartphone className="h-4 w-4 shrink-0" />
                    <span>
                        {isRtl
                            ? 'على متصفح الهاتف: استخدم زر كاميرا الهاتف بالأسفل لالتقاط الصورة فوراً.'
                            : 'On mobile browser: Use the phone camera button below to take a photo directly.'}
                    </span>
                </div>
            )}

            {/* مدخل الكاميرا الأصلي للهاتف (يعمل بدون HTTPS) */}
            <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                capture="user"
                className="hidden"
                onChange={handleFileSelect}
            />

            {/* أزرار التحكم */}
            {!allDone && (
                <div className="flex flex-wrap justify-center gap-2 w-full">
                    {/* زر التقاط صورة الهاتف المباشر */}
                    <Button
                        type="button"
                        variant="default"
                        onClick={() => fileInputRef.current?.click()}
                        disabled={!isReady || isDetecting || disabled}
                        className="flex-1 min-w-[170px]"
                    >
                        <Smartphone className="mr-2 h-4 w-4" />
                        {t('faceRecognition.phoneCamera')}
                    </Button>

                    {/* زر فتح البث المباشر */}
                    {!isCameraOn ? (
                        <Button
                            type="button"
                            variant="outline"
                            onClick={startCamera}
                            disabled={!isReady || isDetecting || disabled}
                        >
                            <Camera className="mr-2 h-4 w-4" />
                            {t('faceRecognition.liveStream')}
                        </Button>
                    ) : (
                        <>
                            <Button
                                type="button"
                                variant="secondary"
                                onClick={handleCaptureFromVideo}
                                disabled={isDetecting || disabled}
                            >
                                <CheckCircle2 className="mr-2 h-4 w-4 text-green-500" />
                                {isRtl ? 'التقاط هذه الصورة' : 'Capture This Shot'}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={startCamera}
                                disabled={isDetecting}
                            >
                                <RefreshCw className="h-4 w-4" />
                            </Button>
                        </>
                    )}
                </div>
            )}

            {/* زر إعادة البدء */}
            {completedSteps > 0 && !allDone && (
                <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={handleReset}
                    className="text-muted-foreground text-xs"
                >
                    <RefreshCw className="mr-1.5 h-3 w-3" />
                    {isRtl ? 'إعادة البدء من الأول' : 'Start Over'}
                </Button>
            )}
        </div>
    );
}