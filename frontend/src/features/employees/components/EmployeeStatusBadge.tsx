import { Badge } from '@/shared/components/ui/badge';
import type { EmployeeStatus } from '../types';

interface EmployeeStatusBadgeProps {
  status: EmployeeStatus;
}

export const EmployeeStatusBadge = ({ status }: EmployeeStatusBadgeProps) => {
  switch (status) {
    case 0:
      return <Badge className="bg-green-500 hover:bg-green-600">Active</Badge>;
    case 1:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-200">On Leave</Badge>;
    case 2:
      return <Badge variant="destructive" className="bg-red-500 hover:bg-red-600">Suspended</Badge>;
    case 3:
      return <Badge variant="outline" className="text-gray-500 border-gray-500">Terminated</Badge>;
    default:
      return <Badge variant="secondary">Unknown</Badge>;
  }
};
