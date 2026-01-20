import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { CalendarClock, User, PawPrint, Calendar, Clock, FileText } from "lucide-react";
import appointmentApi from "./appointment-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface ViewAppointmentProps {
  open: boolean;
  onClose: () => void;
  appointmentId: string;
}

/**
 * ViewAppointment Component
 * 
 * A read-only dialog that displays comprehensive appointment details.
 * 
 * Features:
 * - Fully responsive (mobile, tablet, desktop)
 * - RTL-friendly for Arabic language support
 * - Scrollable content for long forms
 * - Uses ShadCN UI components for consistency
 * - Follows app's design system and spacing rules
 * - Multilingual support with i18n
 */
export default function ViewAppointment({ 
  open, 
  onClose, 
  appointmentId 
}: ViewAppointmentProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  // Fetch appointment data
  const { data: appointment, isLoading } = useQuery({
    queryKey: ["appointment", appointmentId],
    queryFn: () => appointmentApi.getAppointmentById(appointmentId),
    enabled: open && !!appointmentId,
  });

  /**
   * Format date to localized string
   */
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(i18n.language, {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  /**
   * Format time to localized string
   */
  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString(i18n.language, {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  /**
   * Get status badge configuration based on appointment status
   */
  const getStatusBadge = (status: string) => {
    const normalizedStatus = status.toLowerCase();
    
    const statusConfig: Record<string, { bg: string; text: string; dot: string; label: string }> = {
      confirmed: {
        bg: "bg-emerald-50",
        text: "text-emerald-700",
        dot: "bg-emerald-500",
        label: t(i18nKeyContainer.appointment.statusConfirmed),
      },
      cancelled: {
        bg: "bg-red-50",
        text: "text-red-700",
        dot: "bg-red-500",
        label: t(i18nKeyContainer.appointment.statusCancelled),
      },
      completed: {
        bg: "bg-blue-50",
        text: "text-blue-700",
        dot: "bg-blue-500",
        label: t(i18nKeyContainer.appointment.statusCompleted),
      },
      rescheduled: {
        bg: "bg-amber-50",
        text: "text-amber-700",
        dot: "bg-amber-500",
        label: t(i18nKeyContainer.appointment.statusRescheduled),
      },
    };

    return statusConfig[normalizedStatus] || {
      bg: "bg-slate-50",
      text: "text-slate-700",
      dot: "bg-slate-400",
      label: status,
    };
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent
        className="max-w-2xl p-0 bg-white"
        dir={isRtl ? "rtl" : "ltr"}
        onInteractOutside={(e) => e.preventDefault()}
      >
        <div className="w-full">
          {/* Header Section */}
          <div className="border-b border-slate-200 px-6 py-6">
            <div className="flex items-center gap-4">
              {/* Icon */}
              <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                <CalendarClock className="h-6 w-6 text-primary" />
              </div>
              
              {/* Title and Description */}
              <div className="flex-1">
                <h2 className="text-xl font-semibold text-slate-900">
                  {t(i18nKeyContainer.appointment.viewTitle)}
                </h2>
                <p className="text-sm text-slate-500 mt-0.5">
                  {t(i18nKeyContainer.appointment.viewDescription)}
                </p>
              </div>
            </div>
          </div>

          {/* Content Section - Scrollable */}
          <div className="px-6 py-6 max-h-[calc(100vh-16rem)] overflow-y-auto">
            {isLoading ? (
              // Loading Skeleton
              <div className="space-y-4">
                <div className="h-16 bg-slate-100 rounded-lg animate-pulse" />
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="h-16 bg-slate-100 rounded-lg animate-pulse" />
                  <div className="h-16 bg-slate-100 rounded-lg animate-pulse" />
                </div>
                <div className="h-16 bg-slate-100 rounded-lg animate-pulse" />
                <div className="h-16 bg-slate-100 rounded-lg animate-pulse" />
                <div className="h-24 bg-slate-100 rounded-lg animate-pulse" />
              </div>
            ) : appointment ? (
              <div className="space-y-5">
                {/* Status Badge */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.appointment.status)}
                  </label>
                  <div className="flex items-start">
                    {(() => {
                      const statusBadge = getStatusBadge(appointment.status);
                      return (
                        <span
                          className={cn(
                            "inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-sm font-medium",
                            statusBadge.bg,
                            statusBadge.text
                          )}
                        >
                          <span className={cn("h-2 w-2 rounded-full", statusBadge.dot)} />
                          {statusBadge.label}
                        </span>
                      );
                    })()}
                  </div>
                </div>

                {/* Date and Time - Two Column Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Date */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.appointment.date)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Calendar className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatDate(appointment.appointmentDate)}
                      </span>
                    </div>
                  </div>

                  {/* Time */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.appointment.time)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Clock className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatTime(appointment.appointmentDate)}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Client Name */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.appointment.client)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <User className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">{appointment.clientName}</span>
                  </div>
                </div>

                {/* Animal Name */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.appointment.animal)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <PawPrint className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">{appointment.animalName}</span>
                  </div>
                </div>

                {/* Created Date */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.appointment.createdOn)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <FileText className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">
                      {formatDate(appointment.createdOnUtc)}
                    </span>
                  </div>
                </div>
              </div>
            ) : null}
          </div>

          {/* Footer Section */}
          <div className="border-t border-slate-200 px-6 py-4">
            <Button
              onClick={onClose}
              className="w-full cursor-pointer"
            >
              {t(i18nKeyContainer.common.close)}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
