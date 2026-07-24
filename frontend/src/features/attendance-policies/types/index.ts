import type { PaginationRequest } from '@/shared/types';

export const AttendanceMode = {
  Office: 1,
  Field: 2,
  Hybrid: 3,
  Remote: 4,
  Flexible: 5,
} as const;
export type AttendanceMode = typeof AttendanceMode[keyof typeof AttendanceMode];

export const RoundTimeRule = {
  None: 0,
  RoundUp: 1,
  RoundDown: 2,
  RoundToNearest: 3,
} as const;
export type RoundTimeRule = typeof RoundTimeRule[keyof typeof RoundTimeRule];

export const MissingPunchPolicy = {
  Reject: 0,
  MarkAsAbsent: 1,
  RequireApproval: 2,
  DeductHalfDay: 3,
} as const;
export type MissingPunchPolicy = typeof MissingPunchPolicy[keyof typeof MissingPunchPolicy];

export const WeekendPolicy = {
  Regular: 0,
  Overtime: 1,
  StrictAbsent: 2,
} as const;
export type WeekendPolicy = typeof WeekendPolicy[keyof typeof WeekendPolicy];

export const HolidayPolicy = {
  Regular: 0,
  Overtime: 1,
  StrictAbsent: 2,
} as const;
export type HolidayPolicy = typeof HolidayPolicy[keyof typeof HolidayPolicy];

export interface AttendancePolicyDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  description?: string;
  
  attendanceMode: AttendanceMode;
  
  requireFaceRecognition: boolean;
  requireGPS: boolean;
  requireApprovedDevice: boolean;
  requireCompanyNetwork: boolean;
  allowMobileCheckIn: boolean;
  allowDesktopCheckIn: boolean;
  allowWebCheckIn: boolean;
  allowOfflineAttendance: boolean;
  allowedRadiusMeters: number;
  allowMultipleCheckIn: boolean;
  allowManualAttendance: boolean;
  requireManagerApproval: boolean;
  maximumDistanceMeters: number;
  
  lateToleranceMinutes: number;
  earlyLeaveToleranceMinutes: number;
  gracePeriodMinutes: number;
  overtimeThresholdMinutes: number;
  minimumOvertimeMinutes: number;
  maximumOvertimeMinutes: number;
  
  autoCheckoutEnabled: boolean;
  autoCheckoutTime?: string;
  
  multipleShiftsSupport: boolean;
  
  roundTimeRule: RoundTimeRule;
  missingPunchPolicy: MissingPunchPolicy;
  weekendPolicy: WeekendPolicy;
  holidayPolicy: HolidayPolicy;
  
  isActive: boolean;
  isDefault: boolean;
}

export interface AttendancePolicyListDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  attendanceMode: AttendanceMode;
  isActive: boolean;
  isDefault: boolean;
}

export interface AttendancePolicyDetailsDto extends AttendancePolicyDto {}

export interface CreateAttendancePolicyCommand {
  companyId: string;
  name: string;
  code: string;
  description?: string;
  
  attendanceMode: AttendanceMode;
  requireFaceRecognition: boolean;
  requireGPS: boolean;
  requireApprovedDevice: boolean;
  requireCompanyNetwork: boolean;
  allowMobileCheckIn: boolean;
  allowDesktopCheckIn: boolean;
  allowWebCheckIn: boolean;
  allowOfflineAttendance: boolean;
  allowedRadiusMeters: number;
  allowMultipleCheckIn: boolean;
  allowManualAttendance: boolean;
  requireManagerApproval: boolean;
  maximumDistanceMeters: number;

  lateToleranceMinutes: number;
  earlyLeaveToleranceMinutes: number;
  gracePeriodMinutes: number;
  overtimeThresholdMinutes: number;
  minimumOvertimeMinutes: number;
  maximumOvertimeMinutes: number;
  
  autoCheckoutEnabled: boolean;
  autoCheckoutTime?: string;
  
  multipleShiftsSupport: boolean;
  
  roundTimeRule: RoundTimeRule;
  missingPunchPolicy: MissingPunchPolicy;
  weekendPolicy: WeekendPolicy;
  holidayPolicy: HolidayPolicy;
}

export interface UpdateAttendancePolicyCommand extends CreateAttendancePolicyCommand {
  id: string;
}

export interface AttendancePolicyFilter extends PaginationRequest {
  searchTerm?: string;
  isActive?: boolean;
}
