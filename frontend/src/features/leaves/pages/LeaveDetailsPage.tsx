import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, ArrowRight, Check, X } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog, LeaveDetailsCard } from '../components';
import { useLeaveDetails, useApproveLeave, useRejectLeave } from '../hooks';
import { toast } from 'sonner';
import { useTranslation } from '@/shared/i18n';

export function LeaveDetailsPage() {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const { data, isLoading, refetch } = useLeaveDetails(id!);
  const approveMutation = useApproveLeave();
  const rejectMutation = useRejectLeave();

  const [showApprove, setShowApprove] = useState(false);
  const [showReject, setShowReject] = useState(false);

  if (isLoading) {
    return (
      <div className="space-y-4 max-w-5xl mx-auto">
        <div className="h-10 w-32 bg-muted rounded animate-pulse mb-6" />
        <div className="h-64 bg-muted rounded animate-pulse" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4 text-center">
        <p className="text-destructive font-medium">{isRtl ? 'تعذر تحميل تفاصيل طلب الإجازة' : 'Failed to load leave details.'}</p>
        <Button variant="outline" onClick={() => navigate('/leaves')}>{isRtl ? 'العودة لصفحة الإجازات' : 'Back to Leaves'}</Button>
      </div>
    );
  }

  const isPending = String(data.status || '').toLowerCase() === 'pending' || String(data.status || '') === '1' || !data.status;

  const handleApprove = () => {
    approveMutation.mutate({ leaveId: data.id, comment: '' }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم قبول واعتماد طلب الإجازة بنجاح' : 'Leave request approved successfully');
        setShowApprove(false);
        refetch();
      },
      onError: () => {
        toast.error(isRtl ? 'فشل قبول طلب الإجازة' : 'Failed to approve leave request');
        setShowApprove(false);
      },
    });
  };

  const handleReject = () => {
    rejectMutation.mutate({ leaveId: data.id, reason: 'Rejected by manager' }, {
      onSuccess: () => {
        toast.success(isRtl ? 'تم رفض طلب الإجازة' : 'Leave request rejected');
        setShowReject(false);
        refetch();
      },
      onError: () => {
        toast.error(isRtl ? 'فشل رفض طلب الإجازة' : 'Failed to reject leave request');
        setShowReject(false);
      },
    });
  };

  const BackIcon = isRtl ? ArrowRight : ArrowLeft;

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/leaves')}>
            <BackIcon className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-foreground">
              {isRtl ? 'تفاصيل طلب الإجازة' : 'Leave Details'}
            </h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {isRtl ? 'عرض وإدارة واعتماد تفاصيل طلب الإجازة.' : 'View and manage this leave request.'}
            </p>
          </div>
        </div>

        {isPending && (
          <div className="flex items-center gap-3">
            <Button 
              className="bg-emerald-600 hover:bg-emerald-700 text-white gap-2 shadow-sm"
              onClick={() => setShowApprove(true)}
            >
              <Check className="h-4 w-4" />
              <span>{t('leaves.approve')}</span>
            </Button>
            <Button 
              variant="destructive"
              className="gap-2 shadow-sm"
              onClick={() => setShowReject(true)}
            >
              <X className="h-4 w-4" />
              <span>{t('leaves.reject')}</span>
            </Button>
          </div>
        )}
      </div>

      <LeaveDetailsCard leave={data} />

      <ActionDialog
        isOpen={showApprove}
        onClose={() => setShowApprove(false)}
        onConfirm={handleApprove}
        title={isRtl ? 'الموافقة على طلب الإجازة' : 'Approve Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في الموافقة على طلب الإجازة واعتماده؟' : 'Are you sure you want to approve this leave request?'}
        confirmText={t('leaves.approve')}
        variant="default"
        isLoading={approveMutation.isPending}
      />

      <ActionDialog
        isOpen={showReject}
        onClose={() => setShowReject(false)}
        onConfirm={handleReject}
        title={isRtl ? 'رفض طلب الإجازة' : 'Reject Leave Request'}
        description={isRtl ? 'هل أنت متأكد من رغبتك في رفض طلب الإجازة هذا؟' : 'Are you sure you want to reject this leave request?'}
        confirmText={t('leaves.reject')}
        variant="destructive"
        isLoading={rejectMutation.isPending}
      />
    </div>
  );
}
