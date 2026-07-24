import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { LeaveStatusBadge } from './LeaveStatusBadge';
import type { LeaveDetailsDto } from '../types';
import { LeaveType } from '../types';

interface LeaveDetailsCardProps {
  leave: LeaveDetailsDto;
}

export function LeaveDetailsCard({ leave }: LeaveDetailsCardProps) {
  const getLeaveTypeText = (type: string | number) => {
    switch (Number(type)) {
      case LeaveType.Annual: return 'Annual';
      case LeaveType.Sick: return 'Sick';
      case LeaveType.Maternity: return 'Maternity';
      case LeaveType.Paternity: return 'Paternity';
      case LeaveType.Unpaid: return 'Unpaid';
      case LeaveType.Other: return 'Other';
      default: return type;
    }
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      <Card>
        <CardHeader>
          <CardTitle>Leave Information</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <span className="block text-sm font-medium text-gray-500">Employee ID</span>
            <span className="block text-sm text-gray-900 mt-1">{leave.employeeId}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Leave Type</span>
            <span className="block text-sm text-gray-900 mt-1">{getLeaveTypeText(leave.leaveType)}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Date Range</span>
            <span className="block text-sm text-gray-900 mt-1">
              {format(new Date(leave.startDate), 'PPP')} to {format(new Date(leave.endDate), 'PPP')}
            </span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Duration</span>
            <span className="block text-sm text-gray-900 mt-1">
              {Math.ceil((new Date(leave.endDate).getTime() - new Date(leave.startDate).getTime()) / (1000 * 3600 * 24)) + 1} days
            </span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Status</span>
            <div className="mt-1">
              <LeaveStatusBadge status={leave.status} />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Details & Comments</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div>
            <span className="block text-sm font-medium text-gray-500">Reason</span>
            <div className="mt-2 text-sm text-gray-900 bg-gray-50 p-4 rounded-md border border-gray-100 min-h-24 whitespace-pre-wrap">
              {leave.reason || 'No reason provided.'}
            </div>
          </div>
          
          <div>
            <span className="block text-sm font-medium text-gray-500">Manager's Comment</span>
            <div className="mt-2 text-sm text-gray-900 bg-blue-50 p-4 rounded-md border border-blue-100 min-h-24 whitespace-pre-wrap">
              {leave.managerComment || 'No comments from manager yet.'}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
