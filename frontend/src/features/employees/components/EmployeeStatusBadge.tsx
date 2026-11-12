import { Badge } from '@/shared/components/ui/badge';
import type { EmployeeStatus } from '../types';
import { useTranslation } from '@/shared/i18n';

interface EmployeeStatusBadgeProps {
  status: EmployeeStatus;
}

export const EmployeeStatusBadge = ({ status }: EmployeeStatusBadgeProps) => {
  const { t } = useTranslation();

  switch (status) {
    case 'Active':
      return (
        <Badge variant="outline" className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/25">
          {t('employees.active')}
        </Badge>
      );
    case 'OnLeave':
      return (
        <Badge variant="outline" className="bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-500/25">
          {t('employees.onLeave')}
        </Badge>
      );
    case 'Inactive':
      return (
        <Badge variant="outline" className="bg-rose-500/15 text-rose-700 dark:text-rose-400 border-rose-500/25">
          {t('employees.inactive')}
        </Badge>
      );
    case 'Terminated':
      return (
        <Badge variant="outline" className="bg-muted text-muted-foreground border-border">
          {t('employees.terminated')}
        </Badge>
      );
    default:
      return (
        <Badge variant="outline" className="bg-muted text-muted-foreground border-border">
          {status || 'Unknown'}
        </Badge>
      );
  }
};