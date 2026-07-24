import { Badge } from '@/shared/components/ui/badge';

interface CompanyStatusBadgeProps {
  isActive: boolean;
}

export const CompanyStatusBadge = ({ isActive }: CompanyStatusBadgeProps) => {
  return (
    <Badge variant={isActive ? 'default' : 'secondary'} className={isActive ? 'bg-green-500 hover:bg-green-600' : ''}>
      {isActive ? 'Active' : 'Inactive'}
    </Badge>
  );
};
