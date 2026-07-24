export const LeaveStatus = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Cancelled: 4,
} as const;

export type LeaveStatus = typeof LeaveStatus[keyof typeof LeaveStatus];

export const LeaveType = {
  Annual: 1,
  Sick: 2,
  Maternity: 3,
  Paternity: 4,
  Unpaid: 5,
  Other: 6,
} as const;

export type LeaveType = typeof LeaveType[keyof typeof LeaveType];

export interface LeaveListDto {
  id: string;
  employeeId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  status: string;
}

export interface LeaveDetailsDto {
  id: string;
  companyId: string;
  employeeId: string;
  approverId?: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
  managerComment: string;
}

export interface CreateLeaveCommand {
  companyId: string;
  employeeId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface UpdateLeaveCommand {
  leaveId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface ApproveLeaveCommand {
  leaveId: string;
  comment: string;
}

export interface RejectLeaveCommand {
  leaveId: string;
  reason: string;
}

export interface CancelLeaveCommand {
  leaveId: string;
  reason: string;
}

export interface LeaveFilter {
  pageNumber: number;
  pageSize: number;
  employeeId?: string;
  status?: LeaveStatus;
  startDate?: string;
  endDate?: string;
}
