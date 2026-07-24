import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { BranchesPage } from './BranchesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockBranchesResponse = {
  items: [
    { id: '1', name: 'Main Branch', code: 'MB1', isActive: true, companyId: '1', isHeadOffice: true },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('BranchesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Branches`, () => {
        return HttpResponse.json(mockBranchesResponse);
      }),
      http.get(`${BASE_URL}/v1/Companies/lookup`, () => {
        return HttpResponse.json([{ id: '1', name: 'Company 1' }]);
      })
    );
  });

  it('renders loading skeletons and then branch list', async () => {
    renderWithProviders(<BranchesPage />);
    expect(screen.getByText('Branches')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'Main Branch' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Branches`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<BranchesPage />);

    await waitFor(() => {
      expect(screen.getByText(/error loading branches/i)).toBeInTheDocument();
    });
  });
});
