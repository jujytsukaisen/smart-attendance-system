import { useQuery } from '@tanstack/react-query';
import { auditLogsApi } from '../api';
import type { AuditFilterDto } from '../types';

export const auditKeys = {
  all: ['auditLogs'] as const,
  lists: () => [...auditKeys.all, 'list'] as const,
  list: (filter?: AuditFilterDto) => [...auditKeys.lists(), filter] as const,
  details: () => [...auditKeys.all, 'detail'] as const,
  detail: (id: string) => [...auditKeys.details(), id] as const,
};

export const useAuditLogs = (filter?: AuditFilterDto) => {
  return useQuery({
    queryKey: auditKeys.list(filter),
    queryFn: () => auditLogsApi.getLogs(filter),
  });
};

export const useAuditLog = (id: string) => {
  return useQuery({
    queryKey: auditKeys.detail(id),
    queryFn: () => auditLogsApi.getLogDetails(id),
    enabled: !!id,
  });
};
