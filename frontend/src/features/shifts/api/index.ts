import { apiClient } from '@/shared/api/axios';
import type { PagedResult, ApiEnvelope } from '@/shared/types';
import type {
  ShiftListDto,
  ShiftDetailsDto,
  CreateShiftCommand,
  UpdateShiftCommand,
  ShiftFilter,
  WorkScheduleListDto,
} from '../types';

const BASE_URL = '/v1/Shifts';

export const getShifts = async (filter: ShiftFilter): Promise<PagedResult<ShiftListDto>> => {
  const response = await apiClient.get<ApiEnvelope<PagedResult<ShiftListDto>>>(BASE_URL, {
    params: filter,
  });
  return response.data.data;
};

export const getShiftById = async (id: string): Promise<ShiftDetailsDto> => {
  const response = await apiClient.get<ApiEnvelope<ShiftDetailsDto>>(`${BASE_URL}/${id}`);
  return response.data.data;
};

export const createShift = async (command: CreateShiftCommand): Promise<string> => {
  const response = await apiClient.post<ApiEnvelope<string>>(BASE_URL, command);
  return response.data.data;
};

export const updateShift = async (command: UpdateShiftCommand): Promise<void> => {
  await apiClient.put(`${BASE_URL}/${command.shiftId}`, command);
};

export const deleteShift = async (id: string): Promise<void> => {
  await apiClient.delete(`${BASE_URL}/${id}`);
};

export const activateShift = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/activate`);
};

export const deactivateShift = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/deactivate`);
};

export const restoreShift = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/restore`);
};

export const getWorkSchedules = async (pageNumber = 1, pageSize = 100): Promise<PagedResult<WorkScheduleListDto>> => {
  const response = await apiClient.get<ApiEnvelope<PagedResult<WorkScheduleListDto>>>('/v1/WorkSchedules', {
    params: { pageNumber, pageSize },
  });
  return response.data.data;
};