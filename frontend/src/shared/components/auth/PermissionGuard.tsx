import type { ReactNode } from 'react';
import { usePermission } from '@/features/auth/hooks';

interface PermissionGuardProps {
  children: ReactNode;
  permission?: string;
  permissions?: string[];
  role?: string;
  roles?: string[];
  requireAll?: boolean;
  fallback?: ReactNode;
}

export function PermissionGuard({
  children,
  permission,
  permissions,
  role,
  roles,
  requireAll = false,
  fallback = null,
}: PermissionGuardProps) {
  const { hasPermission, hasAnyPermission, hasRole, hasAnyRole } = usePermission();

  let isAuthorized = true;

  if (permission && !hasPermission(permission)) {
    isAuthorized = false;
  }

  if (permissions && permissions.length > 0) {
    if (requireAll) {
      if (!permissions.every(p => hasPermission(p))) isAuthorized = false;
    } else {
      if (!hasAnyPermission(permissions)) isAuthorized = false;
    }
  }

  if (role && !hasRole(role)) {
    isAuthorized = false;
  }

  if (roles && roles.length > 0) {
    if (requireAll) {
      if (!roles.every(r => hasRole(r))) isAuthorized = false;
    } else {
      if (!hasAnyRole(roles)) isAuthorized = false;
    }
  }

  if (!isAuthorized) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
