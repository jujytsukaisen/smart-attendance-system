/**
 * useSystemFormatters
 * 
 * Hook مشترك يقرأ إعدادات DateFormat و TimeFormat من Settings API
 * ويوفر دوال تنسيق جاهزة تُستخدم في كل أنحاء التطبيق.
 * 
 * الاستخدام:
 *   const { formatDate, formatTime, formatDateTime } = useSystemFormatters();
 *   formatDate(new Date())         // "03/09/2026" أو "2026-09-03" حسب الإعداد
 *   formatTime(new Date())         // "9:30 PM" أو "21:30" حسب الإعداد
 *   formatDateTime(new Date())     // التاريخ + الوقت معاً
 */

import { useCallback } from 'react';
import { useGeneralSettings } from '@/features/settings/hooks';
import { useTranslation } from '@/shared/i18n';

export function useSystemFormatters() {
    const { data: settings } = useGeneralSettings();
    const { language } = useTranslation();

    const dateFormat = settings?.dateFormat ?? 'DD/MM/YYYY';
    const timeFormat = settings?.timeFormat ?? '24h';
    const locale = language === 'ar' ? 'ar-IQ' : 'en-US';

    /**
     * تنسيق تاريخ حسب إعداد DateFormat المحفوظ في Settings
     */
    const formatDate = useCallback((date: Date | string | null | undefined): string => {
        if (!date) return '—';
        const d = typeof date === 'string' ? new Date(date) : date;
        if (isNaN(d.getTime())) return '—';

        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = String(d.getFullYear());

        switch (dateFormat) {
            case 'YYYY-MM-DD': return `${year}-${month}-${day}`;
            case 'MM/DD/YYYY': return `${month}/${day}/${year}`;
            case 'DD/MM/YYYY':
            default:           return `${day}/${month}/${year}`;
        }
    }, [dateFormat]);

    /**
     * تنسيق وقت حسب إعداد TimeFormat (12h / 24h)
     */
    const formatTime = useCallback((date: Date | string | null | undefined): string => {
        if (!date) return '—';
        const d = typeof date === 'string' ? new Date(date) : date;
        if (isNaN(d.getTime())) return '—';

        if (timeFormat === '12h') {
            return d.toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit', hour12: true });
        }
        // 24h
        const hh = String(d.getHours()).padStart(2, '0');
        const mm = String(d.getMinutes()).padStart(2, '0');
        return `${hh}:${mm}`;
    }, [timeFormat, locale]);

    /**
     * تنسيق تاريخ + وقت معاً
     */
    const formatDateTime = useCallback((date: Date | string | null | undefined): string => {
        if (!date) return '—';
        return `${formatDate(date)} ${formatTime(date)}`;
    }, [formatDate, formatTime]);

    /**
     * تنسيق وقت من string بصيغة "HH:mm:ss" أو "HH:mm"
     * مفيد لعرض أوقات الوردية والحضور
     */
    const formatTimeString = useCallback((timeStr: string | null | undefined): string => {
        if (!timeStr) return '—';
        const parts = timeStr.split(':');
        if (parts.length < 2) return timeStr;
        const hours = parseInt(parts[0], 10);
        const minutes = parts[1];
        if (isNaN(hours)) return timeStr;

        if (timeFormat === '12h') {
            const period = hours >= 12 ? (language === 'ar' ? 'م' : 'PM') : (language === 'ar' ? 'ص' : 'AM');
            const h12 = hours % 12 || 12;
            return `${h12}:${minutes} ${period}`;
        }
        return `${String(hours).padStart(2, '0')}:${minutes}`;
    }, [timeFormat, language]);

    return { formatDate, formatTime, formatDateTime, formatTimeString, dateFormat, timeFormat };
}
