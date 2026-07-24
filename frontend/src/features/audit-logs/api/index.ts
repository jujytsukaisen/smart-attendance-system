import type {
  AuditLogDetailsDto,
  AuditLogListDto,
  AuditFilterDto
} from '../types';
import { apiClient } from '@/shared/api/axios';

const BASE_URL = '/v1/AuditLogs';

export const auditLogsApi = {
  getLogs: async (filter?: AuditFilterDto): Promise<AuditLogListDto> => {
    const params = filter ? { ...filter } : undefined;
    const response = await apiClient.get<AuditLogListDto>(BASE_URL, { params });
    return response.data;
  },

  getLogDetails: async (id: string): Promise<AuditLogDetailsDto> => {
    const response = await apiClient.get<AuditLogDetailsDto>(`${BASE_URL}/${id}`);
    return response.data;
  }
};
