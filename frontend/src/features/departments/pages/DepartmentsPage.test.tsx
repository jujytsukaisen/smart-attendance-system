import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { DepartmentsPage } from './DepartmentsPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockDepartmentsResponse = {
  items: [
    { id: '1', companyId: '1', name: 'HR Department', code: 'HR01', isActive: true, branchId: '1', branchName: 'Main Branch' },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('DepartmentsPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Departments`, () => {
        return HttpResponse.json(mockDepartmentsResponse);
      }),
      http.get(`${BASE_URL}/v1/Branches/lookup`, () => {
        return HttpResponse.json([{ id: '1', name: 'Main Branch' }]);
      })
    );
  });

  it('renders loading skeletons and then department list', async () => {
    renderWithProviders(<DepartmentsPage />);
    expect(screen.getByText('Departments')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'HR Department' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Departments`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<DepartmentsPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
