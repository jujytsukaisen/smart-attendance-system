import { apiClient } from '@/shared/api/axios';
import type { DashboardData } from '../types';

export const fetchDashboardData = async (): Promise<DashboardData> => {
  const response = await apiClient.get<DashboardData>('/v1/Dashboard');
  return response.data;
};
