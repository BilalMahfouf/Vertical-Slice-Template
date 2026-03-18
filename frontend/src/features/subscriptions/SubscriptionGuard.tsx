import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { Loader2 } from 'lucide-react';
import { useCurrentUser } from '@/features/auth/useCurrentUser';

/**
 * SubscriptionGuard wraps protected app routes.
 * - While fetching user data → shows a loading spinner.
 * - If clinic is not completed → redirects to /onboarding/create-clinic.
 * - If subscription is not active → redirects to /onboarding/subscribe.
 * - Otherwise → renders child routes via <Outlet />.
 */
export default function SubscriptionGuard() {
  const { data: user, isLoading } = useCurrentUser();
  const location = useLocation();

  // Show loading spinner while fetching user data
  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }
  console.log('Current user data:', user);

  if (user?.role === 'Admin') {
    console.log('Admin user detected, bypassing subscription checks');
    return <Outlet />;
  }
  console.log('Checking subscription status for user:', user?.subscriptionStatus);
 // Prevent redirect loops - if already on an onboarding page, allow access
  const isOnOnboardingPage = location.pathname.startsWith('/onboarding');
  if (isOnOnboardingPage) {
    console.log('Already on onboarding page, allowing access to avoid redirect loop');
    return <Outlet />;
  }
  console.log('Not on onboarding page, performing subscription checks');

  // Check clinic info completion
  if (user?.clinicInfromationCompleted === false) {
    console.log('Clinic information incomplete, redirecting to clinic info page');
    return <Navigate to="/onboarding/create-clinic" replace />;
  }
  console.log('Clinic information completed, checking subscription status');

  console.log('User subscription status:', user?.subscriptionStatus);
  // Check subscription status (allow "Active" or "Trialing")
  if (
    user &&
    user.subscriptionStatus !== "Active" &&
    user.subscriptionStatus !== "Trialing"
  ) {
    return <Navigate to="/onboarding/subscribe" replace />;
  } 
  console.log('Subscription status is active or trialing, allowing access to protected content');


  // All checks passed - render protected content
  return <Outlet />;
}
