import { Badge } from '@/shared/components/ui/badge';

interface DepartmentStatusBadgeProps {
  isActive: boolean;
}

export const DepartmentStatusBadge = ({ isActive }: DepartmentStatusBadgeProps) => {
  return (
    <Badge variant={isActive ? 'default' : 'secondary'} className={isActive ? 'bg-green-500 hover:bg-green-600' : ''}>
      {isActive ? 'Active' : 'Inactive'}
    </Badge>
  );
};
