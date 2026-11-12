import type {
  AuditLogDetailsDto,
  AuditLogListDto,
  AuditFilterDto
} from '../types';
import type { ApiEnvelope } from '@/shared/types';
import { apiClient } from '@/shared/api/axios';

const BASE_URL = '/v1/AuditLogs';

export const auditLogsApi = {
  getLogs: async (filter?: AuditFilterDto): Promise<AuditLogListDto> => {
    const params = filter ? { ...filter } : undefined;
    const response = await apiClient.get<ApiEnvelope<AuditLogListDto>>(BASE_URL, { params });
    return response.data.data;
  },

  getLogDetails: async (id: string): Promise<AuditLogDetailsDto> => {
    const response = await apiClient.get<ApiEnvelope<AuditLogDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
  }
};