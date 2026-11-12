import { z } from 'zod';

export const generalSettingsSchema = z.object({
  companyName: z.string().min(1, 'Company Name is required'),
  dateFormat: z.string().min(1, 'Date Format is required'),
  timeFormat: z.string().min(1, 'Time Format is required'),
  timeZone: z.string().min(1, 'Time Zone is required'),
  language: z.string().min(1, 'Language is required'),
  theme: z.enum(['Light', 'Dark', 'System']),
});

export const attendanceSettingsSchema = z.object({
  workingHoursStart: z.string().regex(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/, 'Invalid time format (HH:MM)'),
  workingHoursEnd: z.string().regex(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/, 'Invalid time format (HH:MM)'),
  gracePeriodMinutes: z.number().min(0, 'Must be positive'),
  lateThresholdMinutes: z.number().min(0, 'Must be positive'),
  minimumHoursForHalfDay: z.number().min(0).max(24),
  minimumHoursForFullDay: z.number().min(0).max(24),
  maximumOvertimeHours: z.number().min(0),
  weekendRules: z.string().min(1),
  holidayRules: z.string().min(1),
});

export const securitySettingsSchema = z.object({
  passwordPolicy: z.string().min(1, 'Password Policy is required'),
  sessionTimeoutMinutes: z.number().min(1),
  jwtLifetimeMinutes: z.number().min(1),
  refreshTokenLifetimeDays: z.number().min(1),
  twoFactorAuthEnabled: z.boolean(),
  maxLoginAttempts: z.number().min(1),
  lockoutDurationMinutes: z.number().min(1),
});

export const notificationSettingsSchema = z.object({
  emailNotificationsEnabled: z.boolean(),
  smsNotificationsEnabled: z.boolean(),
  pushNotificationsEnabled: z.boolean(),
  attendanceAlertsEnabled: z.boolean(),
  leaveAlertsEnabled: z.boolean(),
  deviceAlertsEnabled: z.boolean(),
});

export const deviceSettingsSchema = z.object({
  defaultDeviceTimeoutSeconds: z.number().min(1),
  heartbeatIntervalSeconds: z.number().min(1),
  autoSyncEnabled: z.boolean(),
  retryAttempts: z.number().min(0),
  connectionTimeoutSeconds: z.number().min(1),
});

export const emailSettingsSchema = z.object({
  smtpHost: z.string().min(1, 'SMTP Host is required'),
  smtpPort: z.number().min(1, 'SMTP Port is required'),
  enableSsl: z.boolean(),
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required'),
  senderName: z.string().min(1, 'Sender Name is required'),
  senderEmail: z.string().email('Invalid email address'),
});

export const smsSettingsSchema = z.object({
  provider: z.string().min(1, 'Provider is required'),
  apiKey: z.string().min(1, 'API Key is required'),
  senderName: z.string().min(1, 'Sender Name is required'),
});

export const backupSettingsSchema = z.object({
  automaticBackupEnabled: z.boolean(),
  backupTime: z.string().regex(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/, 'Invalid time format (HH:MM)'),
  retentionDays: z.number().min(1),
  storageProvider: z.string().min(1, 'Storage Provider is required'),
});

export const faceRecognitionSettingsSchema = z.object({
  recognitionThreshold: z.number().min(0).max(100),
  minimumFaceQuality: z.number().min(0).max(100),
  livenessDetectionEnabled: z.boolean(),
  maskDetectionEnabled: z.boolean(),
  spoofDetectionEnabled: z.boolean(),
});
