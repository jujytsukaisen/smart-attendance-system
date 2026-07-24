import { z } from 'zod';
import { NotificationType, NotificationPriority, NotificationStatus } from '../types';

export const notificationFilterSchema = z.object({
  type: z.nativeEnum(NotificationType).optional(),
  priority: z.nativeEnum(NotificationPriority).optional(),
  status: z.nativeEnum(NotificationStatus).optional(),
  searchTerm: z.string().optional(),
  page: z.number().optional(),
  pageSize: z.number().optional(),
});
