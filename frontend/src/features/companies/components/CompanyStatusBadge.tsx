import { Badge } from '@/shared/components/ui/badge';
import { useTranslation } from '@/shared/i18n';

interface CompanyStatusBadgeProps {
  isActive: boolean;
}

export const CompanyStatusBadge = ({ isActive }: CompanyStatusBadgeProps) => {
  const { t } = useTranslation();
  return (
    <Badge variant={isActive ? 'default' : 'secondary'} className={isActive ? 'bg-emerald-600 hover:bg-emerald-700 text-white dark:bg-emerald-600' : ''}>
      {isActive ? t('common.active') : t('common.inactive')}
    </Badge>
  );
};
