// Types matching the backend specification

export interface DailyAttendanceReportDto {
  present: number;
  absent: number;
  late: number;
  onLeave: number;
  earlyLeave: number;
  totalEmployees: number;
}

export interface MonthlyAttendanceReportDto {
  workingDays: number;
  attendanceRate: number;
  totalLate: number;
  totalAbsent: number;
  totalOvertime: number;
}

// These are mocked locally based on UI specifications since the backend doesn't implement them yet.

export interface AttendanceTrendData {
  date: string;
  present: number;
  absent: number;
  late: number;
  onLeave: number;
}

export interface LateEmployeesReportDto {
  id: string;
  employeeName: string;
  department: string;
  totalLateMinutes: number;
  lateDaysCount: number;
}

export interface AbsenceReportDto {
  id: string;
  employeeName: string;
  department: string;
  absentDaysCount: number;
  reason?: string;
}

export interface LeaveReportDto {
  id: string;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  status: string;
  totalDays: number;
}

export interface OvertimeReportDto {
  id: string;
  employeeName: string;
  date: string;
  overtimeMinutes: number;
  approved: boolean;
}

export interface DashboardStatisticsDto {
  totalEmployees: number;
  presentToday: number;
  absentToday: number;
  lateToday: number;
  onLeaveToday: number;
  averageAttendanceRate: number; // percentage
  weeklyTrend: AttendanceTrendData[];
  leaveDistribution: { name: string; value: number }[];
  departmentAttendance: { name: string; present: number; absent: number; late: number }[];
}

export interface ReportFilterDto {
  startDate?: string;
  endDate?: string;
  companyId?: string;
  branchId?: string;
  departmentId?: string;
  employeeId?: string;
  status?: string;
  searchTerm?: string;
}
