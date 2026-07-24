import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { useLogin } from "./index";
import { createWrapper } from '@/test/utils';
import { useAuthStore } from '../store';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

describe('Auth Hooks', () => {
  it('useLogin mutation succeeds and updates store', async () => {
    const mockUser = { id: '1', firstName: 'John', lastName: 'Doe', email: 'test@test.com', roles: [] };

    server.use(
      http.post(`${BASE_URL}/v1/Auth/login`, () => {
        return HttpResponse.json({
          token: 'token123',
          refreshToken: 'refresh123',
          user: mockUser
        });
      })
    );

    const { result } = renderHook(() => useLogin(), {
      wrapper: createWrapper()
    });

    result.current.mutate({ email: 'test@test.com', password: 'password' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    const store = useAuthStore.getState();
    expect(store.isAuthenticated).toBe(true);
    expect(store.token).toBe('token123');
  });
});
