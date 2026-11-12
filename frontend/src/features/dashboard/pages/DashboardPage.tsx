import {
  Users,
  UserCheck,
  UserX,
  Clock,
  Coffee,
  CalendarOff,
  MonitorSmartphone,
  Bell,
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
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from '@/shared/components/ui/table';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useAuthStore } from '@/features/auth';
import { useDashboard } from '../hooks/useDashboard';
import { DashboardCard } from '../components/DashboardCard';
import { useTranslation } from '@/shared/i18n';
import { MyDashboard } from './MyDashboard';

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const DAY_NAMES_AR = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
const PIE_COLORS = ['#6366f1', '#22c55e', '#f59e0b', '#ef4444', '#06b6d4', '#a855f7', '#ec4899'];

export const DashboardPage = () => {
  const { user } = useAuthStore();
  const { t, isRtl } = useTranslation();
  
  const isAdmin = user?.roles?.includes('Admin');
  const isEmployeeOnly = !isAdmin;

  const { data, isLoading, isError, error } = useDashboard(!isEmployeeOnly);

  if (isEmployeeOnly) {
    return <MyDashboard />;
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <Skeleton className="h-8 w-48 mb-2" />
          <Skeleton className="h-4 w-72" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-lg" />
          ))}
        </div>
        <div className="grid gap-4 lg:grid-cols-3">
          <Skeleton className="h-80 rounded-lg lg:col-span-2" />
          <Skeleton className="h-80 rounded-lg" />
        </div>
        <div className="grid gap-4 lg:grid-cols-3">
          <Skeleton className="h-64 rounded-lg" />
          <Skeleton className="h-64 rounded-lg lg:col-span-2" />
        </div>
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="flex h-full w-full flex-col items-center justify-center p-8 text-center">
        <h2 className="text-2xl font-semibold mb-2">{t('common.error')}</h2>
        <p className="text-muted-foreground">
          {(error as Error)?.message || (isRtl ? 'حدث خطأ غير متوقع أثناء تحميل البيانات.' : 'An unknown error occurred.')}
        </p>
      </div>
    );
  }

  const chartData = data.attendanceLast7Days.map((d) => {
    const dayIndex = new Date(d.date).getDay();
    return {
      day: isRtl ? DAY_NAMES_AR[dayIndex] : DAY_NAMES[dayIndex],
      Present: d.present,
      Absent: d.absent,
      Late: d.late,
    };
  });

  const getStatusLabel = (status: string) => {
    const s = (status || '').toLowerCase();
    if (s === 'present') return t('attendance.present');
    if (s === 'late') return t('attendance.late');
    if (s === 'absent') return t('attendance.absent');
    if (s === 'leave') return t('dashboard.onLeave');
    return status;
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight text-foreground">
          {t('dashboard.welcomeBack')}, {user?.firstName || user?.email || ''}!
        </h2>
        <p className="text-muted-foreground mt-1">
          {t('dashboard.subtitle')}
        </p>
      </div>

      {/* Stat Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-7">
        <DashboardCard
          title={t('dashboard.totalEmployees')}
          value={data.totalEmployees}
          icon={<Users className="h-4 w-4 text-primary" />}
        />
        <DashboardCard
          title={t('dashboard.presentToday')}
          value={data.presentToday}
          icon={<UserCheck className="h-4 w-4 text-emerald-500" />}
        />
        <DashboardCard
          title={isRtl ? 'في استراحة' : 'On Break'}
          value={data.onBreakToday ?? 0}
          icon={<Coffee className="h-4 w-4 text-amber-500" />}
        />
        <DashboardCard
          title={t('dashboard.absentToday')}
          value={data.absentToday}
          icon={<UserX className="h-4 w-4 text-rose-500" />}
        />
        <DashboardCard
          title={t('attendance.late')}
          value={data.lateToday}
          icon={<Clock className="h-4 w-4 text-orange-500" />}
        />
        <DashboardCard
          title={t('dashboard.onLeave')}
          value={data.employeesOnLeave}
          icon={<CalendarOff className="h-4 w-4 text-purple-500" />}
        />
        <DashboardCard
          title={isRtl ? 'الأجهزة المتصلة' : 'Active Devices'}
          value={data.activeDevices}
          icon={<MonitorSmartphone className="h-4 w-4 text-cyan-500" />}
        />
      </div>


      {/* Charts row */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2 border-border bg-card text-card-foreground shadow-sm">
          <CardHeader>
            <CardTitle>{t('dashboard.weeklyAttendance')}</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
                <XAxis dataKey="day" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
                <Tooltip 
                  contentStyle={{ backgroundColor: 'hsl(var(--card))', borderColor: 'hsl(var(--border))', color: 'hsl(var(--foreground))', borderRadius: '0.5rem' }}
                />
                <Legend />
                <Bar dataKey="Present" name={t('attendance.present')} fill="#22c55e" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Late" name={t('attendance.late')} fill="#f59e0b" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Absent" name={t('attendance.absent')} fill="#ef4444" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="flex flex-col items-center justify-center border-border bg-card text-card-foreground shadow-sm">
          <CardHeader className="w-full pb-2">
            <CardTitle className="text-center">{t('dashboard.attendanceRate')}</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-1 items-center justify-center pt-2">
            <AttendanceRateCircle rate={data.attendanceRate} />
          </CardContent>
        </Card>
      </div>

      {/* Department stats + Notifications */}
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="border-border bg-card text-card-foreground shadow-sm">
          <CardHeader>
            <CardTitle>{t('nav.departments')}</CardTitle>
          </CardHeader>
          <CardContent>
            {data.departmentStatistics.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-8">
                {t('common.noData')}
              </p>
            ) : (
              <ResponsiveContainer width="100%" height={220}>
                <PieChart>
                  <Pie
                    data={data.departmentStatistics}
                    dataKey="count"
                    nameKey="department"
                    cx="50%"
                    cy="50%"
                    outerRadius={75}
                  >
                    {data.departmentStatistics.map((_, index) => (
                      <Cell key={index} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip 
                    contentStyle={{ backgroundColor: 'hsl(var(--card))', borderColor: 'hsl(var(--border))', color: 'hsl(var(--foreground))', borderRadius: '0.5rem' }}
                  />
                  <Legend verticalAlign="bottom" height={36} iconType="circle" />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card className="lg:col-span-2 border-border bg-card text-card-foreground shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Bell className="h-4 w-4 text-primary" /> {t('nav.notifications')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {data.notifications.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-8">
                {isRtl ? 'لا توجد إشعارات حالياً' : 'No recent notifications.'}
              </p>
            ) : (
              <div className="space-y-3">
                {data.notifications.map((n) => (
                  <div key={n.id} className="flex items-center justify-between text-sm py-1 border-b border-border/50 last:border-0">
                    <span className="font-medium text-foreground">{n.message}</span>
                    <span className="text-xs text-muted-foreground font-mono">
                      {new Date(n.createdAt).toLocaleString(isRtl ? 'ar-SA' : 'en-US')}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Recent Attendance table */}
      <Card className="border-border bg-card text-card-foreground shadow-sm">
        <CardHeader>
          <CardTitle>{t('dashboard.recentActivity')}</CardTitle>
        </CardHeader>
        <CardContent>
          {data.recentAttendance.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-8">
              {isRtl ? 'لا توجد سجلات حضور لليوم بعد.' : 'No attendance records for today yet.'}
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="text-start">{t('attendance.employee')}</TableHead>
                  <TableHead className="text-start">{t('employees.department')}</TableHead>
                  <TableHead className="text-start">{t('attendance.checkIn')}</TableHead>
                  <TableHead className="text-start">{t('attendance.checkOut')}</TableHead>
                  <TableHead className="text-start">{t('common.status')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.recentAttendance.map((row) => (
                  <TableRow key={row.id}>
                    <TableCell className="font-medium">{row.employeeName}</TableCell>
                    <TableCell className="text-muted-foreground">{row.department}</TableCell>
                    <TableCell className="font-mono text-xs">
                      {row.checkIn ? new Date(row.checkIn.replace(/Z$/, '')).toLocaleTimeString(isRtl ? 'ar-SA' : 'en-US') : '—'}
                    </TableCell>
                    <TableCell className="font-mono text-xs">
                      {row.checkOut ? new Date(row.checkOut.replace(/Z$/, '')).toLocaleTimeString(isRtl ? 'ar-SA' : 'en-US') : '—'}
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusBadgeVariant(row.status)}>
                        {getStatusLabel(row.status)}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

function statusBadgeVariant(status: string) {
  switch (status?.toLowerCase()) {
    case 'present':
      return 'default';
    case 'late':
      return 'secondary';
    case 'absent':
      return 'destructive';
    default:
      return 'outline';
  }
}

function AttendanceRateCircle({ rate }: { rate: number }) {
  const radius = 54;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (rate / 100) * circumference;

  return (
    <div className="flex flex-col items-center justify-center">
      <svg width="140" height="140" viewBox="0 0 140 140">
        <circle
          cx="70"
          cy="70"
          r={radius}
          fill="none"
          strokeWidth="12"
          className="text-muted stroke-current"
        />
        <circle
          cx="70"
          cy="70"
          r={radius}
          fill="none"
          strokeWidth="12"
          strokeLinecap="round"
          stroke="#22c55e"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          transform="rotate(-90 70 70)"
        />
        <text x="70" y="76" textAnchor="middle" fontSize="26" fontWeight="700" className="fill-foreground font-mono">
          {rate}%
        </text>
      </svg>
    </div>
  );
}

export default DashboardPage;