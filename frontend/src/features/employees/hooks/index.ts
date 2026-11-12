import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { employeesApi } from '../api';
import type { GetEmployeesQuery, CreateEmployeeCommand, UpdateEmployeeCommand } from '../types';

export const employeeKeys = {
  all: ['employees'] as const,
  lists: () => [...employeeKeys.all, 'list'] as const,
  list: (filters: GetEmployeesQuery) => [...employeeKeys.lists(), filters] as const,
  details: () => [...employeeKeys.all, 'detail'] as const,
  detail: (id: string) => [...employeeKeys.details(), id] as const,
};

export const useEmployees = (filters: GetEmployeesQuery, enabled: boolean = true) => {
  return useQuery({
    queryKey: employeeKeys.list(filters),
    queryFn: () => employeesApi.getEmployees(filters),
    enabled,
  });
};

export const useEmployee = (id: string) => {
  return useQuery({
    queryKey: employeeKeys.detail(id),
    queryFn: () => employeesApi.getEmployeeById(id),
    enabled: !!id,
  });
};

export const useCreateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateEmployeeCommand) => employeesApi.createEmployee(command),
    onSuccess: () => {
      toast.success('Employee created successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to create employee';
      toast.error(message);
    },
  });
};

export const useUpdateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: UpdateEmployeeCommand) => employeesApi.updateEmployee(command),
    onSuccess: (_, variables) => {
      toast.success('Employee updated successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
      queryClient.invalidateQueries({ queryKey: employeeKeys.detail(variables.employeeId) });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to update employee';
      toast.error(message);
    },
  });
};

export const useDeleteEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => employeesApi.deleteEmployee(id),
    onSuccess: () => {
      toast.success('Employee deleted successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to delete employee';
      toast.error(message);
    },
  });
};

export const useRestoreEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => employeesApi.restoreEmployee(id),
    onSuccess: () => {
      toast.success('Employee restored successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to restore employee';
      toast.error(message);
    },
  });
};

export const useActivateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => employeesApi.activateEmployee(id),
    onSuccess: (_, id) => {
      toast.success('Employee activated successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
      queryClient.invalidateQueries({ queryKey: employeeKeys.detail(id) });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to activate employee';
      toast.error(message);
    },
  });
};

export const useDeactivateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => employeesApi.deactivateEmployee(id),
    onSuccess: (_, id) => {
      toast.success('Employee deactivated successfully');
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
      queryClient.invalidateQueries({ queryKey: employeeKeys.detail(id) });
    },
    onError: (error: any) => {
      const message = error.message || 'Failed to deactivate employee';
      toast.error(message);
    },
  });
};
