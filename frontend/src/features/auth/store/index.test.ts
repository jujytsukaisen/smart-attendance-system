import { describe, it, expect, beforeEach } from 'vitest';
import { useAuthStore } from './index';

describe('useAuthStore', () => {
  beforeEach(() => {
    // Reset store before each test
    useAuthStore.setState({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
    });
  });

  it('initializes with default values', () => {
    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.token).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });

  it('sets authentication state correctly', () => {
    const user = { id: '1', firstName: 'John', lastName: 'Doe', email: 'test@example.com', roles: [] };

    useAuthStore.getState().setAuth(user, 'access-token', 'refresh-token');

    const state = useAuthStore.getState();
    expect(state.user).toEqual(user);
    expect(state.token).toBe('access-token');
    expect(state.refreshToken).toBe('refresh-token');
    expect(state.isAuthenticated).toBe(true);
  });

  it('updates token correctly', () => {
    useAuthStore.getState().updateToken('new-token', 'new-refresh-token');

    const state = useAuthStore.getState();
    expect(state.token).toBe('new-token');
    expect(state.refreshToken).toBe('new-refresh-token');
  });

  it('clears state on logout', () => {
    const user = { id: '1', firstName: 'John', lastName: 'Doe', email: 'test@example.com', roles: [] };
    useAuthStore.getState().setAuth(user, 'token', 'refresh');

    useAuthStore.getState().logout();

    const state = useAuthStore.getState();
    expect(state.user).toBeNull();
    expect(state.token).toBeNull();
    expect(state.isAuthenticated).toBe(false);
  });
});
