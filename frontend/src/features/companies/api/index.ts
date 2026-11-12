import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  CompanyListDto,
  CompanyDetailsDto,
  GetCompaniesQuery,
  CreateCompanyCommand,
  UpdateCompanyCommand,
} from '../types';

export const getCompanies = async (query: GetCompaniesQuery): Promise<PagedResult<CompanyListDto>> => {
  const { data } = await apiClient.get<ApiEnvelope<PagedResult<CompanyListDto>>>('/v1/Companies', {
    params: query,
  });
  return data.data;
};

export const getCompanyById = async (id: string): Promise<CompanyDetailsDto> => {
  const { data } = await apiClient.get<ApiEnvelope<CompanyDetailsDto>>(`/v1/Companies/${id}`);
  return data.data;
};

export const createCompany = async (command: CreateCompanyCommand): Promise<string> => {
  const { data } = await apiClient.post<ApiEnvelope<string>>('/v1/Companies', command);
  return data.data;
};

export const updateCompany = async (command: UpdateCompanyCommand): Promise<void> => {
  await apiClient.put(`/v1/Companies/${command.companyId}`, command);
};

export const deleteCompany = async (id: string): Promise<void> => {
  await apiClient.delete(`/v1/Companies/${id}`);
};

export const restoreCompany = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Companies/${id}/restore`);
};

export const activateCompany = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Companies/${id}/activate`);
};

export const deactivateCompany = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Companies/${id}/deactivate`);
};