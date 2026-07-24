import { apiClient } from '@/shared/api/axios';
import type { PagedResult } from '@/shared/types';
import type { 
  DepartmentListDto, 
  DepartmentDetailsDto, 
  CreateDepartmentCommand, 
  UpdateDepartmentCommand, 
  DepartmentFilter 
} from '../types';

const BASE_URL = '/v1/Departments';

export const getDepartments = async (params: DepartmentFilter): Promise<PagedResult<DepartmentListDto>> => {
  const response = await apiClient.get<PagedResult<DepartmentListDto>>(BASE_URL, { params });
  return response.data;
};

export const getDepartmentById = async (id: string): Promise<DepartmentDetailsDto> => {
  const response = await apiClient.get<DepartmentDetailsDto>(`${BASE_URL}/${id}`);
  return response.data;
};

export const createDepartment = async (command: CreateDepartmentCommand): Promise<string> => {
  const response = await apiClient.post<string>(BASE_URL, command);
  return response.data;
};

export const updateDepartment = async (command: UpdateDepartmentCommand): Promise<void> => {
  await apiClient.put(`${BASE_URL}/${command.departmentId}`, command);
};

export const deleteDepartment = async (id: string): Promise<void> => {
  await apiClient.delete(`${BASE_URL}/${id}`);
};

export const restoreDepartment = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/restore`);
};

export const activateDepartment = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/activate`);
};

export const deactivateDepartment = async (id: string): Promise<void> => {
  await apiClient.post(`${BASE_URL}/${id}/deactivate`);
};
