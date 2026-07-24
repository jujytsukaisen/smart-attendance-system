import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { AttendancePoliciesPage } from './AttendancePoliciesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockPoliciesResponse = {
  items: [
    { id: '1', companyId: '1', name: 'Standard Policy', code: 'POL1', attendanceMode: 1, isActive: true, isDefault: true },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('AttendancePoliciesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/attendance-policies`, () => {
        return HttpResponse.json(mockPoliciesResponse);
      })
    );
  });

  it('renders loading skeletons and then policy list', async () => {
    renderWithProviders(<AttendancePoliciesPage />);
    expect(screen.getByText('Attendance Policies')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: /Standard Policy/i })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/attendance-policies`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<AttendancePoliciesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
