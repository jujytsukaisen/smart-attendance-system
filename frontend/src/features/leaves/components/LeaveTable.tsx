import { useState } from 'react';
import { format } from 'date-fns';
import { Eye, Edit2, Trash2, Check, X, Slash } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog } from './ActionDialog';
import { LeaveStatusBadge } from './LeaveStatusBadge';
import { 
  useDeleteLeave, 
  useApproveLeave, 
  useRejectLeave, 
  useCancelLeave 
} from '../hooks';
import type { LeaveListDto } from '../types';
import { LeaveStatus, LeaveType } from '../types';
import { toast } from 'sonner';

interface LeaveTableProps {
  data: LeaveListDto[];
}

export function LeaveTable({ data }: LeaveTableProps) {
  const navigate = useNavigate();
  const deleteMutation = useDeleteLeave();
  const approveMutation = useApproveLeave();
  const rejectMutation = useRejectLeave();
  const cancelMutation = useCancelLeave();

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [approveId, setApproveId] = useState<string | null>(null);
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [cancelId, setCancelId] = useState<string | null>(null);

  const handleDelete = () => {
    if (!deleteId) return;
    deleteMutation.mutate(deleteId, {
      onSuccess: () => {
        toast.success('Leave request deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error('Failed to delete leave request');
        setDeleteId(null);
      },
    });
  };

  const handleApprove = () => {
    if (!approveId) return;
    approveMutation.mutate({ leaveId: approveId, comment: '' }, {
      onSuccess: () => {
        toast.success('Leave request approved');
        setApproveId(null);
      },
      onError: () => {
        toast.error('Failed to approve leave request');
        setApproveId(null);
      },
    });
  };

  const handleReject = () => {
    if (!rejectId) return;
    rejectMutation.mutate({ leaveId: rejectId, reason: 'Rejected by manager' }, {
      onSuccess: () => {
        toast.success('Leave request rejected');
        setRejectId(null);
      },
      onError: () => {
        toast.error('Failed to reject leave request');
        setRejectId(null);
      },
    });
  };

  const handleCancel = () => {
    if (!cancelId) return;
    cancelMutation.mutate({ leaveId: cancelId, reason: 'Cancelled by user' }, {
      onSuccess: () => {
        toast.success('Leave request cancelled');
        setCancelId(null);
      },
      onError: () => {
        toast.error('Failed to cancel leave request');
        setCancelId(null);
      },
    });
  };

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
    <>
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Type
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Duration
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th scope="col" className="relative px-6 py-3">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {data.map((leave) => (
              <tr key={leave.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">{getLeaveTypeText(leave.leaveType)}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-900">
                    {format(new Date(leave.startDate), 'MMM dd, yyyy')} - {format(new Date(leave.endDate), 'MMM dd, yyyy')}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <LeaveStatusBadge status={leave.status} />
                </td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <div className="flex justify-end gap-2">
                    {Number(leave.status) === LeaveStatus.Pending && (
                      <>
                        <Button variant="ghost" size="icon" onClick={() => setApproveId(leave.id)} title="Approve">
                          <Check className="h-4 w-4 text-green-600" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => setRejectId(leave.id)} title="Reject">
                          <X className="h-4 w-4 text-red-600" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={() => navigate(`/leaves/${leave.id}/edit`)} title="Edit">
                          <Edit2 className="h-4 w-4 text-gray-500" />
                        </Button>
                      </>
                    )}
                    {Number(leave.status) === LeaveStatus.Approved && (
                      <Button variant="ghost" size="icon" onClick={() => setCancelId(leave.id)} title="Cancel">
                        <Slash className="h-4 w-4 text-gray-500" />
                      </Button>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/leaves/${leave.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button 
                      variant="ghost" 
                      size="icon" 
                      onClick={() => setDeleteId(leave.id)}
                    >
                      <Trash2 className="h-4 w-4 text-red-500" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete Leave Request"
        description="Are you sure you want to delete this leave request? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
      
      <ActionDialog
        isOpen={!!approveId}
        onClose={() => setApproveId(null)}
        onConfirm={handleApprove}
        title="Approve Leave Request"
        description="Are you sure you want to approve this leave request?"
        confirmText="Approve"
        variant="default"
        isLoading={approveMutation.isPending}
      />

      <ActionDialog
        isOpen={!!rejectId}
        onClose={() => setRejectId(null)}
        onConfirm={handleReject}
        title="Reject Leave Request"
        description="Are you sure you want to reject this leave request?"
        confirmText="Reject"
        variant="destructive"
        isLoading={rejectMutation.isPending}
      />

      <ActionDialog
        isOpen={!!cancelId}
        onClose={() => setCancelId(null)}
        onConfirm={handleCancel}
        title="Cancel Leave Request"
        description="Are you sure you want to cancel this approved leave?"
        confirmText="Confirm Cancel"
        variant="destructive"
        isLoading={cancelMutation.isPending}
      />
    </>
  );
}
