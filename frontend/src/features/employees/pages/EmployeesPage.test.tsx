import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { EmployeesPage } from './EmployeesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockEmployeesResponse = {
  items: [
    { id: '1', companyId: '1', branchId: '1', departmentId: '1', employeeNumber: 'EMP01', firstName: 'John', lastName: 'Doe', email: 'john@example.com', jobTitle: 'Developer', employeeStatus: 1 },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('EmployeesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Employees`, () => {
        return HttpResponse.json(mockEmployeesResponse);
      }),
      http.get(`${BASE_URL}/v1/Departments/lookup`, () => {
        return HttpResponse.json([{ id: '1', name: 'HR' }]);
      })
    );
  });

  it('renders loading skeletons and then employee list', async () => {
    renderWithProviders(<EmployeesPage />);
    expect(screen.getByText('Employees')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'John Doe' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Employees`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<EmployeesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
