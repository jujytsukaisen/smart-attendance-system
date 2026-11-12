import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { CalendarClock, CheckCircle, XCircle, Users } from 'lucide-react';
import { useTranslation } from '@/shared/i18n';

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
  const { t } = useTranslation();

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-6">
      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-muted-foreground">{t('leaves.pendingLeaves')}</CardTitle>
          <CalendarClock className="w-4 h-4 text-amber-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-foreground">{pendingCount}</div>
        </CardContent>
      </Card>
      
      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-muted-foreground">{t('leaves.approvedLeaves')}</CardTitle>
          <CheckCircle className="w-4 h-4 text-emerald-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-foreground">{approvedCount}</div>
        </CardContent>
      </Card>

      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-muted-foreground">{t('leaves.rejectedLeaves')}</CardTitle>
          <XCircle className="w-4 h-4 text-rose-500" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-foreground">{rejectedCount}</div>
        </CardContent>
      </Card>

      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
          <CardTitle className="text-sm font-medium text-muted-foreground">{t('leaves.onLeaveToday')}</CardTitle>
          <Users className="w-4 h-4 text-primary" />
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold text-foreground">{onLeaveTodayCount}</div>
        </CardContent>
      </Card>
    </div>
  );
}
