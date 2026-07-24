import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { CalendarClock, CheckCircle, XCircle, Users } from 'lucide-react';

interface LeaveSummaryCardsProps {
  pendingCount?: number;
  approvedCount?: number;
  rejectedCount?: number;
  onLeaveTodayCount?: number;
}

export function LeaveSummaryCards({
  pendingCount = 0,
  approvedCount = 0,
  rejectedCount = 0,
  onLeaveTodayCount = 0
}: LeaveSummaryCardsProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-6">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-gray-500">Pending Leaves</CardTitle>
          <CalendarClock className="w-4 h-4 text-yellow-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-gray-900">{pendingCount}</div>
        </CardContent>
      </Card>
      
      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-gray-500">Approved Leaves</CardTitle>
          <CheckCircle className="w-4 h-4 text-green-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-gray-900">{approvedCount}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-gray-500">Rejected Leaves</CardTitle>
          <XCircle className="w-4 h-4 text-red-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-gray-900">{rejectedCount}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-gray-500">On Leave Today</CardTitle>
          <Users className="w-4 h-4 text-blue-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-gray-900">{onLeaveTodayCount}</div>
        </CardContent>
      </Card>
    </div>
  );
}
