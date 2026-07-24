import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { RolesPage } from './RolesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockRolesResponse = {
  items: [
    { id: '1', name: 'Administrator', description: 'Full access', isActive: true, permissions: [] },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('RolesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Roles`, () => {
        return HttpResponse.json(mockRolesResponse);
      })
    );
  });

  it('renders loading skeletons and then role list', async () => {
    renderWithProviders(<RolesPage />);
    expect(screen.getByText('Roles')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'Administrator' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Roles`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<RolesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
