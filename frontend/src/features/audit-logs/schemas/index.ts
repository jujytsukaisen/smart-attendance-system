import { z } from 'zod';
import { AuditActionType, AuditEntityType } from '../types';

export const auditFilterSchema = z.object({
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  userId: z.string().optional(),
  action: z.nativeEnum(AuditActionType).optional(),
  entityType: z.nativeEnum(AuditEntityType).optional(),
  searchTerm: z.string().optional(),
  page: z.number().optional(),
  pageSize: z.number().optional(),
});
