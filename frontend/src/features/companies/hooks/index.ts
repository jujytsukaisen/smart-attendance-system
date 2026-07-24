import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getCompanies,
  getCompanyById,
  createCompany,
  updateCompany,
  deleteCompany,
  restoreCompany,
  activateCompany,
  deactivateCompany,
} from '../api';
import type { GetCompaniesQuery } from '../types';

export const companiesKeys = {
  all: ['companies'] as const,
  lists: () => [...companiesKeys.all, 'list'] as const,
  list: (filters: GetCompaniesQuery) => [...companiesKeys.lists(), filters] as const,
  details: () => [...companiesKeys.all, 'detail'] as const,
  detail: (id: string) => [...companiesKeys.details(), id] as const,
};

export const useCompanies = (query: GetCompaniesQuery) => {
  return useQuery({
    queryKey: companiesKeys.list(query),
    queryFn: () => getCompanies(query),
  });
};

export const useCompany = (id: string) => {
  return useQuery({
    queryKey: companiesKeys.detail(id),
    queryFn: () => getCompanyById(id),
    enabled: !!id,
  });
};

export const useCreateCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createCompany,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
    },
  });
};

export const useUpdateCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: updateCompany,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: companiesKeys.detail(variables.companyId) });
    },
  });
};

export const useDeleteCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteCompany,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: companiesKeys.detail(id) });
    },
  });
};

export const useRestoreCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: restoreCompany,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: companiesKeys.detail(id) });
    },
  });
};

export const useActivateCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: activateCompany,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: companiesKeys.detail(id) });
    },
  });
};

export const useDeactivateCompany = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deactivateCompany,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: companiesKeys.lists() });
      queryClient.invalidateQueries({ queryKey: companiesKeys.detail(id) });
    },
  });
};
