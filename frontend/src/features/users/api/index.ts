import { apiClient } from '@/shared/api/axios';
import type { PagedResult } from '@/shared/types';
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

    const response = await apiClient.get<PagedResult<UserListDto>>(`${BASE_URL}?${params.toString()}`);
    return response.data;
  },

  getUserById: async (id: string): Promise<UserDetailsDto> => {
    const response = await apiClient.get<UserDetailsDto>(`${BASE_URL}/${id}`);
    return response.data;
  },

  createUser: async (data: CreateUserCommand): Promise<string> => {
    const response = await apiClient.post<string>(BASE_URL, data);
    return response.data;
  },

  updateUser: async (data: UpdateUserCommand): Promise<void> => {
    const { userId, ...rest } = data;
    await apiClient.put(`${BASE_URL}/${userId}`, rest);
  },

  deleteUser: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  activateUser: async (id: string): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${id}/activate`);
  },

  suspendUser: async (id: string): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${id}/suspend`);
  },

  assignRoles: async (data: AssignRolesCommand): Promise<void> => {
    const { userId, roles } = data;
    await apiClient.put(`${BASE_URL}/${userId}/roles`, { roles });
  },

  resetPassword: async (data: ResetPasswordCommand): Promise<void> => {
    const { userId, ...rest } = data;
    await apiClient.post(`${BASE_URL}/${userId}/reset-password`, rest);
  },

  changePassword: async (data: ChangePasswordCommand): Promise<void> => {
    const { userId, ...rest } = data;
    await apiClient.post(`${BASE_URL}/${userId}/change-password`, rest);
  },
};
