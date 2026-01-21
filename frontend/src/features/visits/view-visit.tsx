import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import {
  Stethoscope,
  User,
  PawPrint,
  Calendar,
  Clock,
  FileText,
  Activity,
  Pill,
  CalendarClock,
  Hash,
} from "lucide-react";
import visitApi from "./visit-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface ViewVisitProps {
  open: boolean;
  onClose: () => void;
  visitId: string;
}

/**
 * ViewVisit Component
 * 
 * A read-only dialog that displays comprehensive visit details.
 * 
 * Features:
 * - Fully responsive (mobile, tablet, desktop)
 * - RTL-friendly for Arabic language support
 * - Scrollable content for long forms
 * - Uses ShadCN UI components for consistency
 * - Follows app's design system and spacing rules
 * - Multilingual support with i18n
 * - Display-only components (no input fields)
 */
export default function ViewVisit({ open, onClose, visitId }: ViewVisitProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  // Fetch visit data
  const { data: visit, isLoading } = useQuery({
    queryKey: ["visit", visitId],
    queryFn: () => {
        
      return visitApi.getVisitById(visitId);
    },
    enabled: open && !!visitId,
  });
  console.log("ViewVisit - fetched visit data:", visit);

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
   * Format date and time together
   */
  const formatDateTime = (dateString: string) => {
    return `${formatDate(dateString)} - ${formatTime(dateString)}`;
  };

  /**
   * Get visit type badge configuration
   */
  const getVisitTypeBadge = (visitType: string) => {
    const normalizedType = visitType.toLowerCase();

    const typeConfig: Record<string, { bg: string; text: string; dot: string; label: string }> = {
      clinic: {
        bg: "bg-blue-50",
        text: "text-blue-700",
        dot: "bg-blue-500",
        label: t(i18nKeyContainer.visit.clinic),
      },
      field: {
        bg: "bg-green-50",
        text: "text-green-700",
        dot: "bg-green-500",
        label: t(i18nKeyContainer.visit.field),
      },
      emergency: {
        bg: "bg-red-50",
        text: "text-red-700",
        dot: "bg-red-500",
        label: t(i18nKeyContainer.visit.emergency),
      },
    };

    return typeConfig[normalizedType] || {
      bg: "bg-slate-50",
      text: "text-slate-700",
      dot: "bg-slate-400",
      label: visitType,
    };
  };

  /**
   * Get appointment status badge configuration
   */
  const getAppointmentStatusBadge = (status: string) => {
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

  /**
   * Render array items as a formatted list
   */
  const renderArrayItems = (items: string[] | null) => {
    if (!items || items.length === 0) {
      return (
        <span className="text-slate-400 italic">
          {t(i18nKeyContainer.visit.noDataAvailable)}
        </span>
      );
    }
    return (
      <ul className="space-y-1">
        {items.map((item, index) => (
          <li key={index} className="flex items-start gap-2 text-slate-700">
            <span className="mt-2 h-1.5 w-1.5 rounded-full bg-slate-400 shrink-0" />
            <span>{item}</span>
          </li>
        ))}
      </ul>
    );
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
                <Stethoscope className="h-6 w-6 text-primary" />
              </div>

              {/* Title and Description */}
              <div className="flex-1">
                <h2 className="text-xl font-semibold text-slate-900">
                  {t(i18nKeyContainer.visit.viewTitle)}
                </h2>
                <p className="text-sm text-slate-500 mt-0.5">
                  {t(i18nKeyContainer.visit.viewDescription)}
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
            ) : visit ? (
              <div className="space-y-6">
                {/* Visit ID */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.visit.visitId)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Hash className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-mono text-sm">{visit.id}</span>
                  </div>
                </div>

                {/* Visit Type Badge */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.visit.visitType)}
                  </label>
                  <div className="flex items-start">
                    {(() => {
                      const typeBadge = getVisitTypeBadge(visit.visitType);
                      return (
                        <span
                          className={cn(
                            "inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-sm font-medium",
                            typeBadge.bg,
                            typeBadge.text
                          )}
                        >
                          <span className={cn("h-2 w-2 rounded-full", typeBadge.dot)} />
                          {typeBadge.label}
                        </span>
                      );
                    })()}
                  </div>
                </div>

                {/* Visit Date - Two Column Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Date */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.visit.visitDate)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Calendar className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatDate(visit.createdOnUtc)}
                      </span>
                    </div>
                  </div>

                  {/* Time */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.visit.time)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Clock className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatTime(visit.createdOnUtc)}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Client Information Section */}
                <div className="space-y-3">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                    <User className="h-4 w-4" />
                    {t(i18nKeyContainer.visit.clientInfo)}
                  </h3>
                  <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-slate-200">
                        <User className="h-5 w-5 text-slate-600" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-900">
                          {visit.clientFullName}
                        </p>
                        <p className="text-sm text-slate-500">{visit.clientPhone}</p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Animal Information Section */}
                <div className="space-y-3">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                    <PawPrint className="h-4 w-4" />
                    {t(i18nKeyContainer.visit.animalInfo)}
                  </h3>
                  <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
                        <PawPrint className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-900">
                          {visit.animalName}
                        </p>
                        <p className="text-sm text-slate-500">
                          {visit.animalSpecies}
                          {visit.animalBreed && ` • ${visit.animalBreed}`}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Related Appointment Section (if exists) */}
                {visit.appointmentId && (
                  <div className="space-y-3">
                    <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                      <CalendarClock className="h-4 w-4" />
                      {t(i18nKeyContainer.visit.relatedAppointment)}
                    </h3>
                    <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <Calendar className="h-4 w-4 text-slate-400" />
                          <span className="text-slate-900">
                            {visit.appointmentDate
                              ? formatDateTime(visit.appointmentDate)
                              : "-"}
                          </span>
                        </div>
                        {visit.appointmentStatus && (
                          <span
                            className={cn(
                              "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
                              getAppointmentStatusBadge(visit.appointmentStatus).bg,
                              getAppointmentStatusBadge(visit.appointmentStatus).text
                            )}
                          >
                            <span
                              className={cn(
                                "h-1.5 w-1.5 rounded-full",
                                getAppointmentStatusBadge(visit.appointmentStatus).dot
                              )}
                            />
                            {getAppointmentStatusBadge(visit.appointmentStatus).label}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                )}

                {/* Medical Information Section */}
                <div className="space-y-4 ">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                    <Activity className="h-4 w-4" />
                    {t(i18nKeyContainer.visit.medicalInfo)}
                  </h3>

                  {/* Symptoms */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.visit.symptoms)}
                    </label>
                    <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-16">
                      {renderArrayItems(visit.symptoms)}
                    </div>
                  </div>

                  {/* Diagnosis */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.visit.diagnosis)}
                    </label>
                    <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-16">
                      {renderArrayItems(visit.diagnosis)}
                    </div>
                  </div>

                  {/* Treatment */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                      <Pill className="h-4 w-4 text-slate-400" />
                      {t(i18nKeyContainer.visit.treatment)}
                    </label>
                    <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-16">
                      {renderArrayItems(visit.treatment)}
                    </div>
                  </div>
                </div>

                {/* Notes */}
                <div className="space-y-1.5 flex flex-col gap-0.5">
                  <label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    <FileText className="h-4 w-4 text-slate-400" />
                    {t(i18nKeyContainer.visit.notes)}
                  </label>
                  <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-20">
                    {visit.notes ? (
                      <p className="text-slate-700 whitespace-pre-wrap wrap-break-word">
                        {visit.notes}
                      </p>
                    ) : (
                      <p className="text-slate-400 italic">
                        {t(i18nKeyContainer.visit.noNotes)}
                      </p>
                    )}
                  </div>
                </div>

                {/* Record Timestamps */}
                <div className="pt-2 border-t border-slate-200">
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-xs text-slate-500">
                    <div>
                      <span className="font-medium">{t(i18nKeyContainer.visit.createdOn)}:</span>{" "}
                      {formatDateTime(visit.createdOnUtc)}
                    </div>
                    {visit.updatedOnUtc && (
                      <div>
                        <span className="font-medium">{t(i18nKeyContainer.visit.updatedOn)}:</span>{" "}
                        {formatDateTime(visit.updatedOnUtc)}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ) : null}
          </div>

          {/* Footer Section */}
          <div className="border-t border-slate-200 px-6 py-4">
            <Button onClick={onClose} className="w-full cursor-pointer">
              {t(i18nKeyContainer.common.close)}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
