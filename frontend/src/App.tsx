import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import MainLayout from './common/layouts/MainLayout';
import Login from './features/auth/pages/Login';
import DashboardPage from './features/dashboard/DashboardPage';
import AnimalPage from './features/animals/AnimalPage';
import { QueryClientProvider, QueryClient } from '@tanstack/react-query';
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