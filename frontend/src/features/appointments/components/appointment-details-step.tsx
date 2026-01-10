import { useTranslation } from "react-i18next";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Calendar, Clock, MapPin, FileText, User, PawPrint } from "lucide-react";
import { type Client } from "@/features/clients/client-api";
import { type Animal } from "@/features/animals/animal-api";
import { type Appointment } from "../appointment-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export interface AppointmentFormData {
  date: string;
  time: string;
  location: string;
  notes: string;
}

interface AppointmentDetailsStepProps {
  /** Form data */
  formData: AppointmentFormData;
  /** Update form data */
  onFormDataChange: (data: Partial<AppointmentFormData>) => void;
  /** Selected client info (for display) */
  client: Client | null;
  /** Selected animal info (for display) */
  animal: Animal | null;
  /** Existing appointment (for reschedule mode) */
  existingAppointment?: Appointment | null;
  /** Whether in reschedule mode */
  isRescheduleMode?: boolean;
}

export default function AppointmentDetailsStep({
  formData,
  onFormDataChange,
  client,
  animal,
  existingAppointment,
  isRescheduleMode = false,
}: AppointmentDetailsStepProps) {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      {/* Selected Client & Animal Summary */}
      {(client || existingAppointment) && (
        <div className="rounded-lg bg-slate-50 p-4 space-y-3">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
            {isRescheduleMode
              ? t(i18nKeyContainer.appointment.currentAppointment)
              : t(i18nKeyContainer.appointment.appointmentInfo)}
          </p>

          {/* Client Info */}
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-200">
              <User className="h-4 w-4 text-slate-600" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-900">
                {client?.fullName || existingAppointment?.clientName}
              </p>
              <p className="text-xs text-slate-500">
                {t(i18nKeyContainer.appointment.client)}
              </p>
            </div>
          </div>

          {/* Animal Info */}
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-slate-200">
              <PawPrint className="h-4 w-4 text-slate-600" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-900">
                {animal?.name || existingAppointment?.animalName}
                {animal?.species && (
                  <span className="ms-1.5 text-slate-500">• {animal.species}</span>
                )}
              </p>
              <p className="text-xs text-slate-500">
                {t(i18nKeyContainer.appointment.animal)}
              </p>
            </div>
          </div>

          {/* Current Appointment Date (for reschedule) */}
          {isRescheduleMode && existingAppointment && (
            <div className="mt-2 pt-2 border-t border-slate-200">
              <p className="text-sm text-slate-600">
                <span className="text-slate-500">{t(i18nKeyContainer.appointment.currentAppointment)}:</span>{" "}
                {new Date(existingAppointment.appointmentDate).toLocaleString(undefined, {
                  weekday: "long",
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </p>
            </div>
          )}
        </div>
      )}

      {/* Date Field */}
      <div className="space-y-2">
        <Label htmlFor="date" className="text-sm font-medium text-slate-700">
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.appointment.date)}
          </div>
        </Label>
        <Input
          id="date"
          type="date"
          value={formData.date}
          onChange={(e) => onFormDataChange({ date: e.target.value })}
          className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
          min={new Date().toISOString().split("T")[0]}
          required
        />
      </div>

      {/* Time Field */}
      <div className="space-y-2">
        <Label htmlFor="time" className="text-sm font-medium text-slate-700">
          <div className="flex items-center gap-2">
            <Clock className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.appointment.time)}
          </div>
        </Label>
        <Input
          id="time"
          type="time"
          value={formData.time}
          onChange={(e) => onFormDataChange({ time: e.target.value })}
          className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
          required
        />
      </div>

      {/* Location Field - Only for creating new appointments */}
      {!isRescheduleMode && (
        <div className="space-y-2">
          <Label htmlFor="location" className="text-sm font-medium text-slate-700">
            <div className="flex items-center gap-2">
              <MapPin className="h-4 w-4 text-slate-400" />
              {t(i18nKeyContainer.appointment.location)}
            </div>
          </Label>
          <Input
            id="location"
            type="text"
            value={formData.location}
            onChange={(e) => onFormDataChange({ location: e.target.value })}
            placeholder={t(i18nKeyContainer.appointment.locationPlaceholder)}
            className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
            required
          />
        </div>
      )}

      {/* Notes Field - Optional */}
      <div className="space-y-2">
        <Label htmlFor="notes" className="text-sm font-medium text-slate-700">
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-slate-400" />
            {t(i18nKeyContainer.appointment.notesOptional)}
          </div>
        </Label>
        <Input
          id="notes"
          type="text"
          value={formData.notes}
          onChange={(e) => onFormDataChange({ notes: e.target.value })}
          placeholder={t(i18nKeyContainer.appointment.notesPlaceholder)}
          className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
        />
      </div>
    </div>
  );
}
