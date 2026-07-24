import type { PaginationRequest } from '@/shared/types';

export const AttendanceStatus = {
  Pending: 1,
  Present: 2,
  Absent: 3,
  Late: 4,
  EarlyLeave: 5,
  Invalid: 6,
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

export interface AttendanceDto {
  id: string;
  employeeId: string;
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
  checkInTime: string;
  checkOutTime?: string;
  attendanceStatus: AttendanceStatus;
}

export interface AttendanceDetailsDto {
  id: string;
  employeeId: string;
  shiftId: string;
  attendanceDeviceId: string;
  attendancePolicyId: string;
  
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

  faceVerificationStatus: FaceVerificationStatus;
  gPSVerificationStatus: GPSVerificationStatus;
  deviceVerificationStatus: DeviceVerificationStatus;
  managerApprovalStatus: ManagerApprovalStatus;
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

// Maps to CreateAttendanceCommand in backend
export interface ManualAttendanceCommand {
  employeeId: string;
  attendanceDeviceId: string;
  attendancePolicyId: string;
  shiftId: string;
  checkInTime: string;
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
