import { Search, Menu } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';
import { useTranslation } from 'react-i18next';
import NotificationDropdown from '@/features/notifications/NotificationDropdown';

interface TopNavigationProps {
  onToggleSidebar: () => void;
}

export default function TopNavigation({ onToggleSidebar }: TopNavigationProps) {
  const { i18n } = useTranslation();
  const isRtl = i18n.language === 'ar';

  return (
    <nav className={cn(
      'fixed top-0 end-0 z-40 bg-white border-b border-slate-100 h-16 start-0 lg:start-80 transition-all duration-300',
    )}>
      <div className='flex items-center justify-between h-full px-6'>
        {/* Left side - Menu button (mobile/tablet) and Search */}
        <div className='flex items-center gap-4 flex-1'>
          {/* Menu button for mobile/tablet */}
          <Button
            variant="ghost"
            size="icon"
            onClick={onToggleSidebar}
            className='lg:hidden hover:bg-slate-100'
          >
            <Menu className='h-5 w-5 text-slate-700' />
          </Button>

          {/* Search Bar - hidden on mobile, visible on tablet and desktop */}
          <div className='hidden md:block flex-1 max-w-2xl'>
            <div className='relative'>
              <Search className={cn(
                'absolute top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400',
                isRtl ? 'end-3' : 'start-3'
              )} />
              <Input
                type='text'
                placeholder='Search patients, appointments, clients...'
                className={cn(
                  'bg-slate-50 border-slate-200 h-11 transition-all focus:bg-white rounded-xl',
                  isRtl ? 'pe-10' : 'ps-10'
                )}
              />
            </div>
          </div>
        </div>

        {/* Right Side - Notifications */}
        <div className='flex items-center gap-2'>
          {/* Notifications */}
          <NotificationDropdown />
        </div>
      </div>
    </nav>
  );
}
