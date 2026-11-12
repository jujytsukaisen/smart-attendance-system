import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  RoleListDto,
  RoleDto,
  CreateRoleCommand,
  UpdateRoleCommand,
  AssignPermissionsCommand,
  RoleFilter,
  PermissionDto
} from '../types';

const ROLES_URL = '/v1/Roles';
const PERMISSIONS_URL = '/v1/Permissions';

export const rolesApi = {
  getRoles: async (filter: RoleFilter): Promise<PagedResult<RoleListDto>> => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);

    const response = await apiClient.get<ApiEnvelope<PagedResult<RoleListDto>>>(`${ROLES_URL}?${params.toString()}`);
    return response.data.data;
  },

  getRoleById: async (id: string): Promise<RoleDto> => {
    const response = await apiClient.get<ApiEnvelope<RoleDto>>(`${ROLES_URL}/${id}`);
    return response.data.data;
  },

  createRole: async (data: CreateRoleCommand): Promise<string> => {
    const response = await apiClient.post<ApiEnvelope<string>>(ROLES_URL, data);
    return response.data.data;
  },

  updateRole: async (data: UpdateRoleCommand): Promise<void> => {
    await apiClient.put(`${ROLES_URL}/${data.roleId}`, data);
  },

  deleteRole: async (id: string): Promise<void> => {
    await apiClient.delete(`${ROLES_URL}/${id}`);
  },

  // Status management — POST (not PUT)
  activateRole: async (id: string): Promise<void> => {
    await apiClient.post(`${ROLES_URL}/${id}/activate`);
  },

  deactivateRole: async (id: string): Promise<void> => {
    await apiClient.post(`${ROLES_URL}/${id}/deactivate`);
  },

  // Permission assignment — POST to /assign-permissions
  // Backend expects { roleId: string, permissionIds: string[] }
  assignPermissions: async (data: AssignPermissionsCommand): Promise<void> => {
    const { roleId, permissions } = data;
    await apiClient.post(`${ROLES_URL}/${roleId}/assign-permissions`, { roleId, permissionIds: permissions });
  },

  getPermissions: async (): Promise<PermissionDto[]> => {
    const response = await apiClient.get<ApiEnvelope<PagedResult<PermissionDto>>>(`${PERMISSIONS_URL}?pageNumber=1&pageSize=1000`);
    return response.data.data.items;
  }
};