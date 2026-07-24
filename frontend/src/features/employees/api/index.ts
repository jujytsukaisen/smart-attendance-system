import { apiClient } from '@/shared/api/axios';
import type { PagedResult } from '@/shared/types';
import type { 
  EmployeeListDto, 
  EmployeeDetailsDto, 
  CreateEmployeeCommand, 
  UpdateEmployeeCommand,
  GetEmployeesQuery 
} from '../types';

const BASE_URL = '/v1/Employees';

export const employeesApi = {
  getEmployees: async (filter?: GetEmployeesQuery): Promise<PagedResult<EmployeeListDto>> => {
    const params = filter ? { ...filter } : undefined;
    const response = await apiClient.get<PagedResult<EmployeeListDto>>(BASE_URL, { params });
    return response.data;
  },

  getEmployeeById: async (id: string): Promise<EmployeeDetailsDto> => {
    const response = await apiClient.get<EmployeeDetailsDto>(`${BASE_URL}/${id}`);
    return response.data;
  },

  createEmployee: async (data: CreateEmployeeCommand): Promise<string> => {
    const response = await apiClient.post<string>(BASE_URL, data);
    return response.data;
  },

  updateEmployee: async (data: UpdateEmployeeCommand): Promise<void> => {
    const { employeeId, ...rest } = data;
    await apiClient.put(`${BASE_URL}/${employeeId}`, rest);
  },

  deleteEmployee: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  restoreEmployee: async (id: string): Promise<void> => {
    await apiClient.post(`${BASE_URL}/${id}/restore`);
  },

  activateEmployee: async (id: string): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${id}/activate`);
  },

  deactivateEmployee: async (id: string): Promise<void> => {
    await apiClient.put(`${BASE_URL}/${id}/deactivate`);
  },
};
