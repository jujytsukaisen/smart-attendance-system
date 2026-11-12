import {
  Clock,
  CalendarOff,
  Bell,
  CheckCircle2,
  XCircle,
  AlertCircle
} from 'lucide-react';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';

import { Skeleton } from '@/shared/components/ui/skeleton';
import { useAuthStore } from '@/features/auth';
import { useMyDashboard } from '../hooks/useDashboard';
import { DashboardCard } from '../components/DashboardCard';
import { useTranslation } from '@/shared/i18n';

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const DAY_NAMES_AR = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];

// Custom circular progress for employee
const CircularProgress = ({ rate, label }: { rate: number, label: string }) => {
  const radius = 60;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (rate / 100) * circumference;

  return (
    <div className="relative flex flex-col items-center justify-center">
      <svg className="transform -rotate-90 w-40 h-40">
        <circle cx="80" cy="80" r={radius} stroke="currentColor" strokeWidth="12" fill="transparent" className="text-muted/20" />
        <circle
          cx="80" cy="80" r={radius} stroke="currentColor" strokeWidth="12" fill="transparent"
          strokeDasharray={circumference} strokeDashoffset={strokeDashoffset}
          strokeLinecap="round"
          className="text-primary transition-all duration-1000 ease-in-out"
        />
      </svg>
      <div className="absolute flex flex-col items-center justify-center text-center">
        <span className="text-3xl font-bold text-foreground">{rate}%</span>
        <span className="text-sm text-muted-foreground">{label}</span>
      </div>
    </div>
  );
};

