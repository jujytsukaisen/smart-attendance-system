import { useAuthStore } from '../store';

export function usePermission() {
  const user = useAuthStore((state) => state.user);

  const hasPermission = (permission: string) => {
    if (!user || !user.permissions) return false;
    return user.permissions.includes(permission);
  };

  const hasRole = (role: string) => {
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  };

  const hasAnyPermission = (permissions: string[]) => {
    if (!user || !user.permissions) return false;
    return permissions.some(p => user.permissions!.includes(p));
  };

  const hasAnyRole = (roles: string[]) => {
    if (!user || !user.roles) return false;
    return roles.some(r => user.roles!.includes(r));
  };

  return {
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAnyRole,
  };
}
