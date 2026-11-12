import { Badge } from '@/shared/components/ui/badge';
import { AttendanceStatus } from '../types';
import { useTranslation } from '@/shared/i18n';

interface AttendanceStatusBadgeProps {
  status: AttendanceStatus | string | number;
}

export function AttendanceStatusBadge({ status }: AttendanceStatusBadgeProps) {
  const { t } = useTranslation();

  const normalizedStatus = (() => {
    if (typeof status === 'number') {
      switch (status) {
        case 1: return 'Pending';
        case 2: return 'Present';
        case 3: return 'Absent';
        case 4: return 'Late';
        case 5: return 'EarlyLeave';
        case 6: return 'PresentDuringLeave';
        case 7: return 'LateDuringLeave';
        case 8: return 'Invalid';
        default: return 'Unknown';
      }
    }
    return String(status || '');
  })();

  switch (normalizedStatus.toLowerCase()) {
    case 'present':
    case '2':
      return (
        <Badge variant="outline" className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/25">
          {t('attendance.present')}
        </Badge>
      );
    case 'pending':
    case '1':
      return (
        <Badge variant="outline" className="bg-sky-500/15 text-sky-700 dark:text-sky-400 border-sky-500/25">
          {t('attendance.pending')}
        </Badge>
      );
    case 'absent':
    case '3':
      return (
        <Badge variant="outline" className="bg-rose-500/15 text-rose-700 dark:text-rose-400 border-rose-500/25">
          {t('attendance.absent')}
        </Badge>
      );
    case 'late':
    case '4':
      return (
        <Badge variant="outline" className="bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-500/25">
          {t('attendance.late')}
        </Badge>
      );
    case 'earlyleave':
    case '5':
      return (
        <Badge variant="outline" className="bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-200/20">
          {t('attendance.earlyLeave')}
        </Badge>
      );
    case 'presentduringleave':
    case '6':
      return (
        <Badge variant="outline" className="bg-purple-500/10 text-purple-600 dark:text-purple-400 border-purple-200/20">
          {t('attendance.presentDuringLeave') || 'Present (On Leave)'}
        </Badge>
      );
    case 'lateduringleave':
    case '7':
      return (
        <Badge variant="outline" className="bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-200/20">
          {t('attendance.lateDuringLeave') || 'Late (On Leave)'}
        </Badge>
      );
    case 'invalid':
    case '8':
      return (
        <Badge variant="outline" className="bg-muted text-muted-foreground border-border">
          {t('attendance.invalid')}
        </Badge>
      );
    default:
      return (
        <Badge variant="outline" className="bg-muted text-muted-foreground border-border">
          {normalizedStatus || 'Unknown'}
        </Badge>
      );
  }
}
