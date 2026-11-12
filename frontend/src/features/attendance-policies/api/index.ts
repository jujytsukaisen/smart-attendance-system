import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  AttendancePolicyListDto,
  AttendancePolicyDetailsDto,
  CreateAttendancePolicyCommand,
  UpdateAttendancePolicyCommand,
  AttendancePolicyFilter,
} from '../types';

const API_URL = '/v1/AttendancePolicies';

export const getAttendancePolicies = async (
  filter: AttendancePolicyFilter
): Promise<PagedResult<AttendancePolicyListDto>> => {
  const { data } = await apiClient.get<ApiEnvelope<PagedResult<AttendancePolicyListDto>>>(API_URL, {
    params: filter,
  });
  return data.data;
};

export const getAttendancePolicy = async (id: string): Promise<AttendancePolicyDetailsDto> => {
  const { data } = await apiClient.get<ApiEnvelope<AttendancePolicyDetailsDto>>(`${API_URL}/${id}`);
  return data.data;
};

export const createAttendancePolicy = async (
  command: CreateAttendancePolicyCommand
): Promise<string> => {
  const { data } = await apiClient.post<ApiEnvelope<string>>(API_URL, command);
  return data.data;
};

export const updateAttendancePolicy = async (
  command: UpdateAttendancePolicyCommand
): Promise<void> => {
  await apiClient.put(`${API_URL}/${command.id}`, command);
};

export const deleteAttendancePolicy = async (id: string): Promise<void> => {
  await apiClient.delete(`${API_URL}/${id}`);
};

export const restoreAttendancePolicy = async (id: string): Promise<void> => {
  await apiClient.post(`${API_URL}/${id}/restore`);
};

export const activateAttendancePolicy = async (id: string): Promise<void> => {
  await apiClient.post(`${API_URL}/${id}/activate`);
};

export const deactivateAttendancePolicy = async (id: string): Promise<void> => {
  await apiClient.post(`${API_URL}/${id}/deactivate`);
};