import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { UsersPage } from './UsersPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockUsersResponse = {
  items: [
    { id: '1', username: 'admin', email: 'admin@example.com', isActive: true, roles: ['Admin'] },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('UsersPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Users`, () => {
        return HttpResponse.json(mockUsersResponse);
      }),
      http.get(`${BASE_URL}/v1/Roles/lookup`, () => {
        return HttpResponse.json([{ id: '1', name: 'Admin' }]);
      })
    );
  });

  it('renders loading skeletons and then user list', async () => {
    renderWithProviders(<UsersPage />);
    expect(screen.getByText('Users')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'admin@example.com' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Users`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<UsersPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
