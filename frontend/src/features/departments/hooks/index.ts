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

const getErrorMessage = (error: any, fallback: string): string => {
  const data = error.response?.data;
  if (data?.detail) return data.detail;
  if (data?.errors) {
    if (Array.isArray(data.errors) && data.errors.length > 0) return data.errors[0];
    if (typeof data.errors === 'object') {
      const firstKey = Object.keys(data.errors)[0];
      if (firstKey && Array.isArray(data.errors[firstKey]) && data.errors[firstKey].length > 0) {
        return data.errors[firstKey][0];
      }
    }
  }
  if (data?.message) return data.message;
  if (data?.title) return data.title;
  return error.message || fallback;
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
      toast.error(getErrorMessage(error, 'Failed to create department'));
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
      toast.error(getErrorMessage(error, 'Failed to update department'));
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
      toast.error(getErrorMessage(error, 'Failed to delete department'));
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
      toast.error(getErrorMessage(error, 'Failed to restore department'));
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
      toast.error(getErrorMessage(error, 'Failed to activate department'));
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
      toast.error(getErrorMessage(error, 'Failed to deactivate department'));
    },
  });
};
