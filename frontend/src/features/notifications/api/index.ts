import type {
  NotificationDetailsDto,
  NotificationListDto,
  NotificationFilterDto
} from '../types';
import { apiClient } from '@/shared/api/axios';

const BASE_URL = '/v1/Notifications';

export const notificationsApi = {
  getNotifications: async (filter?: NotificationFilterDto): Promise<NotificationListDto> => {
    const params = filter ? { ...filter } : undefined;
    const response = await apiClient.get<NotificationListDto>(BASE_URL, { params });
    return response.data;
  },

  getNotification: async (id: string): Promise<NotificationDetailsDto> => {
    const response = await apiClient.get<NotificationDetailsDto>(`${BASE_URL}/${id}`);
    return response.data;
  },

  markAsRead: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.post(`${BASE_URL}/read-all`);
  },

  deleteNotification: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  deleteReadNotifications: async (): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/read`);
  },

  getUnreadCount: async (): Promise<number> => {
    const response = await apiClient.get<number>(`${BASE_URL}/unread-count`);
    return response.data;
  }
};
