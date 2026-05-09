// ============================================================
// App.tsx - Entry Point với Instant Hydration
// ============================================================

import React, { useEffect } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Spin } from 'antd';
import adminRoutes from './routes/admin.routes';
import customerRoutes from './routes/customer.routes';
import { useAuthStore } from './store/useAuthStore';
import { authService } from './services/auth/authService';
import { onAuthChange, hasTokens, setCachedUser } from './utils/tokenStorage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

const router = createBrowserRouter([
  ...customerRoutes,
  ...adminRoutes,
  {
    path: '*',
    element: <div style={{ padding: 50, textAlign: 'center', fontSize: 20 }}>404 - Not Found</div>,
  }
]);

const App: React.FC = () => {
  const { user, isLoading, setUser, setLoading, setBackgroundVerifying, logout, hydrate } = useAuthStore();

  useEffect(() => {
    // ---- Background Verify ----
    const verifyAuth = async () => {
      if (!hasTokens()) {
        setLoading(false);
        return;
      }

      setBackgroundVerifying(true);

      try {
        // response = ApiResponse<UserInfoResponse>
        const response = await authService.getCurrentUser();
        const userInfo = response.data; // Unwrap từ Standard Response Wrapper

        const freshUser = {
          id: userInfo.userId,
          email: userInfo.email,
          fullName: userInfo.fullName,
          roles: userInfo.roles,
        };

        setCachedUser(freshUser);
        setUser(freshUser);
      } catch (error) {
        console.warn('[App] Background auth verify failed:', error);
        // If both access and refresh tokens are expired, interceptor will trigger force logout
        // Otherwise, interceptor will handle refresh automatically
        // Don't clear user immediately to avoid UI flicker
      } finally {
        setBackgroundVerifying(false);
        setLoading(false);
      }
    };

    verifyAuth();
  }, [setUser, setLoading, setBackgroundVerifying]);

  useEffect(() => {
    // ---- Force Logout Listener ----
    const handleForceLogout = () => {
      logout();
    };

    window.addEventListener('auth:forceLogout', handleForceLogout);
    return () => window.removeEventListener('auth:forceLogout', handleForceLogout);
  }, [logout]);

  useEffect(() => {
    // ---- Tab Sync ----
    const unsubscribe = onAuthChange((isAuthenticated) => {
      if (!isAuthenticated) {
        logout();
      } else {
        hydrate();
      }
    });

    return unsubscribe;
  }, [logout, hydrate]);

  // Splash screen chỉ khi chưa có user VÀ đang loading
  if (isLoading && !user) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100vh',
        flexDirection: 'column',
        gap: 16
      }}>
        <Spin size="large" />
        <div style={{ fontSize: 16, color: '#666' }}>Đang tải CinemaHub...</div>
      </div>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  );
};

export default App;
