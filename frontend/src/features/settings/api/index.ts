import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
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
    const response = await apiClient.get<ApiEnvelope<SystemSettingsDto>>(`${BASE_URL}/system`);
    return response.data.data;
  },

  getGeneralSettings: async (): Promise<GeneralSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<GeneralSettingsDto>>(`${BASE_URL}/general`);
    return response.data.data;
  },

  updateGeneralSettings: async (data: GeneralSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/general`, data);
  },

  getAttendanceSettings: async (): Promise<AttendanceSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<AttendanceSettingsDto>>(`${BASE_URL}/attendance`);
    return response.data.data;
  },

  updateAttendanceSettings: async (data: AttendanceSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/attendance`, data);
  },

  getSecuritySettings: async (): Promise<SecuritySettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<SecuritySettingsDto>>(`${BASE_URL}/security`);
    return response.data.data;
  },

  updateSecuritySettings: async (data: SecuritySettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/security`, data);
  },

  getNotificationSettings: async (): Promise<NotificationSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<NotificationSettingsDto>>(`${BASE_URL}/notifications`);
    return response.data.data;
  },

  updateNotificationSettings: async (data: NotificationSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/notifications`, data);
  },

  getDeviceSettings: async (): Promise<DeviceSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<DeviceSettingsDto>>(`${BASE_URL}/devices`);
    return response.data.data;
  },

  updateDeviceSettings: async (data: DeviceSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/devices`, data);
  },

  getEmailSettings: async (): Promise<EmailSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<EmailSettingsDto>>(`${BASE_URL}/email`);
    return response.data.data;
  },

  updateEmailSettings: async (data: EmailSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/email`, data);
  },

  getSmsSettings: async (): Promise<SmsSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<SmsSettingsDto>>(`${BASE_URL}/sms`);
    return response.data.data;
  },

  updateSmsSettings: async (data: SmsSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/sms`, data);
  },

  getBackupSettings: async (): Promise<BackupSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<BackupSettingsDto>>(`${BASE_URL}/backup`);
    return response.data.data;
  },

  updateBackupSettings: async (data: BackupSettingsDto): Promise<void> => {
    await apiClient.put(`${BASE_URL}/backup`, data);
  },

  getFaceRecognitionSettings: async (): Promise<FaceRecognitionSettingsDto> => {
    const response = await apiClient.get<ApiEnvelope<FaceRecognitionSettingsDto>>(`${BASE_URL}/face-recognition`);
    return response.data.data;
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