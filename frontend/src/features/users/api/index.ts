import { apiClient } from '@/shared/api/axios';
import type { PagedResult, ApiEnvelope } from '@/shared/types';
import type {
  UserListDto,
  UserDetailsDto,
  CreateUserCommand,
  UpdateUserCommand,
  AssignRolesCommand,
  ResetPasswordCommand,
  ChangePasswordCommand,
  UserFilter
} from '../types';

const BASE_URL = '/v1/Users';

export const usersApi = {
  getUsers: async (filter: UserFilter): Promise<PagedResult<UserListDto>> => {
    const params = new URLSearchParams();
    if (filter.pageNumber) params.append('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize) params.append('pageSize', filter.pageSize.toString());
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.isActive !== undefined) params.append('isActive', filter.isActive.toString());

    const response = await apiClient.get<ApiEnvelope<PagedResult<UserListDto>>>(`${BASE_URL}?${params.toString()}`);
    return response.data.data;
  },

  getUserById: async (id: string): Promise<UserDetailsDto> => {
    const response = await apiClient.get<ApiEnvelope<UserDetailsDto>>(`${BASE_URL}/${id}`);
    return response.data.data;
  },

  createUser: async (data: CreateUserCommand): Promise<string> => {
    const response = await apiClient.post<ApiEnvelope<string>>(BASE_URL, data);
    return response.data.data;
  },

  updateUser: async (data: UpdateUserCommand): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${data.userId}`, data);
  },

  deleteUser: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  // Status management — POST (not PUT)
  activateUser: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/activate`);
  },

  suspendUser: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/suspend`);
  },

  // Role assignment — POST to /assign-roles with { roles } body
  assignRoles: async (data: AssignRolesCommand): Promise<void> => {
    const { userId, roles } = data;
    await apiClient.post(`${BASE_URL}/${userId}/assign-roles`, { userId, roles });
  },

  // Password management
  resetPassword: async (data: ResetPasswordCommand): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${data.userId}/reset-password`, data);
  },

  changePassword: async (data: ChangePasswordCommand): Promise<void> => {
    await apiClient.post(`${BASE_URL}/change-password`, data);
  },
};