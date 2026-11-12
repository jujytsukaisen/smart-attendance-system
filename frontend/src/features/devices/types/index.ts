export const DeviceType = {
  WallMounted: 'WallMounted',
  Mobile: 'Mobile',
  Desktop: 'Desktop',
  Web: 'Web',
  Kiosk: 'Kiosk',
} as const;

export type DeviceType = typeof DeviceType[keyof typeof DeviceType];

export const DeviceStatus = {
  PendingApproval: 'PendingApproval',
  Approved: 'Approved',
  Revoked: 'Revoked',
  Maintenance: 'Maintenance',
  Retired: 'Retired',
} as const;

export type DeviceStatus = typeof DeviceStatus[keyof typeof DeviceStatus];

export const ConnectionStatus = {
  Offline: 'Offline',
  Online: 'Online',
} as const;

export type ConnectionStatus = typeof ConnectionStatus[keyof typeof ConnectionStatus];

export const CommunicationProtocol = {
  TCP: 'TCP',
  UDP: 'UDP',
  HTTP: 'HTTP',
  HTTPS: 'HTTPS',
} as const;

export type CommunicationProtocol = typeof CommunicationProtocol[keyof typeof CommunicationProtocol];

export const SyncStatus = {
  Pending: 'Pending',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Failed: 'Failed',
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
