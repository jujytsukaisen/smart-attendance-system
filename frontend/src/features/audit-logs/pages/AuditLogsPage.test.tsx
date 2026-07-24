import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { AuditLogsPage } from './AuditLogsPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockAuditLogsResponse = {
  items: [
    { id: '1', action: 'Create', entityName: 'User', entityId: '123', userId: '1', userName: 'Admin', createdAt: '2023-10-25T10:00:00Z', ipAddress: '127.0.0.1', browser: 'Chrome', result: 'Success' },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('AuditLogsPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/AuditLogs`, () => {
        return HttpResponse.json(mockAuditLogsResponse);
      })
    );
  });

  it('renders loading skeletons and then audit log list', async () => {
    renderWithProviders(<AuditLogsPage />);
    expect(screen.getByText('Audit Logs')).toBeInTheDocument();
    
    await waitFor(() => {
      const row = screen.getByRole('row', { name: /Create/i });
      expect(within(row).getByText('User')).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/AuditLogs`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<AuditLogsPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
