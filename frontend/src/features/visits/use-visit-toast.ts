import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { parseApiError, ErrorCodes, type ParsedApiError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

/**
 * Visit-specific toast hook
 * Handles all visit-related toast notifications with domain-aware error mapping
 */
export function useVisitToast() {
  const { t } = useTranslation();

  /**
   * Show success toast for visit created
   */
  const created = () => {
    toast.success(t(i18nKeyContainer.toast.visit.created), {
      description: t(i18nKeyContainer.toast.visit.createdDesc),
    });
  };

  /**
   * Show success toast for visit updated
   */
  const updated = () => {
    toast.success(t(i18nKeyContainer.toast.visit.updated), {
      description: t(i18nKeyContainer.toast.visit.updatedDesc),
    });
  };

  /**
   * Show success toast for visit deleted
   */
  const deleted = () => {
    toast.success(t(i18nKeyContainer.toast.visit.deleted), {
      description: t(i18nKeyContainer.toast.visit.deletedDesc),
    });
  };

  /**
   * Show error toast with domain-specific message
   * Maps backend error codes to localized visit error messages
   */
  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);
    
    // Map visit-specific error codes to i18n keys
    let titleKey: string;
    let descKey: string;
    console.log("Parsed visit error:", parsedError);
    switch (parsedError.code) {
      case ErrorCodes.VISIT_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.visit.notFound;
        descKey = i18nKeyContainer.errors.visit.notFoundDesc;
        break;
      case ErrorCodes.VISITS_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.visit.listNotFound;
        descKey = i18nKeyContainer.errors.visit.listNotFoundDesc;
        break;
      case ErrorCodes.VISIT_WITH_APPOINTMENT_ALREADY_EXISTS:
        titleKey = i18nKeyContainer.errors.visit.appointmentAlreadyExists;
        descKey = i18nKeyContainer.errors.visit.appointmentAlreadyExistsDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.CLIENT_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.visit.clientNotFound;
        descKey = i18nKeyContainer.errors.visit.clientNotFoundDesc;
        break;
      case ErrorCodes.ANIMAL_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.visit.animalNotFound;
        descKey = i18nKeyContainer.errors.visit.animalNotFoundDesc;
        break;
      case ErrorCodes.APPOINTMENT_NOT_FOUND:
      case ErrorCodes.APPOINTMENT_GENERIC_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.visit.appointmentNotFound;
        descKey = i18nKeyContainer.errors.visit.appointmentNotFoundDesc;
        break;
      case ErrorCodes.APPOINTMENT_INVALID_DATE:
        titleKey = i18nKeyContainer.errors.visit.invalidAppointmentDate;
        descKey = i18nKeyContainer.errors.visit.invalidAppointmentDateDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.APPOINTMENT_INVALID_STATUS:
        titleKey = i18nKeyContainer.errors.visit.invalidAppointmentStatus;
        descKey = i18nKeyContainer.errors.visit.invalidAppointmentStatusDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.APPOINTMENT_OUTDATED:
        titleKey = i18nKeyContainer.errors.visit.outdatedAppointment;
        descKey = i18nKeyContainer.errors.visit.outdatedAppointmentDesc;
        break;
      case ErrorCodes.APPOINTMENT_RESCHEDULE_PROBLEM:
        titleKey = i18nKeyContainer.errors.visit.rescheduleProblem;
        descKey = i18nKeyContainer.errors.visit.rescheduleProblemDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.APPOINTMENT_COMPLETE_PROBLEM:
        titleKey = i18nKeyContainer.errors.visit.completeProblem;
        descKey = i18nKeyContainer.errors.visit.completeProblemDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.APPOINTMENT_CANCEL_PROBLEM:
        titleKey = i18nKeyContainer.errors.visit.cancelProblem;
        descKey = i18nKeyContainer.errors.visit.cancelProblemDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
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
        titleKey = i18nKeyContainer.errors.visit.operationFailed;
        descKey = i18nKeyContainer.errors.visit.operationFailedDesc;
    }
    
    toast.error(t(titleKey), { description: t(descKey) });
    return parsedError;
  };

  return {
    created,
    updated,
    deleted,
    error,
  };
}
