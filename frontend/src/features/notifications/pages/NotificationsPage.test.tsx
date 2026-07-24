import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { NotificationsPage } from './NotificationsPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockNotificationsResponse = {
  items: [
    { id: '1', recipientUserId: '2', title: 'System Update', message: 'Update completed', isRead: false, createdAt: '2023-10-25T10:00:00Z', type: 'System', priority: 'Low' },
  ],
  totalCount: 1,
  unreadCount: 1
};

describe('NotificationsPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Notifications`, () => {
        return HttpResponse.json(mockNotificationsResponse);
      })
    );
  });

  it('renders loading skeletons and then notification list', async () => {
    renderWithProviders(<NotificationsPage />);
    expect(screen.getByText('Notifications')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'System Update' })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Notifications`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<NotificationsPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
