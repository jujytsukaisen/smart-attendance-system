import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { LeavesPage } from './LeavesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockLeavesResponse = {
  items: [
    { id: '1', employeeId: 'EMP-123', leaveType: 'Annual', status: 'Pending', startDate: '2023-10-25', endDate: '2023-10-26' },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('LeavesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Leaves`, () => {
        return HttpResponse.json(mockLeavesResponse);
      })
    );
  });

  it('renders loading skeletons and then leave list', async () => {
    renderWithProviders(<LeavesPage />);
    expect(screen.getByText('Leave Management')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: /Annual/i })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Leaves`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<LeavesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
