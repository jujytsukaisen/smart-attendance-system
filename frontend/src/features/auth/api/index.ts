import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope, AuthResponse, LoginCredentials } from '../types';

const BASE_URL = '/v1/Auth';

export const login = async (credentials: LoginCredentials): Promise<AuthResponse> => {
  const response = await apiClient.post<ApiEnvelope<AuthResponse>>(`${BASE_URL}/login`, credentials);
  return response.data.data; // فك الغلاف: { data, statusCode, isSuccess } -> data
};

export const refreshToken = async (token: string): Promise<AuthResponse> => {
  const response = await apiClient.post<ApiEnvelope<AuthResponse>>(`${BASE_URL}/refresh-token`, { refreshToken: token });
  return response.data.data;
};

export const logout = async (): Promise<void> => {
  await apiClient.post(`${BASE_URL}/logout`);
};