import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { rolesApi } from '../api';
import type { RoleFilter } from '../types';

export const roleKeys = {
  all: ['roles'] as const,
  lists: () => [...roleKeys.all, 'list'] as const,
  list: (filter: RoleFilter) => [...roleKeys.lists(), filter] as const,
  details: () => [...roleKeys.all, 'detail'] as const,
  detail: (id: string) => [...roleKeys.details(), id] as const,
  permissions: () => ['permissions'] as const,
};

export const useRoles = (filter: RoleFilter) => {
  return useQuery({
    queryKey: roleKeys.list(filter),
    queryFn: () => rolesApi.getRoles(filter),
  });
};

export const useRoleDetails = (id: string) => {
  return useQuery({
    queryKey: roleKeys.detail(id),
    queryFn: () => rolesApi.getRoleById(id),
    enabled: !!id,
  });
};

export const useCreateRole = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.createRole,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
    },
  });
};

export const useUpdateRole = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.updateRole,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(variables.roleId) });
    },
  });
};

export const useDeleteRole = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.deleteRole,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
    },
  });
};

export const useActivateRole = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.activateRole,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(id) });
    },
  });
};

export const useDeactivateRole = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.deactivateRole,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(id) });
    },
  });
};

export const useAssignPermissions = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: rolesApi.assignPermissions,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(variables.roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.lists() });
    },
  });
};

export const usePermissions = () => {
  return useQuery({
    queryKey: roleKeys.permissions(),
    queryFn: () => rolesApi.getPermissions(),
  });
};
