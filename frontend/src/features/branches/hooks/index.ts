import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getBranches,
  getBranchById,
  createBranch,
  updateBranch,
  deleteBranch,
  restoreBranch,
  activateBranch,
  deactivateBranch,
} from '../api';
import type { GetBranchesQuery } from '../types';

export const branchesKeys = {
  all: ['branches'] as const,
  lists: () => [...branchesKeys.all, 'list'] as const,
  list: (filters: GetBranchesQuery) => [...branchesKeys.lists(), filters] as const,
  details: () => [...branchesKeys.all, 'detail'] as const,
  detail: (id: string) => [...branchesKeys.details(), id] as const,
};

export const useBranches = (query: GetBranchesQuery) => {
  return useQuery({
    queryKey: branchesKeys.list(query),
    queryFn: () => getBranches(query),
  });
};

export const useBranch = (id: string) => {
  return useQuery({
    queryKey: branchesKeys.detail(id),
    queryFn: () => getBranchById(id),
    enabled: !!id,
  });
};

export const useCreateBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createBranch,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
    },
  });
};

export const useUpdateBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: updateBranch,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: branchesKeys.detail(variables.branchId) });
    },
  });
};

export const useDeleteBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteBranch,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: branchesKeys.detail(id) });
    },
  });
};

export const useRestoreBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: restoreBranch,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: branchesKeys.detail(id) });
    },
  });
};

export const useActivateBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: activateBranch,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: branchesKeys.detail(id) });
    },
  });
};

export const useDeactivateBranch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deactivateBranch,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: branchesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: branchesKeys.detail(id) });
    },
  });
};
