import { Badge } from '@/shared/components/ui/badge';

interface BranchStatusBadgeProps {
  isActive: boolean;
}

export const BranchStatusBadge = ({ isActive }: BranchStatusBadgeProps) => {
  return (
    <Badge variant={isActive ? 'default' : 'secondary'} className={isActive ? 'bg-green-500 hover:bg-green-600' : ''}>
      {isActive ? 'Active' : 'Inactive'}
    </Badge>
  );
};
