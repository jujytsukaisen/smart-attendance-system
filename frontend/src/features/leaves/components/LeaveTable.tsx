import { useState } from 'react';
import { format } from 'date-fns';
import { arSA } from 'date-fns/locale';
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
import { LeaveType } from '../types';
import { toast } from 'sonner';
import { useTranslation } from '@/shared/i18n';

interface LeaveTableProps {
  data: LeaveListDto[];
  isEmployeeView?: boolean;
}

export function LeaveTable({ data, isEmployeeView }: LeaveTableProps) {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
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
        toast.success(isRtl ? 'تم حذف طلب الإجازة بنجاح' : 'Leave request deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error(isRtl ? 'فشل حذف طلب الإجازة' : 'Failed to delete leave request');
        setDeleteId(null);
      },
    });
  };

  const handleApprove = () => {
    if (!approveId) return;
    approveMutation.mutate({ leaveId: approveId, comment: '' }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم قبول طلب الإجازة بنجاح' : 'Leave request approved');
        setApproveId(null);
      },
      onError: () => {
        toast.error(isRtl ? 'فشل قبول طلب الإجازة' : 'Failed to approve leave request');
        setApproveId(null);
      },
    });
  };

  const handleReject = () => {
    if (!rejectId) return;
    rejectMutation.mutate({ leaveId: rejectId, reason: 'Rejected by manager' }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم رفض طلب الإجازة' : 'Leave request rejected');
        setRejectId(null);
      },
      onError: () => {
        toast.error(isRtl ? 'فشل رفض طلب الإجازة' : 'Failed to reject leave request');
        setRejectId(null);
      },
    });
  };

  const handleCancel = () => {
    if (!cancelId) return;
    cancelMutation.mutate({ leaveId: cancelId, reason: 'Cancelled by user' }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم إلغاء طلب الإجازة' : 'Leave request cancelled');
        setCancelId(null);
      },
      onError: () => {
        toast.error(isRtl ? 'فشل إلغاء طلب الإجازة' : 'Failed to cancel leave request');
        setCancelId(null);
      },
    });
  };

  const isPendingStatus = (status: string | number) => {
    const s = String(status || '').trim().toLowerCase();
    return s === 'pending' || s === '1' || s === '';
  };

  const isApprovedStatus = (status: string | number) => {
    const s = String(status || '').trim().toLowerCase();
    return s === 'approved' || s === '2';
  };

  const getLeaveTypeText = (type: string | number) => {
    switch (Number(type)) {
      case LeaveType.Annual: return t('leaves.annual');
      case LeaveType.Sick: return t('leaves.sick');
      case LeaveType.Maternity: return t('leaves.maternity');
      case LeaveType.Paternity: return t('leaves.paternity');
      case LeaveType.Unpaid: return t('leaves.unpaid');
      case LeaveType.Other: return t('leaves.other');
      default: return type;
    }
  };

  const formatDateValue = (d: string) => {
    try {
      return format(new Date(d), 'MMM dd, yyyy', isRtl ? { locale: arSA } : undefined);
    } catch {
      return d;
    }
  };

  return (
    <>
      <div className="bg-card text-card-foreground shadow-sm border border-border overflow-x-auto sm:rounded-lg">
        <table className="min-w-full divide-y divide-border">
          <thead className="bg-muted/50">
            <tr>
              {!isEmployeeView && (
                <th scope="col" className="px-6 py-3 text-start text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  {t('attendance.employee')}
                </th>
              )}
              <th scope="col" className="px-6 py-3 text-start text-xs font-medium text-muted-foreground uppercase tracking-wider">
                {t('leaves.leaveType')}
              </th>
              <th scope="col" className="px-6 py-3 text-start text-xs font-medium text-muted-foreground uppercase tracking-wider">
                {t('leaves.duration')}
              </th>
              <th scope="col" className="px-6 py-3 text-start text-xs font-medium text-muted-foreground uppercase tracking-wider">
                {t('common.status')}
              </th>
              <th scope="col" className="px-6 py-3 text-end text-xs font-medium text-muted-foreground uppercase tracking-wider">
                {t('common.actions')}
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {data.map((leave) => {
              const pending = isPendingStatus(leave.status);
              const approved = isApprovedStatus(leave.status);

              return (
                <tr key={leave.id} className="hover:bg-muted/30 transition-colors">
                  {!isEmployeeView && (
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-semibold text-foreground">
                        {leave.employeeName || (isRtl ? 'موظف غير معروف' : '—')}
                      </div>
                      {leave.employeeNumber && (
                        <div className="text-xs font-mono text-muted-foreground">
                          {leave.employeeNumber}
                        </div>
                      )}
                    </td>
                  )}
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-foreground">{getLeaveTypeText(leave.leaveType)}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-muted-foreground">
                      {formatDateValue(leave.startDate)} - {formatDateValue(leave.endDate)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <LeaveStatusBadge status={leave.status} />
                  </td>
                  <td className="relative whitespace-nowrap py-4 px-6 text-end text-sm font-medium">
                    <div className="flex justify-end items-center gap-1.5">
                      {pending && (
                        <>
                          {!isEmployeeView && (
                            <>
                              <Button 
                                variant="outline" 
                                size="sm" 
                                className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30 hover:bg-emerald-500/25 gap-1 px-2.5 h-8 font-medium shadow-sm"
                                onClick={() => setApproveId(leave.id)} 
                                title={t('leaves.approve')}
                              >
                                <Check className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                                <span>{t('leaves.approve')}</span>
                              </Button>
                              <Button 
                                variant="outline" 
                                size="sm" 
                                className="bg-rose-500/15 text-rose-700 dark:text-rose-400 border-rose-500/30 hover:bg-rose-500/25 gap-1 px-2.5 h-8 font-medium shadow-sm"
                                onClick={() => setRejectId(leave.id)} 
                                title={t('leaves.reject')}
                              >
                                <X className="h-4 w-4 text-rose-600 dark:text-rose-400" />
                                <span>{t('leaves.reject')}</span>
                              </Button>
                            </>
                          )}
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-8 w-8"
                            onClick={() => navigate(`/leaves/${leave.id}/edit`)} 
                            title={t('common.edit')}
                          >
                            <Edit2 className="h-4 w-4 text-muted-foreground" />
                          </Button>
                        </>
                      )}
                      {approved && !isEmployeeView && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          className="bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-500/30 hover:bg-amber-500/25 h-8 gap-1 px-2 text-xs"
                          onClick={() => setCancelId(leave.id)} 
                          title={t('leaves.cancel')}
                        >
                          <Slash className="h-3.5 w-3.5" />
                          <span>{t('leaves.cancel')}</span>
                        </Button>
                      )}
                      <Button 
                        variant="ghost" 
                        size="icon" 
                        className="h-8 w-8 text-muted-foreground hover:text-foreground"
                        onClick={() => navigate(`/leaves/${leave.id}`)}
                        title={t('common.details')}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      {pending && (
                        <Button 
                          variant="ghost" 
                          size="icon" 
                          className="h-8 w-8 text-destructive hover:text-destructive hover:bg-destructive/10"
                          onClick={() => setDeleteId(leave.id)}
                          title={t('common.delete')}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title={isRtl ? 'حذف طلب الإجازة' : 'Delete Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في حذف طلب الإجازة هذا؟ لا يمكن التراجع عن هذا الإجراء.' : 'Are you sure you want to delete this leave request? This action cannot be undone.'}
        confirmText={t('common.delete')}
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
      
      <ActionDialog
        isOpen={!!approveId}
        onClose={() => setApproveId(null)}
        onConfirm={handleApprove}
        title={isRtl ? 'قبول طلب الإجازة' : 'Approve Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في قبول واعتماد هذا الطلب؟' : 'Are you sure you want to approve this leave request?'}
        confirmText={isRtl ? 'تأكيد القبول' : 'Approve'}
        variant="default"
        isLoading={approveMutation.isPending}
      />

      <ActionDialog
        isOpen={!!rejectId}
        onClose={() => setRejectId(null)}
        onConfirm={handleReject}
        title={isRtl ? 'رفض طلب الإجازة' : 'Reject Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في رفض طلب الإجازة هذا؟' : 'Are you sure you want to reject this leave request?'}
        confirmText={isRtl ? 'تأكيد الرفض' : 'Reject'}
        variant="destructive"
        isLoading={rejectMutation.isPending}
      />

      <ActionDialog
        isOpen={!!cancelId}
        onClose={() => setCancelId(null)}
        onConfirm={handleCancel}
        title={isRtl ? 'إلغاء طلب الإجازة' : 'Cancel Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في إلغاء هذه الإجازة المقبولة؟' : 'Are you sure you want to cancel this approved leave?'}
        confirmText={isRtl ? 'تأكيد الإلغاء' : 'Confirm Cancel'}
        variant="destructive"
        isLoading={cancelMutation.isPending}
      />
    </>
  );
}
