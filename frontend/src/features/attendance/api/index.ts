import { apiClient } from '@/shared/api/axios';
import type { PagedResult } from '@/shared/types';
import type {
  AttendanceListDto,
  AttendanceDetailsDto,
  CheckInCommand,
  CheckOutCommand,
  ManualAttendanceCommand,
  UpdateAttendanceCommand,
  AttendanceFilter
} from '../types';

const BASE_URL = '/v1/Attendances';

export const attendanceApi = {
  getAttendances: async (filter: AttendanceFilter) => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.status !== undefined) params.append('status', filter.status.toString());
    if (filter.employeeId) params.append('employeeId', filter.employeeId);
    if (filter.startDate) params.append('startDate', filter.startDate);
    if (filter.endDate) params.append('endDate', filter.endDate);

    const response = await apiClient.get<PagedResult<AttendanceListDto>>(`${BASE_URL}?${params.toString()}`);
    return response.data;
  },

  getAttendanceDetails: async (id: string) => {
    const response = await apiClient.get<AttendanceDetailsDto>(`${BASE_URL}/${id}`);
    return response.data;
  },

  checkIn: async (data: CheckInCommand) => {
    const response = await apiClient.post(`${BASE_URL}/check-in`, data);
    return response.data;
  },

  checkOut: async (data: CheckOutCommand) => {
    const response = await apiClient.post(`${BASE_URL}/check-out`, data);
    return response.data;
  },

  manualAttendance: async (data: ManualAttendanceCommand) => {
    const response = await apiClient.post(`${BASE_URL}/manual`, data);
    return response.data;
  },

  updateAttendance: async (id: string, data: UpdateAttendanceCommand) => {
    const response = await apiClient.put(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  deleteAttendance: async (id: string) => {
    const response = await apiClient.delete(`/attendances/${id}`);
    return response.data;
  }
};
