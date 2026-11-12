import { apiClient } from '@/shared/api/axios';
import type { PagedResult, ApiEnvelope } from '@/shared/types';
import type {
  AttendanceListDto,
  AttendanceDetailsDto,
  AttendanceEventDto,
  EmployeeCurrentStateDto,
  RecordScanCommand,
  RecordScanResponse,
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

    const response = await apiClient.get<ApiEnvelope<PagedResult<AttendanceListDto>>>(`${BASE_URL}?${params.toString()}`);
    return response.data.data;
  },

  getMyAttendances: async (filter: AttendanceFilter) => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.startDate) params.append('startDate', filter.startDate);
    if (filter.endDate) params.append('endDate', filter.endDate);

    const response = await apiClient.get<ApiEnvelope<PagedResult<AttendanceListDto>>>(`${BASE_URL}/my?${params.toString()}`);
    return response.data.data;
  },

  getAttendanceDetails: async (id: string) => {
    const response = await apiClient.get<ApiEnvelope<AttendanceDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
  },

  checkIn: async (data: CheckInCommand) => {
    const response = await apiClient.post<ApiEnvelope<unknown>>(`${BASE_URL}/check-in`, data);
    return response.data.data;
  },

  checkOut: async (data: CheckOutCommand) => {
    const response = await apiClient.post<ApiEnvelope<unknown>>(`${BASE_URL}/check-out`, data);
    return response.data.data;
  },

  // ── New Multi-Scan & State Machine Methods ─────────────────────────────────

  recordScan: async (data: RecordScanCommand) => {
    const response = await apiClient.post<ApiEnvelope<RecordScanResponse>>(`${BASE_URL}/scan`, data);
    return response.data.data;
  },

  getAttendanceEvents: async (id: string) => {
    const response = await apiClient.get<ApiEnvelope<AttendanceEventDto[]>>(`${BASE_URL}/${id}/events`);
    return response.data.data;
  },

  getEmployeeState: async (employeeId: string) => {
    const response = await apiClient.get<ApiEnvelope<EmployeeCurrentStateDto>>(`${BASE_URL}/employee/${employeeId}/state`);
    return response.data.data;
  },

  getCurrentState: async (employeeId: string) => {
    const response = await apiClient.get<ApiEnvelope<EmployeeCurrentStateDto>>(`${BASE_URL}/employee/${employeeId}/state`);
    return response.data.data;
  },

  // ──────────────────────────────────────────────────────────────────────────

  manualAttendance: async (data: ManualAttendanceCommand) => {
    const response = await apiClient.post<ApiEnvelope<unknown>>(BASE_URL, data);
    return response.data.data;
  },

  updateAttendance: async (id: string, data: UpdateAttendanceCommand) => {
    const response = await apiClient.put<ApiEnvelope<unknown>>(`${BASE_URL}/${id}`, data);
    return response.data.data;
  },

  deleteAttendance: async (id: string) => {
    const response = await apiClient.delete<ApiEnvelope<unknown>>(`${BASE_URL}/${id}`);
    return response.data.data;
  }
};