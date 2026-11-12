export interface AttendanceDayDto {
    date: string; // "2026-07-20"
    present: number;
    absent: number;
    late: number;
}

export interface DepartmentStatisticDto {
    department: string;
    count: number;
}

export interface RecentAttendanceDto {
    id: string;
    employeeName: string;
    department: string;
    checkIn: string | null;
    checkOut: string | null;
    status: 'Present' | 'Late' | 'Absent' | string;
}

export interface UpcomingLeaveDto {
    id: string;
    employeeName: string;
    leaveType: string;
    from: string; // "2026-07-20"
    to: string;
    status: string;
}

export interface DashboardNotificationDto {
    id: string;
    message: string;
    createdAt: string;
}

export interface DashboardStatisticsDto {
    totalEmployees: number;
    presentToday: number;
    absentToday: number;
    lateToday: number;
    onBreakToday?: number;
    employeesOnLeave: number;
    activeDevices: number;
    attendanceRate: number;
    attendanceLast7Days: AttendanceDayDto[];
    departmentStatistics: DepartmentStatisticDto[];
    recentAttendance: RecentAttendanceDto[];
    upcomingLeaves: UpcomingLeaveDto[];
    notifications: DashboardNotificationDto[];
}

export interface MyAttendanceDayDto {
    date: string;
    status: string; // Present | Late | Absent | OnLeave | Weekend
    checkIn: string | null;
    checkOut: string | null;
}

export interface MyDashboardDto {
    todayStatus: string; // NotCheckedIn | Present | Late | Absent | OnLeave
    checkInTime: string | null;
    checkOutTime: string | null;
    totalWorkingDaysThisMonth: number;
    presentDaysThisMonth: number;
    lateDaysThisMonth: number;
    absentDaysThisMonth: number;
    attendanceRateThisMonth: number;
    totalLeavesTaken: number;
    pendingLeaveRequests: number;
    last7Days: MyAttendanceDayDto[];
    upcomingLeaves: UpcomingLeaveDto[];
    notifications: DashboardNotificationDto[];
}
