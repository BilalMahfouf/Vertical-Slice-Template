import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import MainLayout from './common/layouts/MainLayout';
import Login from './features/auth/pages/Login';
import DashboardPage from './features/dashboard/DashboardPage';
import AnimalPage from './features/animals/AnimalPage';
import { QueryClientProvider, QueryClient, useQuery } from '@tanstack/react-query';
import SettingPage from './features/settings/SettingPage';
import VisitPage from './features/visits/VisitPage';
import ClientPage from './features/clients/ClientPage';
import { Toaster } from './components/ui/sonner';
import AppointmentPage from './features/appointments/AppointmentPage';
import RegisterPage from './features/auth/pages/RegisterPage';
import CreateClinicPage from './features/clinics/pages/CreateClinicPage';
import ForgotPasswordPage from './features/auth/pages/ForgotPasswordPage';
import ResetPasswordPage from './features/auth/pages/ResetPasswordPage';
import { AuthProvider } from './features/auth/AuthProvider';
import AuthGuard from './features/auth/AuthGuard';
import type { AxiosError } from 'axios';
import { Loader2 } from 'lucide-react';
import api from './lib/api/api';
import UserPage from './features/users/UserPage';
import type { JSX } from 'react';
import i18n from './lib/i18n';
import i18nKeyContainer from './lib/i18n/keyContainer';

type CurrentUserResponse = {
  role: string;
};

function AdminOnlyRoute({ children }: { children: JSX.Element }) {
  const { data, isLoading } = useQuery({
    queryKey: ['auth-me'],
    queryFn: async () => {
      const response = await api.get<CurrentUserResponse>('/auth/me');
      if (response.status !== 200) {
        throw new Error(i18n.t(i18nKeyContainer.errors.user.fetchCurrentUser));
      }
      return response.data;
    },
    staleTime: 60000,
    retry: false,
  });

  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  const isAdmin = data?.role?.toLowerCase() === 'admin';

  if (!isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}

const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/login" replace />,
  },
  {
    path: '/login',
    element: <Login />,
    // errorElement: <NotFoundPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
  {
    path: '/forgot-password',
    element: <ForgotPasswordPage />,
  },
  {
    path: '/reset-password',
    element: <ResetPasswordPage />,
  },
  {
    path: '/create-clinic',
    element: <CreateClinicPage />,
  },
  {
    element: <AuthGuard />,   // gate: redirects to /login when not authenticated
    children: [{
      element: <MainLayout />, // wrap all pages with sidebar
      children: [
      {
        path: '/dashboard',
        element: <DashboardPage />,
      },
      {
        path: '/animals',
        element: <AnimalPage />,
      },
      {
        path: '/clients',
        element: <ClientPage />,
      },
      {
        path: '/users',
        element: (
          <AdminOnlyRoute>
            <UserPage />
          </AdminOnlyRoute>
        ),
      },
      {
        path: '/appointments',
        element: <AppointmentPage />,
      },
      {
        path: '/visits',
        element: <VisitPage />,
      },
      {
        path: '/settings',
        element: <SettingPage />,
      }
      ],
    }],
  },
]);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (failureCount, error) => {
        // Don't retry 401 errors — the interceptor handles token refresh
        if ((error as AxiosError)?.response?.status === 401) return false;
        return failureCount < 2;
      },
    },
    mutations: {
      retry: false,
    },
  },
});

export default function App() {
  return (
    <AuthProvider>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
        <Toaster />
      </QueryClientProvider>
    </AuthProvider>
  );
}