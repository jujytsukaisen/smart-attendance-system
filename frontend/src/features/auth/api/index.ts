import { apiClient } from '@/shared/api/axios';
import type { AuthResponse, LoginCredentials } from '../types';

const BASE_URL = '/v1/Auth';

export const login = async (credentials: LoginCredentials): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>(`${BASE_URL}/login`, credentials);
  return response.data;
};

export const refreshToken = async (token: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>(`${BASE_URL}/refresh-token`, { refreshToken: token });
  return response.data;
};

export const logout = async (): Promise<void> => {
  await apiClient.post(`${BASE_URL}/logout`);
};
