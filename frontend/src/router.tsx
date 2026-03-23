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
import ForgotPasswordPage from './features/auth/pages/ForgotPasswordPage';
import ResetPasswordPage from './features/auth/pages/ResetPasswordPage';
import AuthGuard from './features/auth/AuthGuard';
import SubscriptionGuard from './features/subscriptions/SubscriptionGuard';
import RoleGuard from './features/auth/RoleGuard';
import UserPage from './features/users/UserPage';
import CreateClinicPage from './features/clinics/pages/CreateClinicPage';
import SubscribePage from './features/subscriptions/pages/SubscribePage';
import RenewPage from './features/subscriptions/pages/RenewPage';
import PaymentSuccessPage from './features/subscriptions/pages/PaymentSuccessPage';
import PaymentFailedPage from './features/subscriptions/pages/PaymentFailedPage';

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
    element: <AuthGuard />,
    children: [
      // Onboarding routes (authenticated but no subscription check)
      {
        path: '/onboarding/create-clinic',
        element: <CreateClinicPage />,
      },
      {
        path: '/onboarding/subscribe',
        element: <SubscribePage />,
      },
      {
        path: '/onboarding/renew',
        element: <RenewPage />,
      },
      // Payment callback routes
      {
        path: '/payment/success',
        element: <PaymentSuccessPage />,
      },
      {
        path: '/payment/failed',
        element: <PaymentFailedPage />,
      },
      // Protected app routes (authenticated + subscription check)
      {
        element: <SubscriptionGuard />,
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
    ],
  },
  {
    path: '*',
    element: <Navigate to="/login" replace />,
  },
]);
