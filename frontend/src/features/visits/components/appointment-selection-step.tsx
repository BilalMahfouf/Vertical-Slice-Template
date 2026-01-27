import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Search,
  Calendar,
  User,
  PawPrint,
  CheckCircle,
  AlertCircle,
  Loader2,
  X,
} from "lucide-react";
import appointmentApi, { type Appointment } from "@/features/appointments/appointment-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export interface AppointmentSelection {
  appointment: Appointment;
}

interface AppointmentSelectionStepProps {
  /** Callback when an appointment is selected */
  onSelect: (selection: AppointmentSelection) => void;
  /** Initial appointment (for edit mode) */
  initialAppointment?: Appointment | null;
  /** Is RTL layout */
  isRtl?: boolean;
  /** Callback to switch to client mode */
  onSwitchToClientMode?: () => void;
}

export default function AppointmentSelectionStep({
  onSelect,
  initialAppointment = null,
  isRtl = false,
  onSwitchToClientMode,
}: AppointmentSelectionStepProps) {
  const { t } = useTranslation();

  // Search state
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(
    initialAppointment
  );
  const [isSearching, setIsSearching] = useState(false);
  const [searchError, setSearchError] = useState<string | null>(null);

  // Handle search by ID
  const handleSearch = async () => {
    const trimmedQuery = searchQuery.trim();
    if (!trimmedQuery) return;

    setIsSearching(true);
    setSearchError(null);

    try {
      const appointment = await appointmentApi.getAppointmentById(trimmedQuery);

      // Validate status: only Confirmed or Rescheduled allowed
      if (appointment.status !== "Confirmed" && appointment.status !== "Rescheduled") {
        setSearchError(t(i18nKeyContainer.visit.appointmentInvalidStatus));
        return;
      }

      // Valid appointment found - select it
      setSelectedAppointment(appointment);
      onSelect({ appointment });
    } catch {
      setSearchError(t(i18nKeyContainer.visit.appointmentNotFound));
    } finally {
      setIsSearching(false);
    }
  };

  // Handle Enter key press
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleSearch();
    }
  };

  // Clear selection
  const handleClearSelection = () => {
    setSelectedAppointment(null);
    setSearchQuery("");
    setSearchError(null);
  };

  // Format date for display
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(isRtl ? "ar-SA" : "en-US", {
      weekday: "short",
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="space-y-6">
      {/* Selected Appointment Card */}
      {selectedAppointment ? (
        <div className="space-y-4">
          <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4">
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-start gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-emerald-100">
                  <Calendar className="h-5 w-5 text-emerald-600" />
                </div>
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-emerald-600" />
                    <span className="text-xs font-medium uppercase tracking-wide text-emerald-600">
                      {t(i18nKeyContainer.visit.appointmentSelected)}
                    </span>
                  </div>
                  <p className="font-medium text-slate-900">
                    {formatDate(selectedAppointment.appointmentDate)}
                  </p>
                  <div className="flex flex-col gap-1 mt-2">
                    <div className="flex items-center gap-1.5 text-sm text-slate-600">
                      <User className="h-3.5 w-3.5" />
                      <span>{selectedAppointment.clientName}</span>
                    </div>
                    <div className="flex items-center gap-1.5 text-sm text-slate-600">
                      <PawPrint className="h-3.5 w-3.5" />
                      <span>{selectedAppointment.animalName}</span>
                    </div>
                  </div>
                </div>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleClearSelection}
                className="h-8 w-8 p-0 text-slate-400 hover:text-slate-600 hover:bg-slate-100 cursor-pointer"
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      ) : (
        /* Appointment Search Section */
        <div className="space-y-4">
          <div className="space-y-3">
            <Label className="text-sm font-medium text-slate-700">
              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-slate-400" />
                {t(i18nKeyContainer.visit.searchAppointments)}
              </div>
            </Label>
            <div className="flex gap-2">
              <Input
                type="text"
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value);
                  setSearchError(null);
                }}
                onKeyDown={handleKeyDown}
                placeholder={t(i18nKeyContainer.visit.appointmentSearchPlaceholder)}
                className="h-11 flex-1 border-slate-200 focus:border-primary focus:ring-primary"
                disabled={isSearching}
              />
              <Button
                type="button"
                onClick={handleSearch}
                disabled={isSearching || !searchQuery.trim()}
                className="h-11 px-4 cursor-pointer"
              >
                {isSearching ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <>
                    <Search className="h-4 w-4 me-2" />
                    {t(i18nKeyContainer.appointment.search)}
                  </>
                )}
              </Button>
            </div>
          </div>

          {/* Error State */}
          {searchError && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-4">
              <div className="flex items-start gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-red-100">
                  <AlertCircle className="h-5 w-5 text-red-600" />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-red-700">{searchError}</p>
                  <p className="text-xs text-red-600 mt-1">
                    {t(i18nKeyContainer.visit.noAppointmentsHint)}
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Empty State / Initial State */}
          {!searchError && (
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-6 text-center">
              <div className="flex justify-center mb-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-purple-50">
                  <Calendar className="h-6 w-6 text-purple-400" />
                </div>
              </div>
              <p className="text-sm font-medium text-slate-700 mb-1">
                {t(i18nKeyContainer.visit.searchAppointments)}
              </p>
              <p className="text-xs text-slate-500 mb-4">
                {t(i18nKeyContainer.visit.noAppointmentsHint)}
              </p>
              {onSwitchToClientMode && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={onSwitchToClientMode}
                  className="cursor-pointer"
                >
                  {t(i18nKeyContainer.visit.switchToClientMode)}
                </Button>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
