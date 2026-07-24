import { Badge } from '@/shared/components/ui/badge';
import { LeaveStatus } from '../types';

interface LeaveStatusBadgeProps {
  status: string | number;
}

export function LeaveStatusBadge({ status }: LeaveStatusBadgeProps) {
  // Map both string ('Pending', 'Approved') and numeric (1, 2) values
  let mappedStatus = status;
  if (typeof status === 'number' || !isNaN(Number(status))) {
    switch (Number(status)) {
      case LeaveStatus.Pending:
        mappedStatus = 'Pending';
        break;
      case LeaveStatus.Approved:
        mappedStatus = 'Approved';
        break;
      case LeaveStatus.Rejected:
        mappedStatus = 'Rejected';
        break;
      case LeaveStatus.Cancelled:
        mappedStatus = 'Cancelled';
        break;
    }
  }

  switch (mappedStatus) {
    case 'Approved':
      return <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Approved</Badge>;
    case 'Pending':
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Pending</Badge>;
    case 'Rejected':
      return <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Rejected</Badge>;
    case 'Cancelled':
      return <Badge variant="outline" className="bg-gray-100 text-gray-800 hover:bg-gray-100">Cancelled</Badge>;
    default:
      return <Badge variant="outline">{mappedStatus}</Badge>;
  }
}
