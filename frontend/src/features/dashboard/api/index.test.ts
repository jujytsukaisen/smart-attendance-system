import { describe, it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { fetchDashboardData } from './index';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

describe('Dashboard API', () => {
  it('fetchDashboardData returns data correctly', async () => {
    const mockData = {
      totalEmployees: 100,
      presentToday: 90,
      absentToday: 10,
      lateToday: 5,
      onLeaveToday: 2,
      recentActivities: []
    };

    server.use(
      http.get(`${BASE_URL}/v1/Dashboard`, () => {
        return HttpResponse.json(mockData);
      })
    );

    const data = await fetchDashboardData();
    expect(data).toEqual(mockData);
  });
});
