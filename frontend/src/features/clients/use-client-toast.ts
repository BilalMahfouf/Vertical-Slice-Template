import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { parseApiError, ErrorCodes, type ParsedApiError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

/**
 * Client-specific toast hook
 * Handles all client-related toast notifications with domain-aware error mapping
 */
export function useClientToast() {
  const { t } = useTranslation();

  /**
   * Show success toast for client operations
   */
  const success = (
    operation: "added" | "updated" | "deleted"
  ) => {
    const titleKey = i18nKeyContainer.toast.client[operation];
    const descKey = i18nKeyContainer.toast.client[`${operation}Desc`];
    
    toast.success(t(titleKey), {
      description: t(descKey),
    });
  };

  /**
   * Show error toast with domain-specific message
   * Maps backend error codes to localized client error messages
   */
  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);
    
    // Map client-specific error codes to i18n keys
    let titleKey: string;
    let descKey: string;
    
    switch (parsedError.code) {
      case ErrorCodes.CLIENT_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.client.notFound;
        descKey = i18nKeyContainer.errors.client.notFoundDesc;
        break;
      case ErrorCodes.CLIENTS_NOT_FOUND:
        titleKey = i18nKeyContainer.errors.client.listNotFound;
        descKey = i18nKeyContainer.errors.client.listNotFoundDesc;
        break;
      case ErrorCodes.CLIENT_DUPLICATE:
        titleKey = i18nKeyContainer.errors.client.duplicate;
        descKey = i18nKeyContainer.errors.client.duplicateDesc;
        toast.warning(t(titleKey), { description: t(descKey) });
        return parsedError;
      case ErrorCodes.CLIENT_SAME_NAME_EXISTS:
        titleKey = i18nKeyContainer.errors.client.sameNameExists;
        descKey = i18nKeyContainer.errors.client.sameNameExistsDesc;
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
      default:
        // Generic client operation error
        titleKey = i18nKeyContainer.errors.client.operationFailed;
        descKey = i18nKeyContainer.errors.client.operationFailedDesc;
    }
    
    toast.error(t(titleKey), {
      description: t(descKey),
    });
    
    return parsedError;
  };

  /**
   * Show warning toast for client operations
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
