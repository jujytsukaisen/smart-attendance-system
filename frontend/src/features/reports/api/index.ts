import { apiClient } from '@/shared/api/axios';
import type { 
  ReportFilterDto, 
  DashboardStatisticsDto, 
  LateEmployeesReportDto,
  AbsenceReportDto,
  LeaveReportDto
} from '../types';

const BASE_URL = '/v1/Reports';

export const reportsApi = {
  getDashboardStatistics: async (filter?: ReportFilterDto): Promise<DashboardStatisticsDto> => {
    const params = filter ? { ...filter } : undefined;
    const response = await apiClient.get<DashboardStatisticsDto>(`${BASE_URL}/dashboard-statistics`, { params });
    return response.data;
  },

  getLateEmployeesReport: async (filter: ReportFilterDto): Promise<LateEmployeesReportDto[]> => {
    const response = await apiClient.get<LateEmployeesReportDto[]>(`${BASE_URL}/late-employees`, { params: filter });
    return response.data;
  },

  getAbsenceReport: async (filter: ReportFilterDto): Promise<AbsenceReportDto[]> => {
    const response = await apiClient.get<AbsenceReportDto[]>(`${BASE_URL}/absence`, { params: filter });
    return response.data;
  },

  getLeaveReport: async (filter: ReportFilterDto): Promise<LeaveReportDto[]> => {
    const response = await apiClient.get<LeaveReportDto[]>(`${BASE_URL}/leave`, { params: filter });
    return response.data;
  },

  exportReport: async (type: 'pdf' | 'csv' | 'excel', filter: ReportFilterDto): Promise<void> => {
    await apiClient.post(`${BASE_URL}/export`, { type, ...filter }, { responseType: 'blob' });
  }
};
