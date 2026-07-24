import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { DashboardPage } from './DashboardPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockDashboardData = {
  stats: {
    totalEmployees: 100,
    totalCompanies: 2,
    totalBranches: 5,
    totalDepartments: 10,
    todaysAttendance: 90,
    lateEmployees: 2,
    activeLeaves: 3,
  },
  recentActivities: [],
  notifications: []
};

describe('DashboardPage', () => {
  it('renders dashboard data correctly', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Dashboard`, () => {
        return HttpResponse.json(mockDashboardData);
      }),
      http.get(`${BASE_URL}/v1/Reports/dashboard`, () => {
        return HttpResponse.json({
          totalEmployees: 100,
          presentToday: 90,
          lateToday: 2,
          onLeaveToday: 3,
          absentToday: 5,
        });
      })
    );

    renderWithProviders(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('100')).toBeInTheDocument();
    });

    expect(screen.getByText('90')).toBeInTheDocument();
    expect(screen.getAllByText('2').length).toBeGreaterThan(0);
  });

  it('renders error state on API failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Dashboard`, () => {
        return HttpResponse.json({ title: 'Failed to load' }, { status: 500 });
      }),
      http.get(`${BASE_URL}/v1/Reports/dashboard`, () => {
        return HttpResponse.json({ title: 'Failed to load' }, { status: 500 });
      })
    );

    renderWithProviders(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText(/Failed to load dashboard/i)).toBeInTheDocument();
    });
  });
});
