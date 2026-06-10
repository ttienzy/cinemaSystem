import { useEffect } from 'react';
import { App as AntdApp } from 'antd';
import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { authService } from '../features/auth/authService';
import { useAuthStore } from '../features/auth/authStore';
import { bindApiMessage } from '../shared/api/axiosClient';
import { hasTokens, onAuthChange } from '../shared/auth/tokenStorage';
import { queryClient } from './queryClient';
import { router } from './router';

function App() {
  const { message } = AntdApp.useApp();
  const { setUser, setLoading, setBackgroundVerifying, logoutLocal, hydrate } = useAuthStore();

  useEffect(() => {
    bindApiMessage(message);
  }, [message]);

  useEffect(() => {
    async function verifySession() {
      if (!hasTokens()) {
        setLoading(false);
        return;
      }

      setBackgroundVerifying(true);
      try {
        const user = await authService.getCurrentUser();
        setUser(user);
      } catch {
        setLoading(false);
      } finally {
        setBackgroundVerifying(false);
      }
    }

    void verifySession();
  }, [setBackgroundVerifying, setLoading, setUser]);

  useEffect(() => {
    const handleForceLogout = () => logoutLocal();
    window.addEventListener('auth:forceLogout', handleForceLogout);
    return () => window.removeEventListener('auth:forceLogout', handleForceLogout);
  }, [logoutLocal]);

  useEffect(() => {
    return onAuthChange((hasAuth) => {
      if (hasAuth) hydrate();
      else logoutLocal();
    });
  }, [hydrate, logoutLocal]);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
}

export default App;
