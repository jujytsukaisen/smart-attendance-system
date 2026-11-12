import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/shared/types';
import type { PagedResult } from '@/shared/types';
import type {
  BranchListDto,
  BranchDetailsDto,
  GetBranchesQuery,
  CreateBranchCommand,
  UpdateBranchCommand,
} from '../types';

export const getBranches = async (query: GetBranchesQuery): Promise<PagedResult<BranchListDto>> => {
  const { data } = await apiClient.get<ApiEnvelope<PagedResult<BranchListDto>>>('/v1/Branches', {
    params: query,
  });
  return data.data;
};

export const getBranchById = async (id: string): Promise<BranchDetailsDto> => {
  const { data } = await apiClient.get<ApiEnvelope<BranchDetailsDto>>(`/v1/Branches/${id}`);
  return data.data;
};

export const createBranch = async (command: CreateBranchCommand): Promise<string> => {
  const { data } = await apiClient.post<ApiEnvelope<string>>('/v1/Branches', command);
  return data.data;
};

export const updateBranch = async (command: UpdateBranchCommand): Promise<void> => {
  await apiClient.put(`/v1/Branches/${command.branchId}`, command);
};

export const deleteBranch = async (id: string): Promise<void> => {
  await apiClient.delete(`/v1/Branches/${id}`);
};

export const restoreBranch = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Branches/${id}/restore`);
};

export const activateBranch = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Branches/${id}/activate`);
};

export const deactivateBranch = async (id: string): Promise<void> => {
  await apiClient.post(`/v1/Branches/${id}/deactivate`);
};