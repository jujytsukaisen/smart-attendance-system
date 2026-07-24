export const DeviceType = {
  WallMounted: 1,
  Mobile: 2,
  Desktop: 3,
  Web: 4,
  Kiosk: 5,
} as const;

export type DeviceType = typeof DeviceType[keyof typeof DeviceType];

export const DeviceStatus = {
  PendingApproval: 1,
  Approved: 2,
  Revoked: 3,
  Maintenance: 4,
  Retired: 5,
} as const;

export type DeviceStatus = typeof DeviceStatus[keyof typeof DeviceStatus];

export const ConnectionStatus = {
  Offline: 1,
  Online: 2,
} as const;

export type ConnectionStatus = typeof ConnectionStatus[keyof typeof ConnectionStatus];

export const CommunicationProtocol = {
  TCP: 1,
  UDP: 2,
  HTTP: 3,
  HTTPS: 4,
} as const;

export type CommunicationProtocol = typeof CommunicationProtocol[keyof typeof CommunicationProtocol];

export const SyncStatus = {
  Pending: 1,
  InProgress: 2,
  Completed: 3,
  Failed: 4,
} as const;

export type SyncStatus = typeof SyncStatus[keyof typeof SyncStatus];

export interface AttendanceDeviceDto {
  id: string;
  name: string;
  code: string;
  serialNumber?: string;
  deviceType: DeviceType;
  deviceStatus: DeviceStatus;
  branchId?: string;
  connectionStatus: ConnectionStatus;
  isActive: boolean;
  lastHeartbeat?: string;
}

export type AttendanceDeviceListDto = AttendanceDeviceDto;

export interface AttendanceDeviceDetailsDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  serialNumber?: string;
  deviceType: DeviceType;
  deviceStatus: DeviceStatus;
  operatingSystem?: string;
  applicationVersion?: string;
  ipAddress?: string;
  port?: number;
  communicationProtocol: CommunicationProtocol;
  connectionStatus: ConnectionStatus;
  branchId?: string;
  latitude: number;
  longitude: number;
  allowedRadiusMeters: number;
  publicKey?: string;
  deviceFingerprint?: string;
  requireDeviceApproval: boolean;
  requireSecureConnection: boolean;
  supportsFaceRecognition: boolean;
  supportsFingerprint: boolean;
  supportsGPS: boolean;
  supportsOfflineAttendance: boolean;
  supportsQRCode: boolean;
  supportsNFC: boolean;
  lastHeartbeat?: string;
  lastSynchronization?: string;
  isActive: boolean;
}

export interface CreateAttendanceDeviceCommand {
  companyId: string;
  name: string;
  code: string;
  serialNumber?: string;
  deviceType: DeviceType;
  operatingSystem?: string;
  applicationVersion?: string;
  branchId?: string;
  latitude: number;
  longitude: number;
  allowedRadiusMeters: number;
  publicKey?: string;
  deviceFingerprint?: string;
  requireDeviceApproval: boolean;
  requireSecureConnection: boolean;
  supportsFaceRecognition: boolean;
  supportsFingerprint: boolean;
  supportsGPS: boolean;
  supportsOfflineAttendance: boolean;
  supportsQRCode: boolean;
  supportsNFC: boolean;
  ipAddress?: string;
  port?: number;
  communicationProtocol: CommunicationProtocol;
}

export interface UpdateAttendanceDeviceCommand extends Omit<CreateAttendanceDeviceCommand, 'companyId'> {
  id: string;
}

export interface DeviceFilter {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  deviceType?: DeviceType;
  deviceStatus?: DeviceStatus;
  connectionStatus?: ConnectionStatus;
}

// Mocked return types for missing hardware endpoints
export interface SyncResultDto {
  id: string;
  deviceId: string;
  syncTime: string;
  recordsSynced: number;
  status: SyncStatus;
  message?: string;
}

export interface DeviceLogDto {
  id: string;
  timestamp: string;
  level: 'Info' | 'Warning' | 'Error';
  message: string;
}
