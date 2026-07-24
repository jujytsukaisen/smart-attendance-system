import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { AttendancePage } from './AttendancePage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockAttendanceResponse = {
  items: [
    { id: '1', employeeId: 'EMP-0123456789', checkInTime: '2023-10-25T09:00:00Z', attendanceStatus: 2 },
  ],
  pagination: { pageNumber: 1, pageSize: 10, totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false }
};

describe('AttendancePage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Attendances`, () => {
        return HttpResponse.json(mockAttendanceResponse);
      })
    );
  });

  it('renders loading skeletons and then attendance list', async () => {
    renderWithProviders(<AttendancePage />);
    expect(screen.getByText('Attendance')).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByRole('cell', { name: /EMP-0123/i })).toBeInTheDocument();
    });
  });

  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Attendances`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<AttendancePage />);

    await waitFor(() => {
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });
});
