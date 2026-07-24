import { apiClient } from '@/shared/api/axios';
import type { PagedResult } from '@/shared/types';
import type { 
  AttendancePolicyListDto, 
  AttendancePolicyDetailsDto, 
  CreateAttendancePolicyCommand, 
  UpdateAttendancePolicyCommand,
  AttendancePolicyFilter 
} from '../types';

const BASE_URL = '/v1/AttendancePolicies';

export const attendancePoliciesApi = {
  getAll: async (filter: AttendancePolicyFilter): Promise<PagedResult<AttendancePolicyListDto>> => {
    const { data } = await apiClient.get<PagedResult<AttendancePolicyListDto>>(BASE_URL, { params: filter });
    return data;
  },

  getById: async (id: string): Promise<AttendancePolicyDetailsDto> => {
    const { data } = await apiClient.get<AttendancePolicyDetailsDto>(`${BASE_URL}/${id}`);
    return data;
  },

  create: async (command: CreateAttendancePolicyCommand): Promise<string> => {
    const { data } = await apiClient.post<{ id: string }>(BASE_URL, command);
    return data.id;
  },

  update: async (command: UpdateAttendancePolicyCommand): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${command.id}`, command);
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  restore: async (id: string): Promise<void> => {
    await apiClient.patch(`${BASE_URL}/${id}/restore`);
  },

  activate: async (id: string): Promise<void> => {
    await apiClient.patch(`${BASE_URL}/${id}/activate`);
  },

  deactivate: async (id: string): Promise<void> => {
    await apiClient.patch(`/attendancepolicies/${id}/deactivate`);
  }
};
