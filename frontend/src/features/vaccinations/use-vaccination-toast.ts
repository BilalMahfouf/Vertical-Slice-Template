import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { parseApiError, ErrorCodes, type ParsedApiError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

/**
 * Vaccination-specific toast hook
 * Handles all vaccination-related toast notifications with domain-aware error mapping
 */
export function useVaccinationToast() {
  const { t } = useTranslation();

  /**
   * Show success toast for vaccination created
   */
  const created = () => {
    toast.success(t(i18nKeyContainer.toast.vaccination.created), {
      description: t(i18nKeyContainer.toast.vaccination.createdDesc),
    });
  };

  /**
   * Show success toast for vaccination updated
   */
  const updated = () => {
    toast.success(t(i18nKeyContainer.toast.vaccination.updated), {
      description: t(i18nKeyContainer.toast.vaccination.updatedDesc),
    });
  };

  /**
   * Show success toast for vaccination deleted
   */
  const deleted = () => {
    toast.success(t(i18nKeyContainer.toast.vaccination.deleted), {
      description: t(i18nKeyContainer.toast.vaccination.deletedDesc),
    });
  };

  /**
   * Show error toast with domain-specific message
   * Maps backend error codes to localized vaccination error messages
   */
  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);
    
    // Map vaccination-specific error codes to i18n keys
    let titleKey: string;
    let descKey: string;

    switch (parsedError.code) {
      case ErrorCodes.VACCINATION_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.vaccination.notFound;
        descKey = i18nKeyContainer.errors.vaccination.notFoundDesc;
        break;
      case ErrorCodes.VACCINATIONS_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.vaccination.listNotFound;
        descKey = i18nKeyContainer.errors.vaccination.listNotFoundDesc;
        break;
      default:
        titleKey = i18nKeyContainer.errors.vaccination.operationFailed;
        descKey = i18nKeyContainer.errors.vaccination.operationFailedDesc;
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
