import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import MainLayout from './common/layouts/MainLayout';
import Login from './features/auth/pages/Login';
import DashboardPage from './features/dashboard/DashboardPage';
import AnimalPage from './features/animals/AnimalPage';
import {QueryClientProvider, QueryClient} from '@tanstack/react-query';
import SettingPage from './features/visits/VisitPage';
import VisitPage from './features/visits/VisitPage';
import ClientPage from './features/clients/ClientPage';
import { Toaster } from './components/ui/sonner';
import AppointmentPage from './features/appointments/AppointmentPage';



const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
    // errorElement: <NotFoundPage />,
  },
  {
    element: <MainLayout/>, // wrap all pages with sidebar
    // errorElement: <NotFoundPage />,
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
  },
]);

    const queryClient = new QueryClient();
export default function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <RouterProvider router={router} />
            <Toaster />
        </QueryClientProvider>
    )
}