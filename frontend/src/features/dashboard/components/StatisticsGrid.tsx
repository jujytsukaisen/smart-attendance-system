import { Users, Building2, MapPin, Network, Clock, AlertTriangle, CalendarOff } from 'lucide-react';
import { DashboardCard } from './DashboardCard';
import type { DashboardStats } from '../types';

interface StatisticsGridProps {
  stats: DashboardStats;
}

export const StatisticsGrid = ({ stats }: StatisticsGridProps) => {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <DashboardCard
        title="Total Employees"
        value={stats.totalEmployees}
        icon={<Users className="h-4 w-4" />}
      />
      <DashboardCard
        title="Today's Attendance"
        value={stats.todaysAttendance}
        icon={<Clock className="h-4 w-4" />}
        description={`${Math.round((stats.todaysAttendance / stats.totalEmployees) * 100)}% of workforce`}
      />
      <DashboardCard
        title="Late Employees"
        value={stats.lateEmployees}
        icon={<AlertTriangle className="h-4 w-4 text-amber-500" />}
      />
      <DashboardCard
        title="Active Leaves"
        value={stats.activeLeaves}
        icon={<CalendarOff className="h-4 w-4" />}
      />
      
      {/* Structural Stats */}
      <DashboardCard
        title="Companies"
        value={stats.totalCompanies}
        icon={<Building2 className="h-4 w-4" />}
      />
      <DashboardCard
        title="Branches"
        value={stats.totalBranches}
        icon={<MapPin className="h-4 w-4" />}
      />
      <DashboardCard
        title="Departments"
        value={stats.totalDepartments}
        icon={<Network className="h-4 w-4" />}
      />
    </div>
  );
};
