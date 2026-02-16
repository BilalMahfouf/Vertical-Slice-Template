import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import {
  Syringe,
  User,
  PawPrint,
  Calendar,
  FileText,
  Stethoscope,
  AlertCircle,
  Hash,
} from "lucide-react";
import vaccinationApi from "./vaccination-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface ViewVaccinationProps {
  open: boolean;
  onClose: () => void;
  vaccinationId: string;
}

/**
 * ViewVaccination Component
 *
 * A read-only dialog that displays comprehensive vaccination details.
 *
 * Features:
 * - Fully responsive (mobile, tablet, desktop)
 * - RTL-friendly for Arabic language support
 * - Scrollable content for long forms
 * - Uses ShadCN UI components for consistency
 * - Follows app's design system and spacing rules
 * - Multilingual support with i18n
 * - Display-only components (no input fields)
 * - Due date urgency indicator with color-coded badge
 */
export default function ViewVaccination({
  open,
  onClose,
  vaccinationId,
}: ViewVaccinationProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  // Fetch vaccination data
  const { data: vaccination, isLoading } = useQuery({
    queryKey: ["vaccination", vaccinationId],
    queryFn: () => vaccinationApi.getVaccinationById(vaccinationId),
    enabled: open && !!vaccinationId,
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
   * Get due date urgency level and badge styling
   */
  const getDueDateBadge = (dueToDate: string | null) => {
    if (!dueToDate) {
      return {
        bg: "bg-slate-100",
        text: "text-slate-600",
        dot: "bg-slate-400",
        label: t(i18nKeyContainer.vaccination.noDueDate),
        icon: null,
      };
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dueDate = new Date(dueToDate);
    dueDate.setHours(0, 0, 0, 0);
    const diffDays = Math.ceil(
      (dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24)
    );

    if (diffDays < 0) {
      // Overdue
      return {
        bg: "bg-red-50",
        text: "text-red-700",
        dot: "bg-red-500",
        label: t(i18nKeyContainer.vaccination.overdue),
        icon: <AlertCircle className="h-4 w-4" />,
      };
    } else if (diffDays <= 7) {
      // Due soon (within 7 days)
      return {
        bg: "bg-amber-50",
        text: "text-amber-700",
        dot: "bg-amber-500",
        label: t(i18nKeyContainer.vaccination.dueSoon),
        icon: null,
      };
    } else if (diffDays <= 30) {
      // Due within month
      return {
        bg: "bg-blue-50",
        text: "text-blue-700",
        dot: "bg-blue-500",
        label: t(i18nKeyContainer.vaccination.dueWithinMonth),
        icon: null,
      };
    } else {
      // Due later
      return {
        bg: "bg-green-50",
        text: "text-green-700",
        dot: "bg-green-500",
        label: t(i18nKeyContainer.vaccination.dueLater),
        icon: null,
      };
    }
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
                <Syringe className="h-6 w-6 text-primary" />
              </div>

              {/* Title and Description */}
              <div className="flex-1">
                <h2 className="text-xl font-semibold text-slate-900">
                  {t(i18nKeyContainer.vaccination.viewTitle)}
                </h2>
                <p className="text-sm text-slate-500 mt-0.5">
                  {t(i18nKeyContainer.vaccination.viewDescription)}
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
            ) : vaccination ? (
              <div className="space-y-6">
                {/* Vaccination ID */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.vaccination.vaccinationId)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Hash className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-mono text-sm">{vaccination.id}</span>
                  </div>
                </div>

                {/* Vaccination Name */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.vaccination.vaccineName)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Syringe className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-medium">
                      {vaccination.vaccinationName}
                    </span>
                  </div>
                </div>

                {/* Dates Section - Two Column Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Given At */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.vaccination.givenAt)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Calendar className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatDate(vaccination.givenAt)}
                      </span>
                    </div>
                  </div>

                  {/* Due Date with Urgency Badge */}
                  <div className="space-y-1.5 flex flex-col gap-0.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.vaccination.dueTo)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Calendar className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900 flex-1">
                        {vaccination.dueTo
                          ? formatDate(vaccination.dueTo)
                          : t(i18nKeyContainer.vaccination.noDueDate)}
                      </span>
                      {vaccination.dueTo && (
                        <span
                          className={cn(
                            "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
                            getDueDateBadge(vaccination.dueTo).bg,
                            getDueDateBadge(vaccination.dueTo).text
                          )}
                        >
                          {getDueDateBadge(vaccination.dueTo).icon}
                          <span
                            className={cn(
                              "h-1.5 w-1.5 rounded-full",
                              getDueDateBadge(vaccination.dueTo).dot
                            )}
                          />
                          {getDueDateBadge(vaccination.dueTo).label}
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                {/* Client Information Section */}
                <div className="space-y-3">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                    <User className="h-4 w-4" />
                    {t(i18nKeyContainer.vaccination.clientInfo)}
                  </h3>
                  <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-slate-200">
                        <User className="h-5 w-5 text-slate-600" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-900">
                          {vaccination.clientName}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Animal Information Section */}
                <div className="space-y-3">
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                    <PawPrint className="h-4 w-4" />
                    {t(i18nKeyContainer.vaccination.animalInfo)}
                  </h3>
                  <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
                        <PawPrint className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="font-medium text-slate-900">
                          {vaccination.animalName}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Related Visit Section (if exists) */}
                {vaccination.visitId && (
                  <div className="space-y-3">
                    <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500 flex items-center gap-2">
                      <Stethoscope className="h-4 w-4" />
                      {t(i18nKeyContainer.vaccination.relatedVisit)}
                    </h3>
                    <div className="rounded-lg bg-slate-50 border border-slate-200 p-4 space-y-3">
                      <div className="flex items-center gap-2">
                        <Stethoscope className="h-4 w-4 text-slate-400" />
                        <span className="text-slate-900 font-mono text-sm">
                          {vaccination.visitId}
                        </span>
                      </div>
                    </div>
                  </div>
                )}

                {/* Notes */}
                <div className="space-y-1.5 flex flex-col gap-0.5">
                  <label className="text-sm font-medium text-slate-700 flex items-center gap-2">
                    <FileText className="h-4 w-4 text-slate-400" />
                    {t(i18nKeyContainer.vaccination.notes)}
                  </label>
                  <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-20">
                    {vaccination.notes ? (
                      <p className="text-slate-700 whitespace-pre-wrap wrap-break-word">
                        {vaccination.notes}
                      </p>
                    ) : (
                      <p className="text-slate-400 italic">
                        {t(i18nKeyContainer.vaccination.noNotes)}
                      </p>
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
