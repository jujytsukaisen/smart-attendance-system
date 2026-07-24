export interface DashboardStats {
  totalEmployees: number;
  totalCompanies: number;
  totalBranches: number;
  totalDepartments: number;
  todaysAttendance: number;
  lateEmployees: number;
  activeLeaves: number;
}

export interface RecentActivity {
  id: string;
  type: 'attendance' | 'leave' | 'system';
  message: string;
  timestamp: string;
}

export interface Notification {
  id: string;
  title: string;
  description: string;
  read: boolean;
  timestamp: string;
}

export interface DashboardData {
  stats: DashboardStats;
  recentActivities: RecentActivity[];
  notifications: Notification[];
}
