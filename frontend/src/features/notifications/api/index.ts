import type {
  NotificationDto,
  NotificationDetailsDto,
  NotificationListDto,
  NotificationFilterDto
} from '../types';
import type { ApiEnvelope } from '@/shared/types';
import { apiClient } from '@/shared/api/axios';

const BASE_URL = '/v1/Notifications';

export const notificationsApi = {
  getNotifications: async (filter?: NotificationFilterDto): Promise<NotificationListDto> => {
    const params: Record<string, unknown> = {};
    if (filter?.status) params['status'] = filter.status;
    if (filter?.type) params['type'] = filter.type;
    if (filter?.priority) params['priority'] = filter.priority;
    if (filter?.searchTerm) params['searchTerm'] = filter.searchTerm;
    if (filter?.page) params['pageNumber'] = filter.page;
    if (filter?.pageSize) params['pageSize'] = filter.pageSize;

    const response = await apiClient.get<ApiEnvelope<{ items: NotificationDto[]; pagination: { totalCount: number }; unreadCount: number }>>(
      BASE_URL,
      { params }
    );
    const raw = response.data.data;
    return {
      items: raw.items ?? [],
      totalCount: raw.pagination?.totalCount ?? 0,
      unreadCount: raw.unreadCount ?? 0,
    };
  },

  getNotification: async (id: string): Promise<NotificationDetailsDto> => {
    const response = await apiClient.get<ApiEnvelope<NotificationDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
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
    const response = await apiClient.get<ApiEnvelope<number>>(`${BASE_URL}/unread-count`);
    return response.data.data;
  }
};