export const MyDashboard = () => {
  const { user } = useAuthStore();
  const { t, isRtl } = useTranslation();
  const { data, isLoading, isError, error } = useMyDashboard();

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <Skeleton className="h-8 w-48 mb-2" />
          <Skeleton className="h-4 w-72" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-lg" />
          ))}
        </div>
        <div className="grid gap-4 lg:grid-cols-3">
          <Skeleton className="h-80 rounded-lg lg:col-span-2" />
          <Skeleton className="h-80 rounded-lg" />
        </div>
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="flex h-full w-full flex-col items-center justify-center p-8 text-center">
        <h2 className="text-2xl font-semibold mb-2">{t('common.error')}</h2>
        <p className="text-muted-foreground whitespace-pre-wrap text-left text-xs bg-muted p-4 rounded-md overflow-auto max-w-full">
          {error ? JSON.stringify(error, Object.getOwnPropertyNames(error), 2) : (isRtl ? 'حدث خطأ غير متوقع أثناء تحميل البيانات.' : 'An unknown error occurred.')}
        </p>
      </div>
    );
  }

  const chartData = data.last7Days.map((d) => {
    const dayIndex = new Date(d.date).getDay();
    const isPresent = d.status.toLowerCase() === 'present' ? 1 : 0;
    const isLate = d.status.toLowerCase() === 'late' ? 1 : 0;
    const isAbsent = d.status.toLowerCase() === 'absent' ? 1 : 0;

    return {
      day: isRtl ? DAY_NAMES_AR[dayIndex] : DAY_NAMES[dayIndex],
      Present: isPresent,
      Late: isLate,
      Absent: isAbsent,
    };
  });

  const getStatusIcon = (status: string) => {
    const s = (status || '').toLowerCase();
    if (s === 'present') return <CheckCircle2 className="h-5 w-5 text-emerald-500" />;
    if (s === 'late') return <AlertCircle className="h-5 w-5 text-orange-500" />;
    if (s === 'absent') return <XCircle className="h-5 w-5 text-rose-500" />;
    return <Clock className="h-5 w-5 text-muted-foreground" />;
  };

  const getStatusLabel = (status: string) => {
    const s = (status || '').toLowerCase();
    if (s === 'present') return t('attendance.present');
    if (s === 'late') return t('attendance.late');
    if (s === 'absent') return t('attendance.absent');
    if (s === 'onleave') return t('dashboard.onLeave');
    if (s === 'notcheckedin') return isRtl ? 'لم يتم تسجيل الحضور' : 'Not Checked In';
    return status;
  };

  const getLeaveTypeText = (type: string | number) => {
    const s = String(type || '').toLowerCase();
    if (s === 'sick' || s === '1') return isRtl ? 'إجازة مرضية' : 'Sick Leave';
    if (s === 'annual' || s === '0') return isRtl ? 'إجازة سنوية' : 'Annual Leave';
    if (s === 'unpaid' || s === '2') return isRtl ? 'إجازة بدون راتب' : 'Unpaid Leave';
    if (s === 'maternity' || s === '3') return isRtl ? 'إجازة أمومة' : 'Maternity Leave';
    if (s === 'paternity' || s === '4') return isRtl ? 'إجازة أبوة' : 'Paternity Leave';
    if (s === 'other' || s === '5') return isRtl ? 'إجازة أخرى' : 'Other Leave';
    return String(type);
  };

  const getLeaveStatusBadge = (status: string | number) => {
    const s = String(status || '').toLowerCase();
    if (s === 'approved' || s === '2') {
      return (
        <Badge variant="outline" className="bg-emerald-500/10 text-emerald-500 border-emerald-500/20">
          {isRtl ? 'مقبولة' : 'Approved'}
        </Badge>
      );
    }
    if (s === 'pending' || s === '1') {
      return (
        <Badge variant="outline" className="bg-amber-500/10 text-amber-500 border-amber-500/20">
          {isRtl ? 'قيد الانتظار' : 'Pending'}
        </Badge>
      );
    }
    if (s === 'rejected' || s === '3') {
      return (
        <Badge variant="outline" className="bg-rose-500/10 text-rose-500 border-rose-500/20">
          {isRtl ? 'مرفوضة' : 'Rejected'}
        </Badge>
      );
    }
    return <Badge variant="outline">{String(status)}</Badge>;
  };

  const formatNotificationTime = (dateStr: string) => {
    try {
      if (!dateStr) return '';
      const normalized = dateStr.includes('T') ? dateStr : dateStr.replace(' ', 'T');
      const hasZone = normalized.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(normalized);
      const d = new Date(hasZone ? normalized : normalized + 'Z');
      if (isNaN(d.getTime())) {
        const fallbackDate = new Date(dateStr);
        if (!isNaN(fallbackDate.getTime())) {
          return fallbackDate.toLocaleString(isRtl ? 'ar-SA' : 'en-US');
        }
        return dateStr;
      }
      return d.toLocaleString(isRtl ? 'ar-SA' : 'en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateStr;
    }
  };

  const formatNotificationMessage = (msg: string) => {
    if (!isRtl || !msg) return msg;
    const approvedMatch = msg.match(/Leave approved for (.+?) \((.+?)\): (.+?)\.?$/i);
    if (approvedMatch) {
      const [, name, type, dates] = approvedMatch;
      const typeAr = getLeaveTypeText(type);
      return `تمت الموافقة على ${typeAr} للموظف (${name}): ${dates}`;
    }
    const requestedMatch = msg.match(/Leave request submitted for (.+?) \((.+?)\): (.+?)\.?$/i);
    if (requestedMatch) {
      const [, name, type, dates] = requestedMatch;
      const typeAr = getLeaveTypeText(type);
      return `تم تقديم طلب ${typeAr} للموظف (${name}): ${dates}`;
    }
    const rejectedMatch = msg.match(/Leave rejected for (.+?) \((.+?)\): (.+?)\.?$/i);
    if (rejectedMatch) {
      const [, name, type, dates] = rejectedMatch;
      const typeAr = getLeaveTypeText(type);
      return `تم رفض طلب ${typeAr} للموظف (${name}): ${dates}`;
    }
    return msg;
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight text-foreground">
          {t('dashboard.welcomeBack')}, {user?.firstName || user?.email || ''}!
        </h2>
        <p className="text-muted-foreground mt-1">
          {isRtl ? 'إليك نظرة عامة على حضورك وإجازاتك.' : 'Here is an overview of your attendance and leaves.'}
        </p>
      </div>

      {/* Stat Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardContent className="p-6">
            <div className="flex justify-between items-start">
              <div>
                <p className="text-sm font-medium text-muted-foreground mb-1">{isRtl ? 'حالة اليوم' : 'Today\'s Status'}</p>
                <h3 className="text-2xl font-bold">{getStatusLabel(data.todayStatus)}</h3>
                {data.checkInTime && (
                  <p className="text-sm text-muted-foreground mt-1">
                    {isRtl ? 'دخول:' : 'In:'} {new Date(data.checkInTime + 'Z').toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </p>
                )}
              </div>
              <div className="p-2 bg-primary/10 rounded-full text-primary">
                {getStatusIcon(data.todayStatus)}
              </div>
            </div>
          </CardContent>
        </Card>
        
        <DashboardCard
          title={isRtl ? 'أيام الحضور (هذا الشهر)' : 'Present Days (This Month)'}
          value={data.presentDaysThisMonth}
          icon={<CheckCircle2 className="h-4 w-4 text-emerald-500" />}
        />
        
        <DashboardCard
          title={isRtl ? 'أيام التأخير (هذا الشهر)' : 'Late Days (This Month)'}
          value={data.lateDaysThisMonth}
          icon={<AlertCircle className="h-4 w-4 text-orange-500" />}
        />

        <DashboardCard
          title={isRtl ? 'رصيد الإجازات المتبقي' : 'Leave Balance'}
          value={data.totalLeavesTaken > 0 ? `-${data.totalLeavesTaken}` : '0'} // Placeholder logic for leave balance, ideally comes from backend 
          icon={<CalendarOff className="h-4 w-4 text-purple-500" />}
        />
      </div>

      {/* Charts row */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2 border-border bg-card text-card-foreground shadow-sm">
          <CardHeader>
            <CardTitle>{isRtl ? 'حضوري آخر 7 أيام' : 'My Attendance (Last 7 Days)'}</CardTitle>
          </CardHeader>
          <CardContent>
            {chartData.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-[280px] text-muted-foreground border-2 border-dashed border-border/50 rounded-lg bg-muted/20">
                <CalendarOff className="h-8 w-8 mb-2 opacity-50" />
                <p className="text-sm font-medium">{isRtl ? 'لا توجد بيانات حضور لآخر 7 أيام' : 'No attendance data for the last 7 days'}</p>
              </div>
            ) : (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
                  <XAxis dataKey="day" tick={{ fontSize: 12 }} />
                  <YAxis tick={{ fontSize: 12 }} allowDecimals={false} tickFormatter={(val) => val === 1 ? 'Yes' : 'No'} />
                  <Tooltip 
                    contentStyle={{ backgroundColor: 'hsl(var(--card))', borderColor: 'hsl(var(--border))', color: 'hsl(var(--foreground))', borderRadius: '0.5rem' }}
                  />
                  <Legend />
                  <Bar dataKey="Present" name={t('attendance.present')} fill="#22c55e" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="Late" name={t('attendance.late')} fill="#f59e0b" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="Absent" name={t('attendance.absent')} fill="#ef4444" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card className="flex flex-col items-center justify-center border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="w-full pb-2">
            <CardTitle className="text-center">{isRtl ? 'معدل الحضور (هذا الشهر)' : 'Attendance Rate (This Month)'}</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-1 items-center justify-center pt-2 w-full">
            <CircularProgress rate={data.attendanceRateThisMonth} label={t('dashboard.attendanceRate')} />
          </CardContent>
        </Card>
      </div>

      {/* Upcoming Leaves + Notifications */}
      <div className="grid gap-4 lg:grid-cols-2">
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader>
            <CardTitle>{t('dashboard.upcomingLeaves', isRtl ? 'الإجازات القادمة' : 'Upcoming Leaves')}</CardTitle>
          </CardHeader>
          <CardContent>
            {data.upcomingLeaves.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-8">
                {t('common.noData')}
              </p>
            ) : (
              <div className="space-y-4">
                {data.upcomingLeaves.map((l, idx) => (
                  <div key={idx} className="flex items-center justify-between border-b border-border pb-4 last:border-0 last:pb-0">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-full bg-purple-500/10 text-purple-500">
                        <CalendarOff className="h-4 w-4" />
                      </div>
                      <div>
                        <p className="font-medium text-sm text-foreground">{getLeaveTypeText(l.leaveType)}</p>
                        <p className="text-xs text-muted-foreground">
                          {new Date(l.from).toLocaleDateString(isRtl ? 'ar-SA' : 'en-US')} - {new Date(l.to).toLocaleDateString(isRtl ? 'ar-SA' : 'en-US')}
                        </p>
                      </div>
                    </div>
                    {getLeaveStatusBadge(l.status)}
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle>{t('dashboard.notifications', isRtl ? 'الإشعارات' : 'Notifications')}</CardTitle>
            <Bell className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {data.notifications.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-8">
                {isRtl ? 'لا توجد إشعارات' : 'No notifications'}
              </p>
            ) : (
              <div className="space-y-4">
                {data.notifications.map((n, idx) => (
                  <div key={idx} className="flex gap-3 items-start border-b border-border pb-4 last:border-0 last:pb-0">
                    <div className="mt-0.5 w-2 h-2 rounded-full bg-primary flex-shrink-0" />
                    <div>
                      <p className="text-sm text-foreground">{formatNotificationMessage(n.message)}</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {formatNotificationTime(n.createdAt)}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
