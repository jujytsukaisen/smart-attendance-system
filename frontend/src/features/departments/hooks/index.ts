import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  getDepartments,
  getDepartmentById,
  createDepartment,
  updateDepartment,
  deleteDepartment,
  restoreDepartment,
  activateDepartment,
  deactivateDepartment,
} from '../api';
import type { DepartmentFilter, CreateDepartmentCommand, UpdateDepartmentCommand } from '../types';

export const departmentKeys = {
  all: ['departments'] as const,
  lists: () => [...departmentKeys.all, 'list'] as const,
  list: (filters: DepartmentFilter) => [...departmentKeys.lists(), filters] as const,
  details: () => [...departmentKeys.all, 'detail'] as const,
  detail: (id: string) => [...departmentKeys.details(), id] as const,
};

export const useDepartments = (filters: DepartmentFilter) => {
  return useQuery({
    queryKey: departmentKeys.list(filters),
    queryFn: () => getDepartments(filters),
  });
};

export const useDepartment = (id: string) => {
  return useQuery({
    queryKey: departmentKeys.detail(id),
    queryFn: () => getDepartmentById(id),
    enabled: !!id,
  });
};

export const useCreateDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateDepartmentCommand) => createDepartment(command),
    onSuccess: () => {
      toast.success('Department created successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to create department';
      toast.error(message);
    },
  });
};

export const useUpdateDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: UpdateDepartmentCommand) => updateDepartment(command),
    onSuccess: (_, variables) => {
      toast.success('Department updated successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
      queryClient.invalidateQueries({ queryKey: departmentKeys.detail(variables.departmentId) });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to update department';
      toast.error(message);
    },
  });
};

export const useDeleteDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteDepartment(id),
    onSuccess: () => {
      toast.success('Department deleted successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to delete department';
      toast.error(message);
    },
  });
};

export const useRestoreDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => restoreDepartment(id),
    onSuccess: () => {
      toast.success('Department restored successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to restore department';
      toast.error(message);
    },
  });
};

export const useActivateDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateDepartment(id),
    onSuccess: (_, id) => {
      toast.success('Department activated successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
      queryClient.invalidateQueries({ queryKey: departmentKeys.detail(id) });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to activate department';
      toast.error(message);
    },
  });
};

export const useDeactivateDepartment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateDepartment(id),
    onSuccess: (_, id) => {
      toast.success('Department deactivated successfully');
      queryClient.invalidateQueries({ queryKey: departmentKeys.lists() });
      queryClient.invalidateQueries({ queryKey: departmentKeys.detail(id) });
    },
    onError: (error: any) => {
      const message = error.response?.data?.title || 'Failed to deactivate department';
      toast.error(message);
    },
  });
};
