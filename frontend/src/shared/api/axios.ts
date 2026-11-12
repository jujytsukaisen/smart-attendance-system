import axios, { AxiosError } from 'axios';
import type { InternalAxiosRequestConfig, AxiosResponse } from 'axios';
import { useAuthStore } from '@/features/auth/store';

// Replace with your actual backend URL or use environment variables
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = useAuthStore.getState().token;
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response Interceptor
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  async (error: AxiosError<any>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Handle 401 Unauthorized globally
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      const refreshToken = useAuthStore.getState().refreshToken;

      if (refreshToken) {
        try {
          // Attempt to refresh the token using a separate axios instance to avoid interceptor loops
          const refreshResponse = await axios.post(`${BASE_URL}/v1/Auth/refresh-token`, {
            refreshToken,
          });
          
          const { token, refreshToken: newRefreshToken } = refreshResponse.data;
          
          // Update the store
          useAuthStore.getState().updateToken(token, newRefreshToken);
          
          // Update the original request header and retry
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return apiClient(originalRequest);
        } catch (refreshError) {
          // Refresh failed, logout
          useAuthStore.getState().logout();
          window.location.href = '/auth/login';
          return Promise.reject(refreshError);
        }
      } else {
        // No refresh token available, logout
        useAuthStore.getState().logout();
        window.location.href = '/auth/login';
      }
    }
    
    // Handle 403 Forbidden globally
    if (error.response?.status === 403) {
      window.location.href = '/unauthorized';
    }

    // Extract error details from API response
    const data = error.response?.data;
    if (data) {
      if (data?.extensions?.errors && Array.isArray(data.extensions.errors)) {
        error.message = data.extensions.errors.join('\n');
      } else if (data?.errors && typeof data.errors === 'object') {
        // Built-in ValidationProblemDetails mapping
        const errorMessages = Object.values(data.errors).flat().join('\n');
        if (errorMessages) {
          error.message = errorMessages;
        }
      } else if (data?.detail) {
        error.message = data.detail;
      } else if (data?.message) {
        error.message = data.message;
      } else if (data?.title) {
        error.message = data.title;
      }
    } else if (error.response?.status === 500) {
      error.message = 'An unexpected internal server error occurred.';
    }

    return Promise.reject(error);
  }
);
