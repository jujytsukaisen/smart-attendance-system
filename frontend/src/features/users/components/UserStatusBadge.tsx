import { Badge } from '@/shared/components/ui/badge';

interface UserStatusBadgeProps {
  isActive: boolean;
}

export function UserStatusBadge({ isActive }: UserStatusBadgeProps) {
  if (isActive) {
    return <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Active</Badge>;
  }
  return <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Suspended</Badge>;
}
