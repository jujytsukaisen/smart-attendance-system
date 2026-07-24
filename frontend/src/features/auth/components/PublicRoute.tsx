import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store';

export const PublicRoute = () => {
  const { isAuthenticated } = useAuthStore();

  if (isAuthenticated) {
    // If user is already authenticated, don't let them see the login page
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
};
