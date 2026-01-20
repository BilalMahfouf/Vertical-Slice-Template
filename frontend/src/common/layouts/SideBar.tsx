import { LayoutDashboard, Stethoscope, Users, Calendar, Clock, Bell, Settings, LogOut, ChevronRight, Languages } from "lucide-react";
import SideBarLink from "./SideBarLink";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useMutation } from "@tanstack/react-query";
import { authApi } from "@/lib/api/auth";
import { useNavigate } from "react-router-dom";

const navigationItems = [
  { pathname: "/dashboard", key: i18nKeyContainer.dashboard, icon: LayoutDashboard },
  { pathname: "/animals", key: i18nKeyContainer.animals, icon: Stethoscope },
  { pathname: "/clients", key: i18nKeyContainer.clients, icon: Users },
  { pathname: "/appointments", key: i18nKeyContainer.appointments, icon: Calendar },
  { pathname: "/visits", key: i18nKeyContainer.visits, icon: Clock },
  { pathname: "/notifications", key: i18nKeyContainer.notifications, icon: Bell },
  { pathname: "/settings", key: i18nKeyContainer.settings, icon: Settings },
];

export default function Sidebar({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === 'ar';
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: authApi.logout,
    onSuccess: ()=>{
        navigate('/login');
    }
  })

    const handleLogOut = () => {
      mutation.mutate();
    }


  return (
    <>
      {/* Sidebar */}
      <div
        className={cn(
          "fixed top-0 bottom-0 w-80 bg-white border-e border-slate-200 z-50 transform transition-all duration-500 ease-in-out",
          isOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0",
          isRtl ? "end-0 lg:end-auto lg:start-0" : "start-0"
        )}
      >
        <div className="flex flex-col h-full bg-white">
          {/* Header */}
          <div className="p-6 bg-white">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 bg-primary rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
                  <Stethoscope className="w-6 h-6 text-white" />
                </div>
                <span className="text-xl font-bold text-slate-900">VetiCloud</span>
              </div>
              <Button
                variant="ghost"
                size="icon"
                onClick={onClose}
                className="lg:hidden hover:bg-slate-100"
              >
                <Languages className="h-5 w-5 text-slate-600" />
              </Button>
            </div>
          </div>

          {/* Navigation */}
          <nav className="flex-1 overflow-y-auto p-4 space-y-1 bg-white">
            {navigationItems.map((item) => (
              <SideBarLink
                key={item.pathname}
                pathname={item.pathname}
                content={t(item.key)}
                icon={item.icon}
                rightIcon={item.rightIcon}
              />
            ))}
          </nav>

          {/* User Profile Footer */}
          <div className="p-4 bg-white">
            <div className="flex items-center gap-3 p-3 rounded-xl hover:bg-slate-50 transition-colors cursor-pointer">
              <Avatar>
                <AvatarImage src="https://i.pravatar.cc/150?img=47" alt="Dr. Sarah Smith" />
                <AvatarFallback className="bg-primary text-white">SS</AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-slate-900 truncate">Dr. Sarah Smith</p>
                <p className="text-xs text-slate-500 truncate">{t(i18nKeyContainer.chiefVet)}</p>
              </div>
            </div>
            
            <Button
              variant="ghost"
              className="w-full justify-start gap-3 text-slate-600 hover:text-red-600 hover:bg-red-50"
              onClick={handleLogOut}
            >
              <LogOut className="h-4 w-4" />
              <span>{t(i18nKeyContainer.logout)}</span>
            </Button>
          </div>
        </div>
      </div>

      {/* Overlay for mobile */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/20 backdrop-blur-sm z-40 lg:hidden"
          onClick={onClose}
        />
      )}
    </>
  );
}