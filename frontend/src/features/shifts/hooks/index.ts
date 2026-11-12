import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  getShifts,
  getShiftById,
  createShift,
  updateShift,
  deleteShift,
  activateShift,
  deactivateShift,
  restoreShift,
  getWorkSchedules,
} from '../api';
import type { ShiftFilter, CreateShiftCommand, UpdateShiftCommand } from '../types';

export const shiftKeys = {
  all: ['shifts'] as const,
  lists: () => [...shiftKeys.all, 'list'] as const,
  list: (filters: ShiftFilter) => [...shiftKeys.lists(), { filters }] as const,
  details: () => [...shiftKeys.all, 'detail'] as const,
  detail: (id: string) => [...shiftKeys.details(), id] as const,
  workSchedules: () => ['workSchedules'] as const,
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

export const useShifts = (filter: ShiftFilter) => {
  return useQuery({
    queryKey: shiftKeys.list(filter),
    queryFn: () => getShifts(filter),
  });
};

export const useShift = (id: string) => {
  return useQuery({
    queryKey: shiftKeys.detail(id),
    queryFn: () => getShiftById(id),
    enabled: !!id,
  });
};

export const useWorkSchedules = () => {
  return useQuery({
    queryKey: shiftKeys.workSchedules(),
    queryFn: () => getWorkSchedules(1, 100),
  });
};

export const useCreateShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: CreateShiftCommand) => createShift(command),
    onSuccess: () => {
      toast.success('Shift created successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to create shift'));
    },
  });
};

export const useUpdateShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (command: UpdateShiftCommand) => updateShift(command),
    onSuccess: (_, variables) => {
      toast.success('Shift updated successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
      queryClient.invalidateQueries({ queryKey: shiftKeys.detail(variables.shiftId) });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to update shift'));
    },
  });
};

export const useDeleteShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteShift(id),
    onSuccess: () => {
      toast.success('Shift deleted successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to delete shift'));
    },
  });
};

export const useActivateShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateShift(id),
    onSuccess: (_, id) => {
      toast.success('Shift activated successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
      queryClient.invalidateQueries({ queryKey: shiftKeys.detail(id) });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to activate shift'));
    },
  });
};

export const useDeactivateShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateShift(id),
    onSuccess: (_, id) => {
      toast.success('Shift deactivated successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
      queryClient.invalidateQueries({ queryKey: shiftKeys.detail(id) });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to deactivate shift'));
    },
  });
};

export const useRestoreShift = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => restoreShift(id),
    onSuccess: () => {
      toast.success('Shift restored successfully');
      queryClient.invalidateQueries({ queryKey: shiftKeys.lists() });
    },
    onError: (error: any) => {
      toast.error(getErrorMessage(error, 'Failed to restore shift'));
    },
  });
};