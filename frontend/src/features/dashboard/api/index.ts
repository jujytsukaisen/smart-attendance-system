import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/features/auth/types';
import type { DashboardStatisticsDto, MyDashboardDto } from '../types';

const BASE_URL = '/v1/Dashboard';

export const getDashboardStatistics = async (): Promise<DashboardStatisticsDto> => {
    const response = await apiClient.get<ApiEnvelope<DashboardStatisticsDto>>(BASE_URL);
    return response.data.data;
};

export const getMyDashboard = async (): Promise<MyDashboardDto> => {
    const response = await apiClient.get<ApiEnvelope<MyDashboardDto>>(`${BASE_URL}/my`);
    return response.data.data;
};