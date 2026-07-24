import { Badge } from '@/shared/components/ui/badge';
import { AttendanceStatus } from '../types';

interface AttendanceStatusBadgeProps {
  status: AttendanceStatus;
}

export function AttendanceStatusBadge({ status }: AttendanceStatusBadgeProps) {
  switch (status) {
    case AttendanceStatus.Present:
      return <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Present</Badge>;
    case AttendanceStatus.Pending:
      return <Badge variant="secondary">Pending</Badge>;
    case AttendanceStatus.Absent:
      return <Badge variant="destructive">Absent</Badge>;
    case AttendanceStatus.Late:
      return <Badge variant="default" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Late</Badge>;
    case AttendanceStatus.EarlyLeave:
      return <Badge variant="default" className="bg-orange-100 text-orange-800 hover:bg-orange-100">Early Leave</Badge>;
    case AttendanceStatus.Invalid:
    default:
      return <Badge variant="outline">Invalid</Badge>;
  }
}
