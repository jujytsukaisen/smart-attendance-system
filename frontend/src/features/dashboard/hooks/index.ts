import { useQuery } from '@tanstack/react-query';
import { fetchDashboardData } from '../api';
import type { DashboardData } from '../types';

export const useDashboardData = () => {
  return useQuery<DashboardData, Error>({
    queryKey: ['dashboardData'],
    queryFn: fetchDashboardData,
    staleTime: 1000 * 60 * 5, // Data is fresh for 5 minutes
  });
};
