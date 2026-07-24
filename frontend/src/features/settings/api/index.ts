import { apiClient } from '@/shared/api/axios';
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

const BASE_URL = '/v1/Settings';

export const settingsApi = {
  getSystemSettings: async (): Promise<SystemSettingsDto> => {
    const response = await apiClient.get<SystemSettingsDto>(`${BASE_URL}/system`);
    return response.data;
  },

  getGeneralSettings: async (): Promise<GeneralSettingsDto> => {
    const response = await apiClient.get<GeneralSettingsDto>(`${BASE_URL}/general`);
    return response.data;
  },

  updateGeneralSettings: async (data: GeneralSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/general`, data);
  },

  getAttendanceSettings: async (): Promise<AttendanceSettingsDto> => {
    const response = await apiClient.get<AttendanceSettingsDto>(`${BASE_URL}/attendance`);
    return response.data;
  },

  updateAttendanceSettings: async (data: AttendanceSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/attendance`, data);
  },

  getSecuritySettings: async (): Promise<SecuritySettingsDto> => {
    const response = await apiClient.get<SecuritySettingsDto>(`${BASE_URL}/security`);
    return response.data;
  },

  updateSecuritySettings: async (data: SecuritySettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/security`, data);
  },

  getNotificationSettings: async (): Promise<NotificationSettingsDto> => {
    const response = await apiClient.get<NotificationSettingsDto>(`${BASE_URL}/notifications`);
    return response.data;
  },

  updateNotificationSettings: async (data: NotificationSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/notifications`, data);
  },

  getDeviceSettings: async (): Promise<DeviceSettingsDto> => {
    const response = await apiClient.get<DeviceSettingsDto>(`${BASE_URL}/devices`);
    return response.data;
  },

  updateDeviceSettings: async (data: DeviceSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/devices`, data);
  },

  getEmailSettings: async (): Promise<EmailSettingsDto> => {
    const response = await apiClient.get<EmailSettingsDto>(`${BASE_URL}/email`);
    return response.data;
  },

  updateEmailSettings: async (data: EmailSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/email`, data);
  },

  getSmsSettings: async (): Promise<SmsSettingsDto> => {
    const response = await apiClient.get<SmsSettingsDto>(`${BASE_URL}/sms`);
    return response.data;
  },

  updateSmsSettings: async (data: SmsSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/sms`, data);
  },

  getBackupSettings: async (): Promise<BackupSettingsDto> => {
    const response = await apiClient.get<BackupSettingsDto>(`${BASE_URL}/backup`);
    return response.data;
  },

  updateBackupSettings: async (data: BackupSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/backup`, data);
  },

  getFaceRecognitionSettings: async (): Promise<FaceRecognitionSettingsDto> => {
    const response = await apiClient.get<FaceRecognitionSettingsDto>(`${BASE_URL}/face-recognition`);
    return response.data;
  },

  updateFaceRecognitionSettings: async (data: FaceRecognitionSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/face-recognition`, data);
  },

  resetSection: async (section: keyof SystemSettingsDto): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${section}/reset`);
  },

  resetAll: async (): Promise<void> => {
    await apiClient.post(`${BASE_URL}/reset-all`);
  }
};
