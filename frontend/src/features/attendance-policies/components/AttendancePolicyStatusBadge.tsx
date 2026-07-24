import { Badge } from '@/shared/components/ui/badge';

interface AttendancePolicyStatusBadgeProps {
  isActive: boolean;
}

export function AttendancePolicyStatusBadge({ isActive }: AttendancePolicyStatusBadgeProps) {
  return (
    <Badge variant={isActive ? 'default' : 'secondary'}>
      {isActive ? 'Active' : 'Inactive'}
    </Badge>
  );
}
