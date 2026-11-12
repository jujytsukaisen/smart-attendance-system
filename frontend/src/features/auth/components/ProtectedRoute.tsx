import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store';

interface ProtectedRouteProps {
  allowedRoles?: string[];
  requireAdmin?: boolean;
}

export const ProtectedRoute = ({ allowedRoles, requireAdmin }: ProtectedRouteProps) => {
  const { isAuthenticated, user } = useAuthStore();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/auth/login" state={{ from: location }} replace />;
  }

  if (requireAdmin && user) {
    const isAdmin = user.roles?.includes('Admin');
    if (!isAdmin) {
      return <Navigate to="/dashboard" replace />;
    }
  }

  if (allowedRoles && user) {
    const hasRole = user.roles.some((role) => allowedRoles.includes(role));
    if (!hasRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return <Outlet />;
};
