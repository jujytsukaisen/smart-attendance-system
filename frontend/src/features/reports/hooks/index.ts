import { useQuery, useMutation } from '@tanstack/react-query';
import { reportsApi } from '../api';
import type { ReportFilterDto } from '../types';

export const reportKeys = {
  all: ['reports'] as const,
  dashboard: (filter?: ReportFilterDto) => [...reportKeys.all, 'dashboard', filter] as const,
  lateEmployees: (filter: ReportFilterDto) => [...reportKeys.all, 'late', filter] as const,
  absence: (filter: ReportFilterDto) => [...reportKeys.all, 'absence', filter] as const,
  leave: (filter: ReportFilterDto) => [...reportKeys.all, 'leave', filter] as const,
};

export const useDashboardStatistics = (filter?: ReportFilterDto) => {
  return useQuery({
    queryKey: reportKeys.dashboard(filter),
    queryFn: () => reportsApi.getDashboardStatistics(filter),
  });
};

export const useLateEmployeesReport = (filter: ReportFilterDto) => {
  return useQuery({
    queryKey: reportKeys.lateEmployees(filter),
    queryFn: () => reportsApi.getLateEmployeesReport(filter),
  });
};

export const useAbsenceReport = (filter: ReportFilterDto) => {
  return useQuery({
    queryKey: reportKeys.absence(filter),
    queryFn: () => reportsApi.getAbsenceReport(filter),
  });
};

export const useLeaveReport = (filter: ReportFilterDto) => {
  return useQuery({
    queryKey: reportKeys.leave(filter),
    queryFn: () => reportsApi.getLeaveReport(filter),
  });
};

export const useExportReport = () => {
  return useMutation({
    mutationFn: ({ type, filter }: { type: 'pdf' | 'csv' | 'excel', filter: ReportFilterDto }) => 
      reportsApi.exportReport(type, filter),
  });
};
