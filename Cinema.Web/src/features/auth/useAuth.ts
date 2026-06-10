import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from './authService';
import { useAuthStore } from './authStore';
import type { LoginRequest, RegisterRequest } from '../../shared/types/auth';

export function useAuth() {
  const navigate = useNavigate();
  const { user, isAuthenticated, isLoading, setUser, logoutLocal } = useAuthStore();

  const login = useCallback(
    async (request: LoginRequest) => {
      const nextUser = await authService.login(request);
      setUser(nextUser);
      navigate('/', { replace: true });
    },
    [navigate, setUser],
  );

  const register = useCallback(
    async (request: RegisterRequest) => {
      const nextUser = await authService.register(request);
      setUser(nextUser);
      navigate('/', { replace: true });
    },
    [navigate, setUser],
  );

  const logout = useCallback(async () => {
    await authService.logout();
    logoutLocal();
    navigate('/login', { replace: true });
  }, [logoutLocal, navigate]);

  return {
    user,
    isAuthenticated,
    isLoading,
    login,
    register,
    logout,
  };
}
