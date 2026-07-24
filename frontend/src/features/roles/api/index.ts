import { apiClient } from '@/shared/api/axios';
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

    const response = await apiClient.get<PagedResult<RoleListDto>>(`${ROLES_URL}?${params.toString()}`);
    return response.data;
  },

  getRoleById: async (id: string): Promise<RoleDto> => {
    const response = await apiClient.get<RoleDto>(`${ROLES_URL}/${id}`);
    return response.data;
  },

  createRole: async (data: CreateRoleCommand): Promise<string> => {
    const response = await apiClient.post<string>(ROLES_URL, data);
    return response.data;
  },

  updateRole: async (data: UpdateRoleCommand): Promise<void> => {
    const { roleId, ...rest } = data;
    await apiClient.put(`${ROLES_URL}/${roleId}`, rest);
  },

  deleteRole: async (id: string): Promise<void> => {
    await apiClient.delete(`${ROLES_URL}/${id}`);
  },

  activateRole: async (id: string): Promise<void> => {
    await apiClient.put(`${ROLES_URL}/${id}/activate`);
  },

  deactivateRole: async (id: string): Promise<void> => {
    await apiClient.put(`${ROLES_URL}/${id}/deactivate`);
  },

  assignPermissions: async (data: AssignPermissionsCommand): Promise<void> => {
    const { roleId, permissions } = data;
    await apiClient.put(`${ROLES_URL}/${roleId}/permissions`, { permissions });
  },

  getPermissions: async (): Promise<PermissionDto[]> => {
    const response = await apiClient.get<PagedResult<PermissionDto>>(`${PERMISSIONS_URL}?pageNumber=1&pageSize=1000`);
    return response.data.items;
  }
};
