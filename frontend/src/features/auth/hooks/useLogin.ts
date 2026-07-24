import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { login } from '../api';
import { useAuthStore } from '../store';

export const useLogin = () => {
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      setAuth(data.user, data.token, data.refreshToken);
      toast.success('Logged in successfully');
      navigate('/');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.title || 'Login failed');
    },
  });
};
