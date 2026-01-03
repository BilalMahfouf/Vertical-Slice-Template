import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import MainLayout from './common/layouts/MainLayout';
import Login from './features/auth/pages/Login';
import DashboardPage from './features/dashboard/DashboardPage';
import AnimalPage from './features/animals/AnimalPage';
import {QueryClientProvider, QueryClient} from '@tanstack/react-query';



const router = createBrowserRouter([
  {
    path: '/',
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
    //   {
    //     path: '/inventory',
    //     element: <InventoryPage />,
    //   },
    //   {
    //     path: '/sales',
    //     element: <SalesPage />,
    //   },
    //   {
    //     path: '/customers',
    //     element: <CustomersPage />,
    //   },
    //   {
    //     path: '/settings',
    //     element: <SettingsPage />,
    //   },
    ],
  },
]);

    const queryClient = new QueryClient();
export default function App() {
    return (
        <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
        </QueryClientProvider>
    )
}