import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { leavesApi } from '../api';
import type { LeaveFilter } from '../types';

export const leaveKeys = {
  all: ['leaves'] as const,
  lists: () => [...leaveKeys.all, 'list'] as const,
  list: (filter: LeaveFilter) => [...leaveKeys.lists(), filter] as const,
  details: () => [...leaveKeys.all, 'detail'] as const,
  detail: (id: string) => [...leaveKeys.details(), id] as const,
};

export const useLeaves = (filter: LeaveFilter) => {
  return useQuery({
    queryKey: leaveKeys.list(filter),
    queryFn: () => leavesApi.getLeaves(filter),
  });
};

export const useLeaveDetails = (id: string) => {
  return useQuery({
    queryKey: leaveKeys.detail(id),
    queryFn: () => leavesApi.getLeaveById(id),
    enabled: !!id,
  });
};

export const useCreateLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.createLeave,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
    },
  });
};

export const useUpdateLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.updateLeave,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
      queryClient.invalidateQueries({ queryKey: leaveKeys.detail(variables.leaveId) });
    },
  });
};

export const useDeleteLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.deleteLeave,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
    },
  });
};

export const useApproveLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.approveLeave,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
      queryClient.invalidateQueries({ queryKey: leaveKeys.detail(variables.leaveId) });
    },
  });
};

export const useRejectLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.rejectLeave,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
      queryClient.invalidateQueries({ queryKey: leaveKeys.detail(variables.leaveId) });
    },
  });
};

export const useCancelLeave = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: leavesApi.cancelLeave,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leaveKeys.lists() });
      queryClient.invalidateQueries({ queryKey: leaveKeys.detail(variables.leaveId) });
    },
  });
};
