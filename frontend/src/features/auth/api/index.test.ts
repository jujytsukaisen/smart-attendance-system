import { describe, it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';
import { login, refreshToken, logout } from './index';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

describe('Auth API', () => {
  it('login sends correct credentials and returns data', async () => {
    const mockResponse = {
      token: 'test-token',
      refreshToken: 'test-refresh',
      user: { id: '1', firstName: 'John', lastName: 'Doe', email: 'test@example.com', roles: [] },
    };

    server.use(
      http.post(`${BASE_URL}/v1/Auth/login`, async ({ request }) => {
        const body = await request.json() as any;
        expect(body.email).toBe('test@example.com');
        expect(body.password).toBe('password');
        return HttpResponse.json(mockResponse);
      })
    );

    const result = await login({ email: 'test@example.com', password: 'password' });
    expect(result).toEqual(mockResponse);
  });

  it('refreshToken sends token and returns new tokens', async () => {
    const mockResponse = {
      token: 'new-token',
      refreshToken: 'new-refresh',
      user: { id: '1', firstName: 'John', lastName: 'Doe', email: 'test@example.com', roles: [] },
    };

    server.use(
      http.post(`${BASE_URL}/v1/Auth/refresh-token`, async ({ request }) => {
        const body = await request.json() as any;
        expect(body.refreshToken).toBe('old-refresh');
        return HttpResponse.json(mockResponse);
      })
    );

    const result = await refreshToken('old-refresh');
    expect(result).toEqual(mockResponse);
  });

  it('logout resolves successfully', async () => {
    server.use(
      http.post(`${BASE_URL}/v1/Auth/logout`, () => {
        return HttpResponse.json({}, { status: 200 });
      })
    );

    await expect(logout()).resolves.toBeUndefined();
  });
});
