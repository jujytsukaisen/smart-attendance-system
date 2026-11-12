import { Badge } from '@/shared/components/ui/badge';
import { useTranslation } from '@/shared/i18n';

interface LeaveStatusBadgeProps {
  status: string | number;
}

export function LeaveStatusBadge({ status }: LeaveStatusBadgeProps) {
  const { t } = useTranslation();
  const str = String(status || '').trim().toLowerCase();

  if (str === 'approved' || str === '2') {
    return (
      <Badge variant="outline" className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/25 font-medium">
        {t('leaves.approved')}
      </Badge>
    );
  }
  if (str === 'pending' || str === '1' || str === '') {
    return (
      <Badge variant="outline" className="bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-500/25 font-medium">
        {t('leaves.pending')}
      </Badge>
    );
  }
  if (str === 'rejected' || str === '3') {
    return (
      <Badge variant="outline" className="bg-rose-500/15 text-rose-700 dark:text-rose-400 border-rose-500/25 font-medium">
        {t('leaves.rejected')}
      </Badge>
    );
  }
  if (str === 'cancelled' || str === '4') {
    return (
      <Badge variant="outline" className="bg-muted text-muted-foreground border-border font-medium">
        {t('leaves.cancelled')}
      </Badge>
    );
  }
  return <Badge variant="outline" className="bg-muted text-muted-foreground border-border">{status}</Badge>;
}
