import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import {
  LayoutDashboard,
  Plus,
  Calendar,
  Users,
  ClipboardList,
  TrendingUp,
  Loader2,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";

import dashboardApi, { type DashboardCardsResponse } from "./dashboard-api";
import AddUpdateVisit from "../visits/add-update-visit";

// Card configuration for stats display
type StatCardConfig = {
  key: keyof DashboardCardsResponse;
  titleKey: string;
  descriptionKey: string;
  icon: React.ElementType;
  iconBgClass: string;
  iconColorClass: string;
};

const STAT_CARDS_CONFIG: StatCardConfig[] = [
  {
    key: "totalAnimals",
    titleKey: "dashboardPage.cards.totalAnimals.title",
    descriptionKey: "dashboardPage.cards.totalAnimals.description",
    icon: Users,
    iconBgClass: "bg-blue-50",
    iconColorClass: "text-blue-500",
  },
  {
    key: "totalAppointments",
    titleKey: "dashboardPage.cards.totalAppointments.title",
    descriptionKey: "dashboardPage.cards.totalAppointments.description",
    icon: TrendingUp,
    iconBgClass: "bg-emerald-50",
    iconColorClass: "text-emerald-500",
  },
  {
    key: "totalVisits",
    titleKey: "dashboardPage.cards.totalVisits.title",
    descriptionKey: "dashboardPage.cards.totalVisits.description",
    icon: Calendar,
    iconBgClass: "bg-purple-50",
    iconColorClass: "text-purple-500",
  },
  {
    key: "completedAppointments",
    titleKey: "dashboardPage.cards.completedAppointments.title",
    descriptionKey: "dashboardPage.cards.completedAppointments.description",
    icon: TrendingUp,
    iconBgClass: "bg-orange-50",
    iconColorClass: "text-orange-500",
  },
];


export default function DashboardPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const isRtl = i18n.language === "ar";

  // Modal state for Add Visit
  const [isAddVisitOpen, setIsAddVisitOpen] = useState(false);

  // Fetch dashboard stats
  const { data, isLoading } = useQuery({
    queryKey: ["dashboard-cards"],
    queryFn:  () => {
        const data = dashboardApi.getDashboardCard();
        return data;
    },
    
  });
 

  // Handle Add Visit action
  const handleAddVisit = () => {
    setIsAddVisitOpen(true);
  };

  // Handle navigate to appointments
  const handleGoToAppointments = () => {
    navigate("/appointments");
  };

  return (
    <div dir={isRtl ? "rtl" : "ltr"} className="min-h-full">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-8">
        {/* Title Section */}
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
            <LayoutDashboard className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">
              {t("dashboardPage.title")}
            </h1>
            <p className="text-slate-500 text-sm">
              {t("dashboardPage.subtitle")}
            </p>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex items-center gap-3 sm:ms-auto">
          <Button
            variant="outline"
            onClick={handleGoToAppointments}
            className="gap-2 border-0 bg-white hover:bg-slate-50 cursor-pointer"
          >
            <ClipboardList className="h-4 w-4" />
            {t("dashboardPage.appointmentsButton")}
          </Button>
          <Button onClick={handleAddVisit} className="gap-2 cursor-pointer">
            <Plus className="h-4 w-4" />
            {t("dashboardPage.addVisit")}
          </Button>
        </div>
      </div>

      {/* Stats Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
        {STAT_CARDS_CONFIG.map((card) => {
          const Icon = card.icon;
          const value = data?.[card.key] ?? 0;

          return (
            <Card
              key={card.key}
              className="p-6 rounded-2xl transition-shadow hover:shadow-md bg-white border-0 shadow-sm"
            >
              {/* Title and Icon Row */}
              <div className="flex items-start justify-between mb-6">
                <p className="text-sm font-bold text-slate-700">
                  {t(card.titleKey)}
                </p>
                <div
                  className={`flex h-10 w-10 items-center justify-center rounded-lg ${card.iconBgClass}`}
                >
                  <Icon className={`h-5 w-5 ${card.iconColorClass}`} />
                </div>
              </div>

              {/* Value */}
              <div className="text-3xl font-bold text-slate-900 mb-1 h-9 flex items-center">
                {isLoading ? (
                  <Loader2 className="h-7 w-7 animate-spin text-slate-400" />
                ) : (
                  value.toLocaleString()
                )}
              </div>

              {/* Description */}
              <p className="text-sm text-slate-500">
                {t(card.descriptionKey)}
              </p>
            </Card>
          );
        })}
      </div>

      {/* Add Visit Modal (Stub) */}
      <AddUpdateVisit open={isAddVisitOpen} onClose={() => setIsAddVisitOpen(false)} />
    </div>
  );
}