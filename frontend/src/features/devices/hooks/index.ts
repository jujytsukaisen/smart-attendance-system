import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { devicesApi } from '../api';
import type { DeviceFilter } from '../types';

export const deviceKeys = {
  all: ['devices'] as const,
  lists: () => [...deviceKeys.all, 'list'] as const,
  list: (filter: DeviceFilter) => [...deviceKeys.lists(), filter] as const,
  details: () => [...deviceKeys.all, 'detail'] as const,
  detail: (id: string) => [...deviceKeys.details(), id] as const,
  logs: (id: string) => [...deviceKeys.detail(id), 'logs'] as const,
};

export const useDevices = (filter: DeviceFilter) => {
  return useQuery({
    queryKey: deviceKeys.list(filter),
    queryFn: () => devicesApi.getDevices(filter),
  });
};

export const useDeviceDetails = (id: string) => {
  return useQuery({
    queryKey: deviceKeys.detail(id),
    queryFn: () => devicesApi.getDeviceById(id),
    enabled: !!id,
  });
};

export const useCreateDevice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: devicesApi.createDevice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: deviceKeys.lists() });
    },
  });
};

export const useUpdateDevice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: devicesApi.updateDevice,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: deviceKeys.lists() });
      queryClient.invalidateQueries({ queryKey: deviceKeys.detail(variables.id) });
    },
  });
};

export const useDeleteDevice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: devicesApi.deleteDevice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: deviceKeys.lists() });
    },
  });
};

export const useActivateDevice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: devicesApi.activateDevice,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: deviceKeys.lists() });
      queryClient.invalidateQueries({ queryKey: deviceKeys.detail(id) });
    },
  });
};

export const useDeactivateDevice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: devicesApi.deactivateDevice,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: deviceKeys.lists() });
      queryClient.invalidateQueries({ queryKey: deviceKeys.detail(id) });
    },
  });
};

// ==========================================
// Mocked Hardware Operations Hooks
// ==========================================

export const useSyncDevice = () => {
  return useMutation({
    mutationFn: devicesApi.syncDevice,
  });
};

export const useTestConnection = () => {
  return useMutation({
    mutationFn: devicesApi.testConnection,
  });
};

export const useRestartDevice = () => {
  return useMutation({
    mutationFn: devicesApi.restartDevice,
  });
};

export const useUploadUsers = () => {
  return useMutation({
    mutationFn: devicesApi.uploadUsers,
  });
};

export const useUploadPolicies = () => {
  return useMutation({
    mutationFn: devicesApi.uploadPolicies,
  });
};

export const useDeviceLogs = (id: string) => {
  return useQuery({
    queryKey: deviceKeys.logs(id),
    queryFn: () => devicesApi.downloadLogs(id),
    enabled: !!id,
  });
};
