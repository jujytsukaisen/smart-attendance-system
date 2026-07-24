import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { settingsApi } from '../api';
import type {
  SystemSettingsDto,
  GeneralSettingsDto,
  AttendanceSettingsDto,
  SecuritySettingsDto,
  NotificationSettingsDto,
  DeviceSettingsDto,
  EmailSettingsDto,
  SmsSettingsDto,
  BackupSettingsDto,
  FaceRecognitionSettingsDto,
} from '../types';

export const settingsKeys = {
  all: ['settings'] as const,
  general: () => [...settingsKeys.all, 'general'] as const,
  attendance: () => [...settingsKeys.all, 'attendance'] as const,
  security: () => [...settingsKeys.all, 'security'] as const,
  notifications: () => [...settingsKeys.all, 'notifications'] as const,
  devices: () => [...settingsKeys.all, 'devices'] as const,
  email: () => [...settingsKeys.all, 'email'] as const,
  sms: () => [...settingsKeys.all, 'sms'] as const,
  backup: () => [...settingsKeys.all, 'backup'] as const,
  faceRecognition: () => [...settingsKeys.all, 'faceRecognition'] as const,
};

// General
export const useGeneralSettings = () => {
  return useQuery({ queryKey: settingsKeys.general(), queryFn: settingsApi.getGeneralSettings });
};
export const useUpdateGeneralSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: GeneralSettingsDto) => settingsApi.updateGeneralSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.general() }),
  });
};

// Attendance
export const useAttendanceSettings = () => {
  return useQuery({ queryKey: settingsKeys.attendance(), queryFn: settingsApi.getAttendanceSettings });
};
export const useUpdateAttendanceSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AttendanceSettingsDto) => settingsApi.updateAttendanceSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.attendance() }),
  });
};

// Security
export const useSecuritySettings = () => {
  return useQuery({ queryKey: settingsKeys.security(), queryFn: settingsApi.getSecuritySettings });
};
export const useUpdateSecuritySettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: SecuritySettingsDto) => settingsApi.updateSecuritySettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.security() }),
  });
};

// Notifications
export const useNotificationSettings = () => {
  return useQuery({ queryKey: settingsKeys.notifications(), queryFn: settingsApi.getNotificationSettings });
};
export const useUpdateNotificationSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: NotificationSettingsDto) => settingsApi.updateNotificationSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.notifications() }),
  });
};

// Devices
export const useDeviceSettings = () => {
  return useQuery({ queryKey: settingsKeys.devices(), queryFn: settingsApi.getDeviceSettings });
};
export const useUpdateDeviceSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: DeviceSettingsDto) => settingsApi.updateDeviceSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.devices() }),
  });
};

// Email
export const useEmailSettings = () => {
  return useQuery({ queryKey: settingsKeys.email(), queryFn: settingsApi.getEmailSettings });
};
export const useUpdateEmailSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EmailSettingsDto) => settingsApi.updateEmailSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.email() }),
  });
};

// SMS
export const useSmsSettings = () => {
  return useQuery({ queryKey: settingsKeys.sms(), queryFn: settingsApi.getSmsSettings });
};
export const useUpdateSmsSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: SmsSettingsDto) => settingsApi.updateSmsSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.sms() }),
  });
};

// Backup
export const useBackupSettings = () => {
  return useQuery({ queryKey: settingsKeys.backup(), queryFn: settingsApi.getBackupSettings });
};
export const useUpdateBackupSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: BackupSettingsDto) => settingsApi.updateBackupSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.backup() }),
  });
};

// Face Recognition
export const useFaceRecognitionSettings = () => {
  return useQuery({ queryKey: settingsKeys.faceRecognition(), queryFn: settingsApi.getFaceRecognitionSettings });
};
export const useUpdateFaceRecognitionSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: FaceRecognitionSettingsDto) => settingsApi.updateFaceRecognitionSettings(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.faceRecognition() }),
  });
};

// Global
export const useResetSection = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (section: keyof SystemSettingsDto) => settingsApi.resetSection(section),
    onSuccess: (_, variables) => {
      const keyFn = settingsKeys[variables as keyof typeof settingsKeys];
      queryClient.invalidateQueries({ queryKey: typeof keyFn === 'function' ? keyFn() : keyFn });
    },
  });
};

export const useResetAllSettings = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => settingsApi.resetAll(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: settingsKeys.all }),
  });
};
