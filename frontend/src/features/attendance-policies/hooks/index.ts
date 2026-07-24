import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getAttendancePolicies,
  getAttendancePolicy,
  createAttendancePolicy,
  updateAttendancePolicy,
  deleteAttendancePolicy,
  restoreAttendancePolicy,
  activateAttendancePolicy,
  deactivateAttendancePolicy,
} from '../api';
import type { 
  AttendancePolicyFilter 
} from '../types';

export const ATTENDANCE_POLICIES_KEYS = {
  all: ['attendancePolicies'] as const,
  lists: () => [...ATTENDANCE_POLICIES_KEYS.all, 'list'] as const,
  list: (filter: AttendancePolicyFilter) => [...ATTENDANCE_POLICIES_KEYS.lists(), filter] as const,
  details: () => [...ATTENDANCE_POLICIES_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...ATTENDANCE_POLICIES_KEYS.details(), id] as const,
};

export const useAttendancePolicies = (filter: AttendancePolicyFilter) => {
  return useQuery({
    queryKey: ATTENDANCE_POLICIES_KEYS.list(filter),
    queryFn: () => getAttendancePolicies(filter),
  });
};

export const useAttendancePolicy = (id: string) => {
  return useQuery({
    queryKey: ATTENDANCE_POLICIES_KEYS.detail(id),
    queryFn: () => getAttendancePolicy(id),
    enabled: !!id,
  });
};

export const useCreateAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createAttendancePolicy,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
    },
  });
};

export const useUpdateAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateAttendancePolicy,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
      queryClient.invalidateQueries({
        queryKey: ATTENDANCE_POLICIES_KEYS.detail(variables.id),
      });
    },
  });
};

export const useDeleteAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteAttendancePolicy,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
    },
  });
};

export const useRestoreAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: restoreAttendancePolicy,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
    },
  });
};

export const useActivateAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: activateAttendancePolicy,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.details() });
    },
  });
};

export const useDeactivateAttendancePolicy = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deactivateAttendancePolicy,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_POLICIES_KEYS.details() });
    },
  });
};
