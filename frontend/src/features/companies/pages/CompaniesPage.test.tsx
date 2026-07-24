import { describe, it, expect, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '@/test/utils';
import { CompaniesPage } from './CompaniesPage';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const mockCompaniesResponse = {
  items: [
    { id: '1', name: 'Test Company 1', registrationNumber: 'RN1', isActive: true, email: 'test1@test.com', phoneNumber: '123', foundedDate: '2020-01-01' },
    { id: '2', name: 'Test Company 2', registrationNumber: 'RN2', isActive: false, email: 'test2@test.com', phoneNumber: '456', foundedDate: '2020-01-01' },
  ],
  pagination: {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    totalCount: 2,
    hasPreviousPage: false,
    hasNextPage: false
  }
};

describe('CompaniesPage', () => {
  beforeEach(() => {
    server.use(
      http.get(`${BASE_URL}/v1/Companies`, () => {
        return HttpResponse.json(mockCompaniesResponse);
      })
    );
  });

  it('renders loading skeletons and then company list', async () => {
    renderWithProviders(<CompaniesPage />);

    expect(screen.getByText('Companies')).toBeInTheDocument();

    // Skeletons will show up, wait for data
    await waitFor(() => {
      expect(screen.getByRole('cell', { name: 'Test Company 1' })).toBeInTheDocument();
    });
    expect(screen.getByRole('cell', { name: 'Test Company 2' })).toBeInTheDocument();
  });



  it('renders error state on fetch failure', async () => {
    server.use(
      http.get(`${BASE_URL}/v1/Companies`, () => {
        return HttpResponse.json({ message: 'Error loading data' }, { status: 500 });
      })
    );

    renderWithProviders(<CompaniesPage />);

    await waitFor(() => {
      expect(screen.getByText(/error loading companies/i)).toBeInTheDocument();
    });
  });
});
