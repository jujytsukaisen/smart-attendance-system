import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
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
    const response = await apiClient.get<ApiEnvelope<DashboardStatisticsDto>>(`${BASE_URL}/dashboard-statistics`, { params });
    return response.data.data;
  },

  getLateEmployeesReport: async (filter: ReportFilterDto): Promise<LateEmployeesReportDto[]> => {
    const response = await apiClient.get<ApiEnvelope<LateEmployeesReportDto[]>>(`${BASE_URL}/late-employees`, { params: filter });
    return response.data.data;
  },

  getAbsenceReport: async (filter: ReportFilterDto): Promise<AbsenceReportDto[]> => {
    const response = await apiClient.get<ApiEnvelope<AbsenceReportDto[]>>(`${BASE_URL}/absence`, { params: filter });
    return response.data.data;
  },

  getLeaveReport: async (filter: ReportFilterDto): Promise<LeaveReportDto[]> => {
    const response = await apiClient.get<ApiEnvelope<LeaveReportDto[]>>(`${BASE_URL}/leave`, { params: filter });
    return response.data.data;
  },

  exportReport: async (type: 'pdf' | 'csv' | 'excel', filter: ReportFilterDto): Promise<void> => {
    const response = await apiClient.post(`${BASE_URL}/export`, { type, ...filter }, { responseType: 'blob' });
    
    if (type === 'pdf') {
      const text = await response.data.text();
      let htmlContent = text;
      try {
        const parsed = JSON.parse(text);
        if (parsed && typeof parsed.data === 'string') {
          htmlContent = parsed.data;
        }
      } catch {
        // Raw HTML string
      }
      
      const printWindow = window.open('', '_blank');
      if (printWindow) {
        printWindow.document.open();
        printWindow.document.write(htmlContent);
        printWindow.document.close();
      } else {
        const htmlBlob = new Blob([htmlContent], { type: 'text/html; charset=utf-8' });
        const url = window.URL.createObjectURL(htmlBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Attendance_Report_${new Date().toISOString().slice(0, 10)}.html`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      }
      return;
    }

    const headerContentType = response.headers['content-type'];
    const contentType = typeof headerContentType === 'string' ? headerContentType : (type === 'excel' ? 'application/vnd.ms-excel; charset=utf-8' : 'text/csv; charset=utf-8');
    const blob = new Blob([response.data], { type: contentType });
    
    let filename = `Attendance_Report_${new Date().toISOString().slice(0, 10)}.${type === 'excel' ? 'xls' : 'csv'}`;
    const disposition = response.headers['content-disposition'];
    if (disposition) {
      const match = disposition.match(/filename\*?=['"]?(?:UTF-\d['"]*)?([^;\r\n"']*)['"]?/i);
      if (match && match[1]) {
        filename = decodeURIComponent(match[1]);
      }
    }

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    link.remove();

    setTimeout(() => {
      window.URL.revokeObjectURL(url);
    }, 1000);
  }
};