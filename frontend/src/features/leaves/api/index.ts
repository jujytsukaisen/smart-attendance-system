import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  LeaveListDto,
  LeaveDetailsDto,
  CreateLeaveCommand,
  UpdateLeaveCommand,
  ApproveLeaveCommand,
  RejectLeaveCommand,
  CancelLeaveCommand,
  LeaveFilter
} from '../types';

const BASE_URL = '/v1/Leaves';

export const leavesApi = {
  getLeaves: async (filter: LeaveFilter): Promise<PagedResult<LeaveListDto>> => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.employeeId) params.append('employeeId', filter.employeeId);
    if (filter.status !== undefined) params.append('status', filter.status.toString());
    if (filter.startDate) params.append('startDate', filter.startDate);
    if (filter.endDate) params.append('endDate', filter.endDate);

    const response = await apiClient.get<ApiEnvelope<PagedResult<LeaveListDto>>>(`${BASE_URL}?${params.toString()}`);
    return response.data.data;
  },

  getMyLeaves: async (filter: LeaveFilter): Promise<PagedResult<LeaveListDto>> => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());

    const response = await apiClient.get<ApiEnvelope<PagedResult<LeaveListDto>>>(`${BASE_URL}/my?${params.toString()}`);
    return response.data.data;
  },

  getLeaveById: async (id: string): Promise<LeaveDetailsDto> => {
    const response = await apiClient.get<ApiEnvelope<LeaveDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
  },

  createLeave: async (data: CreateLeaveCommand): Promise<string> => {
    const response = await apiClient.post<ApiEnvelope<string>>(BASE_URL, data);
    return response.data.data;
  },

  updateLeave: async (data: UpdateLeaveCommand): Promise<void> => {
    const { leaveId, ...rest } = data;
    await apiClient.put(`${BASE_URL}/${leaveId}`, rest);
  },

  deleteLeave: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  approveLeave: async (data: ApproveLeaveCommand): Promise<void> => {
    const { leaveId, ...rest } = data;
    await apiClient.post(`${BASE_URL}/${leaveId}/approve`, rest);
  },

  rejectLeave: async (data: RejectLeaveCommand): Promise<void> => {
    const { leaveId, ...rest } = data;
    await apiClient.post(`${BASE_URL}/${leaveId}/reject`, rest);
  },

  cancelLeave: async (data: CancelLeaveCommand): Promise<void> => {
    const { leaveId, ...rest } = data;
    await apiClient.post(`${BASE_URL}/${leaveId}/cancel`, rest);
  },
};