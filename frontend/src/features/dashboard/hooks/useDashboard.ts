import { useQuery } from '@tanstack/react-query';
import { getDashboardStatistics, getMyDashboard } from '../api';

export const useDashboard = (enabled: boolean = true) => {
    return useQuery({
        queryKey: ['dashboard', 'statistics'],
        queryFn: getDashboardStatistics,
        staleTime: 60 * 1000,
        refetchOnWindowFocus: false,
        enabled,
    });
};

export const useMyDashboard = () => {
    return useQuery({
        queryKey: ['dashboard', 'my'],
        queryFn: getMyDashboard,
        staleTime: 60 * 1000,
        refetchOnWindowFocus: false,
    });
};