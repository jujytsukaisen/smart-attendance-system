import { format } from 'date-fns';
import { arSA } from 'date-fns/locale';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { LeaveStatusBadge } from './LeaveStatusBadge';
import type { LeaveDetailsDto } from '../types';
import { LeaveType } from '../types';
import { useTranslation } from '@/shared/i18n';

interface LeaveDetailsCardProps {
  leave: LeaveDetailsDto;
}

export function LeaveDetailsCard({ leave }: LeaveDetailsCardProps) {
  const { t, isRtl } = useTranslation();

  const getLeaveTypeText = (type: string | number) => {
    switch (Number(type)) {
      case LeaveType.Annual: return t('leaves.annual');
      case LeaveType.Sick: return t('leaves.sick');
      case LeaveType.Maternity: return t('leaves.maternity');
      case LeaveType.Paternity: return t('leaves.paternity');
      case LeaveType.Unpaid: return t('leaves.unpaid');
      case LeaveType.Other: return t('leaves.other');
      default: return String(type);
    }
  };

  const formatDateStr = (dateStr: string) => {
    try {
      return format(new Date(dateStr), 'PPP', { locale: isRtl ? arSA : undefined });
    } catch {
      return dateStr;
    }
  };

  const durationDays = Math.ceil((new Date(leave.endDate).getTime() - new Date(leave.startDate).getTime()) / (1000 * 3600 * 24)) + 1;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="border-b border-border/50 pb-3">
          <CardTitle className="text-base font-semibold text-foreground">
            {isRtl ? 'معلومات الإجازة' : 'Leave Information'}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4 pt-4">
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{t('attendance.employee')}</span>
            <span className="block text-base font-semibold text-foreground mt-1">
              {leave.employeeName || '—'}
              {leave.employeeNumber && <span className="text-sm font-normal text-muted-foreground mx-2 font-mono">({leave.employeeNumber})</span>}
            </span>
          </div>
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{t('leaves.leaveType')}</span>
            <span className="block text-sm font-medium text-foreground mt-1">{getLeaveTypeText(leave.leaveType)}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{isRtl ? 'الفترة الزمنية' : 'Date Range'}</span>
            <span className="block text-sm font-medium text-foreground mt-1">
              {formatDateStr(leave.startDate)} {isRtl ? 'إلى' : 'to'} {formatDateStr(leave.endDate)}
            </span>
          </div>
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{t('leaves.duration')}</span>
            <span className="block text-sm font-medium text-foreground mt-1">
              {durationDays} {isRtl ? (durationDays === 1 ? 'يوم' : durationDays === 2 ? 'يومان' : 'أيام') : (durationDays === 1 ? 'day' : 'days')}
            </span>
          </div>
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{t('common.status')}</span>
            <div className="mt-1">
              <LeaveStatusBadge status={leave.status} />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="border-b border-border/50 pb-3">
          <CardTitle className="text-base font-semibold text-foreground">
            {isRtl ? 'التفاصيل والملاحظات' : 'Details & Comments'}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-6 pt-4">
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{t('leaves.reason')}</span>
            <div className="mt-2 text-sm text-foreground bg-muted/40 p-4 rounded-md border border-border min-h-24 whitespace-pre-wrap">
              {leave.reason || (isRtl ? 'لا يوجد سبب محدد.' : 'No reason provided.')}
            </div>
          </div>
          
          <div>
            <span className="block text-sm font-medium text-muted-foreground">{isRtl ? 'ملاحظات المدير' : "Manager's Comment"}</span>
            <div className="mt-2 text-sm text-foreground bg-primary/5 p-4 rounded-md border border-primary/20 min-h-24 whitespace-pre-wrap">
              {leave.managerComment || (isRtl ? 'لا توجد ملاحظات من المدير بعد.' : 'No comments from manager yet.')}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
