import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { parseApiError, ErrorCodes, type ParsedApiError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

/**
 * Animal-specific toast hook
 * Handles all animal-related toast notifications with domain-aware error mapping
 */
export function useAnimalToast() {
  const { t } = useTranslation();

  /**
   * Show success toast for animal operations
   */
  const success = (
    operation: "added" | "updated" | "deleted"
  ) => {
    const titleKey = i18nKeyContainer.toast.animal[operation];
    const descKey = i18nKeyContainer.toast.animal[`${operation}Desc`];
    
    toast.success(t(titleKey), {
      description: t(descKey),
    });
  };

  /**
   * Show error toast with domain-specific message
   * Maps backend error codes to localized animal error messages
   */
  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);
    
    // Map animal-specific error codes to i18n keys
    let titleKey: string;
    let descKey: string;
    
    switch (parsedError.code) {
      case ErrorCodes.ANIMAL_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.animal.notFound;
        descKey = i18nKeyContainer.errors.animal.notFoundDesc;
        break;
      case ErrorCodes.ANIMALS_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.animal.listNotFound;
        descKey = i18nKeyContainer.errors.animal.listNotFoundDesc;
        break;
      case ErrorCodes.CLIENT_NOT_FOUND:
        // Animal operations may fail if client (owner) is not found
        titleKey = i18nKeyContainer.errors.animal.ownerNotFound;
        descKey = i18nKeyContainer.errors.animal.ownerNotFoundDesc;
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
      default:
        // Generic animal operation error
        titleKey = i18nKeyContainer.errors.animal.operationFailed;
        descKey = i18nKeyContainer.errors.animal.operationFailedDesc;
    }
    
    toast.error(t(titleKey), {
      description: t(descKey),
    });
    
    return parsedError;
  };

  /**
   * Show warning toast for animal operations
   */
  const warning = (
    titleKey: string,
    descriptionKey?: string
  ) => {
    toast.warning(t(titleKey), {
      description: descriptionKey ? t(descriptionKey) : undefined,
    });
  };

  return {
    // Success operations
    success,
    added: () => success("added"),
    updated: () => success("updated"),
    deleted: () => success("deleted"),
    
    // Error handling
    error,
    
    // Warning
    warning,
  };
}
