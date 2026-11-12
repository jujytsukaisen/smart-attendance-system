import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  AttendanceDeviceListDto,
  AttendanceDeviceDetailsDto,
  CreateAttendanceDeviceCommand,
  UpdateAttendanceDeviceCommand,
  DeviceFilter,
  SyncResultDto,
  DeviceLogDto
} from '../types';

const BASE_URL = '/v1/AttendanceDevices';

export const devicesApi = {
  getDevices: async (filter: DeviceFilter): Promise<PagedResult<AttendanceDeviceListDto>> => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.deviceType) params.append('deviceType', filter.deviceType.toString());
    if (filter.deviceStatus) params.append('deviceStatus', filter.deviceStatus.toString());
    if (filter.connectionStatus) params.append('connectionStatus', filter.connectionStatus.toString());

    const response = await apiClient.get<ApiEnvelope<PagedResult<AttendanceDeviceListDto>>>(`${BASE_URL}?${params.toString()}`);
    return response.data.data;
  },

  getDeviceById: async (id: string): Promise<AttendanceDeviceDetailsDto> => {
    const response = await apiClient.get<ApiEnvelope<AttendanceDeviceDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
  },

  createDevice: async (data: CreateAttendanceDeviceCommand): Promise<string> => {
    const response = await apiClient.post<ApiEnvelope<string>>(BASE_URL, data);
    return response.data.data;
  },

  updateDevice: async (data: UpdateAttendanceDeviceCommand): Promise<void> => {
    const { id, ...rest } = data;
    await apiClient.put(`${BASE_URL}/${id}`, rest);
  },

  deleteDevice: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  activateDevice: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/activate`);
  },

  deactivateDevice: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/deactivate`);
  },

  restoreDevice: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/restore`);
  },

  syncDevice: async (id: string): Promise<SyncResultDto> => {
    const response = await apiClient.post<ApiEnvelope<SyncResultDto>>(`${BASE_URL}/${id}/sync`);
    return response.data.data;
  },

  testConnection: async (id: string): Promise<boolean> => {
    const response = await apiClient.post<ApiEnvelope<boolean>>(`${BASE_URL}/${id}/test-connection`);
    return response.data.data;
  },

  restartDevice: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/restart`);
  },

  uploadUsers: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/upload-users`);
  },

  uploadPolicies: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/upload-policies`);
  },

  downloadLogs: async (id: string): Promise<DeviceLogDto[]> => {
    const response = await apiClient.get<ApiEnvelope<DeviceLogDto[]>>(`${BASE_URL}/${id}/logs`);
    return response.data.data;
  }
};