import type { PaginationRequest } from '@/shared/types';

// ── Existing Enums (unchanged) ───────────────────────────────────────────────

export const AttendanceStatus = {
  Pending: 1,
  Present: 2,
  Absent: 3,
  Late: 4,
  EarlyLeave: 5,
  PresentDuringLeave: 6,
  LateDuringLeave: 7,
  Invalid: 8,
} as const;
export type AttendanceStatus = typeof AttendanceStatus[keyof typeof AttendanceStatus];

export const CheckInMethod = {
  FaceRecognition: 1,
  GPS: 2,
  BiometricDevice: 3,
  Web: 4,
  Mobile: 5,
  Manual: 6,
} as const;
export type CheckInMethod = typeof CheckInMethod[keyof typeof CheckInMethod];

export const FaceVerificationStatus = {
  Pending: 1,
  Verified: 2,
  Failed: 3,
  Bypassed: 4,
  NotRequired: 5,
} as const;
export type FaceVerificationStatus = typeof FaceVerificationStatus[keyof typeof FaceVerificationStatus];

export const GPSVerificationStatus = {
  Pending: 1,
  Verified: 2,
  Failed: 3,
  Bypassed: 4,
  NotRequired: 5,
} as const;
export type GPSVerificationStatus = typeof GPSVerificationStatus[keyof typeof GPSVerificationStatus];

export const DeviceVerificationStatus = {
  Pending: 1,
  Verified: 2,
  Failed: 3,
  Bypassed: 4,
  NotRequired: 5,
} as const;
export type DeviceVerificationStatus = typeof DeviceVerificationStatus[keyof typeof DeviceVerificationStatus];

export const ManagerApprovalStatus = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  NotRequired: 4,
} as const;
export type ManagerApprovalStatus = typeof ManagerApprovalStatus[keyof typeof ManagerApprovalStatus];

// ── New: Multi-Scan State Machine Types ──────────────────────────────────────

/** Matches the backend AttendanceEventType enum. */
export const AttendanceEventType = {
  CheckIn:    'CheckIn',
  BreakStart: 'BreakStart',
  BreakEnd:   'BreakEnd',
  CheckOut:   'CheckOut',
} as const;
export type AttendanceEventType = typeof AttendanceEventType[keyof typeof AttendanceEventType];

/** Derived live state of an employee for the current workday. */
export const EmployeeAttendanceState = {
  NotCheckedIn: 'NotCheckedIn',
  Working:      'Working',
  OnBreak:      'OnBreak',
  Finished:     'Finished',
} as const;
export type EmployeeAttendanceState = typeof EmployeeAttendanceState[keyof typeof EmployeeAttendanceState];

/** A single scan event in the daily attendance timeline. */
export interface AttendanceEventDto {
  id: string;
  attendanceId: string;
  timestamp: string;
  eventType: AttendanceEventType | string;
  eventTypeLabel: string;
  note?: string;
}

/** Live state response for a single employee today. */
export interface EmployeeCurrentStateDto {
  employeeId: string;
  state: EmployeeAttendanceState | string;
  checkInTime?: string;
  lastEventTime?: string;
  attendanceId?: string;
}

// ── Existing DTOs (unchanged) ─────────────────────────────────────────────────

export interface AttendanceDto {
  id: string;
  employeeId: string;
  employeeName?: string;
  employeeNumber?: string;
  attendancePolicyId: string;
  shiftId: string;
  checkInTime: string;
  checkOutTime?: string;
  attendanceStatus: AttendanceStatus;
  workingMinutes: number;
}

export interface AttendanceListDto {
  id: string;
  employeeId: string;
  employeeName?: string;
  employeeNumber?: string;
  checkInTime: string;
  checkOutTime?: string;
  attendanceStatus: AttendanceStatus;
}

export interface AttendanceDetailsDto {
  id: string;
  employeeId: string;
  employeeName?: string;
  employeeNumber?: string;
  shiftId: string;
  shiftName?: string;
  attendanceDeviceId: string;
  deviceName?: string;
  attendancePolicyId: string;
  policyName?: string;
  
  checkInTime: string;
  checkInMethod: CheckInMethod;
  checkInLatitude?: number;
  checkInLongitude?: number;
  checkInIPAddress?: string;
  checkInDeviceInfo?: string;
  checkInPhotoReference?: string;

  checkOutTime?: string;
  checkOutMethod?: CheckInMethod;
  checkOutLatitude?: number;
  checkOutLongitude?: number;
  checkOutIPAddress?: string;
  checkOutDeviceInfo?: string;
  checkOutPhotoReference?: string;

  attendanceStatus: AttendanceStatus;
  workingMinutes: number;
  lateMinutes: number;
  earlyLeaveMinutes: number;
  overtimeMinutes: number;
  breakMinutes: number;
  excessBreakMinutes: number;

  faceVerificationStatus: FaceVerificationStatus;
  gPSVerificationStatus: GPSVerificationStatus;
  deviceVerificationStatus: DeviceVerificationStatus;
  managerApprovalStatus: ManagerApprovalStatus;

  // ── New multi-scan fields ──────────────────────────────────────────────────
  /** Ordered list of all scan events for the day. */
  timeline?: AttendanceEventDto[];
  /** Derived current state: NotCheckedIn | Working | OnBreak | Finished. */
  currentState?: EmployeeAttendanceState | string;
}

export interface CheckInCommand {
  employeeId: string;
  shiftId: string;
  attendancePolicyId: string;
  attendanceDeviceId?: string;
  checkInMethod: CheckInMethod;
  latitude: number;
  longitude: number;
  iPAddress: string;
  deviceInformation: string;
  checkInTime: string;
}

export interface CheckOutCommand {
  attendanceId: string;
  checkOutMethod: CheckInMethod;
  latitude: number;
  longitude: number;
  iPAddress: string;
  deviceInformation: string;
  checkOutTime: string;
}

export interface RecordScanCommand {
  employeeId: string;
  shiftId?: string;
  attendancePolicyId: string;
  attendanceDeviceId?: string;
  checkInMethod: number;
  latitude: number;
  longitude: number;
  iPAddress?: string;
  deviceInformation?: string;
  timestamp: string;
  /** When true, forces CheckOut even while in Working state. */
  isExplicitCheckOut?: boolean;
  note?: string;
}

/** Response from POST /scan */
export interface RecordScanResponse {
  attendanceId: string;
  eventId: string;
  actionTaken: AttendanceEventType | string;
  currentState: EmployeeAttendanceState | string;
  timestamp: string;
}

// Maps to CreateAttendanceCommand in backend
export interface ManualAttendanceCommand {
  employeeId: string;
  attendanceDeviceId?: string;
  attendancePolicyId?: string;
  shiftId?: string;
  checkInTime: string;
  checkOutTime?: string;
  checkInMethod: CheckInMethod;
}

export interface UpdateAttendanceCommand {
  attendanceId: string;
  checkOutTime?: string;
}

export interface AttendanceFilter extends PaginationRequest {
  searchTerm?: string;
  status?: AttendanceStatus;
  employeeId?: string;
  startDate?: string;
  endDate?: string;
}
