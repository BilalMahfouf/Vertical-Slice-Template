import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { parseApiError, ErrorCodes, type ParsedApiError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

/**
 * Appointment-specific toast hook
 * Handles all appointment-related toast notifications with domain-aware error mapping
 */
export function useAppointmentToast() {
  const { t } = useTranslation();

  /**
   * Show success toast for appointment created
   */
  const created = () => {
    toast.success(t(i18nKeyContainer.toast.appointment.created), {
      description: t(i18nKeyContainer.toast.appointment.createdDesc),
    });
  };

  /**
   * Show success toast for appointment rescheduled
   */
  const rescheduled = () => {
    toast.success(t(i18nKeyContainer.toast.appointment.rescheduled), {
      description: t(i18nKeyContainer.toast.appointment.rescheduledDesc),
    });
  };

  /**
   * Show success toast for appointment canceled
   */
  const canceled = () => {
    toast.success(t(i18nKeyContainer.toast.appointment.canceled), {
      description: t(i18nKeyContainer.toast.appointment.canceledDesc),
    });
  };

  /**
   * Show error toast with domain-specific message
   * Maps backend error codes to localized appointment error messages
   */
  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);
    
    // Map appointment-specific error codes to i18n keys
    let titleKey: string;
    let descKey: string;
    
    switch (parsedError.code) {
      case ErrorCodes.APPOINTMENT_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.appointment.notFound;
        descKey = i18nKeyContainer.errors.appointment.notFoundDesc;
        break;
      case ErrorCodes.CLIENT_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.appointment.clientNotFound;
        descKey = i18nKeyContainer.errors.appointment.clientNotFoundDesc;
        break;
      case ErrorCodes.ANIMAL_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.appointment.animalNotFound;
        descKey = i18nKeyContainer.errors.appointment.animalNotFoundDesc;
        break;
      case ErrorCodes.VALIDATION_ERROR:
        titleKey = i18nKeyContainer.errors.validation;
        descKey = i18nKeyContainer.errors.validationDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.NETWORK_ERROR:
        titleKey = i18nKeyContainer.errors.network;
        descKey = i18nKeyContainer.errors.networkDesc;
        break;
      case ErrorCodes.SERVER_ERROR:
        titleKey = i18nKeyContainer.errors.server;
        descKey = i18nKeyContainer.errors.serverDesc;
        break;
      default:
        titleKey = i18nKeyContainer.errors.appointment.operationFailed;
        descKey = i18nKeyContainer.errors.appointment.operationFailedDesc;
    }
    
    toast.error(t(titleKey), { description: t(descKey) });
    return parsedError;
  };

  return {
    created,
    rescheduled,
    canceled,
    error,
  };
}
