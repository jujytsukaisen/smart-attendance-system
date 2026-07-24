export interface GeneralSettingsDto {
  companyName: string;
  dateFormat: string;
  timeFormat: string;
  timeZone: string;
  language: string;
  theme: 'Light' | 'Dark' | 'System';
}

export interface AttendanceSettingsDto {
  workingHoursStart: string;
  workingHoursEnd: string;
  gracePeriodMinutes: number;
  lateThresholdMinutes: number;
  minimumHoursForHalfDay: number;
  minimumHoursForFullDay: number;
  maximumOvertimeHours: number;
  weekendRules: string;
  holidayRules: string;
}

export interface SecuritySettingsDto {
  passwordPolicy: string;
  sessionTimeoutMinutes: number;
  jwtLifetimeMinutes: number;
  refreshTokenLifetimeDays: number;
  twoFactorAuthEnabled: boolean;
  maxLoginAttempts: number;
  lockoutDurationMinutes: number;
}

export interface NotificationSettingsDto {
  emailNotificationsEnabled: boolean;
  smsNotificationsEnabled: boolean;
  pushNotificationsEnabled: boolean;
  attendanceAlertsEnabled: boolean;
  leaveAlertsEnabled: boolean;
  deviceAlertsEnabled: boolean;
}

export interface DeviceSettingsDto {
  defaultDeviceTimeoutSeconds: number;
  heartbeatIntervalSeconds: number;
  autoSyncEnabled: boolean;
  retryAttempts: number;
  connectionTimeoutSeconds: number;
}

export interface EmailSettingsDto {
  smtpHost: string;
  smtpPort: number;
  enableSsl: boolean;
  username: string;
  senderName: string;
  senderEmail: string;
}

export interface SmsSettingsDto {
  provider: string;
  apiKey: string;
  senderName: string;
}

export interface BackupSettingsDto {
  automaticBackupEnabled: boolean;
  backupTime: string;
  retentionDays: number;
  storageProvider: string;
}

export interface FaceRecognitionSettingsDto {
  recognitionThreshold: number;
  minimumFaceQuality: number;
  livenessDetectionEnabled: boolean;
  maskDetectionEnabled: boolean;
  spoofDetectionEnabled: boolean;
}

export interface SystemSettingsDto {
  general: GeneralSettingsDto;
  attendance: AttendanceSettingsDto;
  security: SecuritySettingsDto;
  notifications: NotificationSettingsDto;
  devices: DeviceSettingsDto;
  email: EmailSettingsDto;
  sms: SmsSettingsDto;
  backup: BackupSettingsDto;
  faceRecognition: FaceRecognitionSettingsDto;
}

export interface UpdateSettingsCommand<T> {
  settings: T;
}
