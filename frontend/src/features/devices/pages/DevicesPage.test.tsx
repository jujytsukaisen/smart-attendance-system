import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { DevicesPage } from './DevicesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockDevicesResponse = {
  items: [
    { 
      id: '1', 
      name: 'Main Gate Device', 
      code: 'DEV-01',
      serialNumber: 'SN123', 
      deviceType: 1, 
      deviceStatus: 2, 
      connectionStatus: 2, 
      isActive: true 
    },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('DevicesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/AttendanceDevices`, () => {
        return HttpResponse.json(mockDevicesResponse);
      }),
      http.get(`${BASE_URL}/v1/Branches/lookup`, () => {
        return HttpResponse.json([{ id: '1', name: 'Main Branch' }]);
      })
    );
  });

  it('renders loading skeletons and then device list', async () => {
    renderWithProviders(<DevicesPage />);
    expect(screen.getByText('Devices')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: /Main Gate Device/i })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/AttendanceDevices`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<DevicesPage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
