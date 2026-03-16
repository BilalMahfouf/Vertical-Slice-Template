import { createBrowserRouter, Navigate } from 'react-router-dom';
import MainLayout from './common/layouts/MainLayout';
import Login from './features/auth/pages/Login';
import DashboardPage from './features/dashboard/DashboardPage';
import AnimalPage from './features/animals/AnimalPage';
import SettingPage from './features/settings/SettingPage';
import VisitPage from './features/visits/VisitPage';
import ClientPage from './features/clients/ClientPage';
import AppointmentPage from './features/appointments/AppointmentPage';
import RegisterPage from './features/auth/pages/RegisterPage';
import CreateClinicPage from './features/clinics/pages/CreateClinicPage';
import ForgotPasswordPage from './features/auth/pages/ForgotPasswordPage';
import ResetPasswordPage from './features/auth/pages/ResetPasswordPage';
import AuthGuard from './features/auth/AuthGuard';
import RoleGuard from './features/auth/RoleGuard';
import UserPage from './features/users/UserPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/login" replace />,
  },
  {
    path: '/login',
    element: <Login />,
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
    element: <AuthGuard />,
    children: [{
      element: <MainLayout />,
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
            <RoleGuard requiredRole="admin">
              <UserPage />
            </RoleGuard>
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
        },
      ],
    }],
  },
  {
    path: '*',
    element: <Navigate to="/login" replace />,
  },
]);
