import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { parseApiError, getErrorI18nKey, type ParsedApiError } from "@/lib/api/error-types";

type ToastType = "success" | "error" | "warning" | "info";

interface ToastOptions {
  description?: string;
  duration?: number;
}

/**
 * Centralized toast hook with i18n support
 * Provides consistent toast notifications throughout the application
 */
export function useToast() {
  const { t } = useTranslation();

  const showToast = (
    type: ToastType,
    titleKey: string,
    options?: ToastOptions
  ) => {
    const title = t(titleKey);
    const description = options?.description ? t(options.description) : undefined;

    switch (type) {
      case "success":
        toast.success(title, { description, duration: options?.duration });
        break;
      case "error":
        toast.error(title, { description, duration: options?.duration });
        break;
      case "warning":
        toast.warning(title, { description, duration: options?.duration });
        break;
      case "info":
        toast.info(title, { description, duration: options?.duration });
        break;
    }
  };

  /**
   * Handle API error and show appropriate toast
   * Maps backend error codes to localized messages
   */
  const handleApiError = (error: unknown, fallbackKey?: string): ParsedApiError => {
    const parsedError = parseApiError(error);
    const errorTitleKey = getErrorI18nKey(parsedError.code);
    
    // Determine toast type based on error type
    let toastType: ToastType = "error";
    if (parsedError.type === "validation") {
      toastType = "warning";
    } else if (parsedError.type === "notFound") {
      toastType = "warning";
    }
    
    showToast(toastType, errorTitleKey, {
      description: fallbackKey,
    });
    
    return parsedError;
  };

  // Predefined toast methods for common actions
  return {
    // Generic methods
    success: (titleKey: string, options?: ToastOptions) =>
      showToast("success", titleKey, options),
    error: (titleKey: string, options?: ToastOptions) =>
      showToast("error", titleKey, options),
    warning: (titleKey: string, options?: ToastOptions) =>
      showToast("warning", titleKey, options),
    info: (titleKey: string, options?: ToastOptions) =>
      showToast("info", titleKey, options),

    // API error handler
    handleApiError,

    // Client-specific toasts (kept for backward compatibility)
    clientAdded: () =>
      showToast("success", i18nKeyContainer.toast.clientAdded, {
        description: i18nKeyContainer.toast.clientAddedDesc,
      }),
    clientUpdated: () =>
      showToast("success", i18nKeyContainer.toast.clientUpdated, {
        description: i18nKeyContainer.toast.clientUpdatedDesc,
      }),
    clientDeleted: () =>
      showToast("success", i18nKeyContainer.toast.clientDeleted, {
        description: i18nKeyContainer.toast.clientDeletedDesc,
      }),
    clientError: () =>
      showToast("error", i18nKeyContainer.toast.errorOccurred, {
        description: i18nKeyContainer.toast.clientErrorDesc,
      }),

    // Animal-specific toasts (kept for backward compatibility)
    animalAdded: () =>
      showToast("success", i18nKeyContainer.toast.animalAdded, {
        description: i18nKeyContainer.toast.animalAddedDesc,
      }),
    animalUpdated: () =>
      showToast("success", i18nKeyContainer.toast.animalUpdated, {
        description: i18nKeyContainer.toast.animalUpdatedDesc,
      }),
    animalDeleted: () =>
      showToast("success", i18nKeyContainer.toast.animalDeleted, {
        description: i18nKeyContainer.toast.animalDeletedDesc,
      }),
    animalError: () =>
      showToast("error", i18nKeyContainer.toast.errorOccurred, {
        description: i18nKeyContainer.toast.animalErrorDesc,
      }),

    // Generic error toast
    genericError: (descriptionKey?: string) =>
      showToast("error", i18nKeyContainer.toast.errorOccurred, {
        description: descriptionKey || i18nKeyContainer.toast.genericErrorDesc,
      }),

    // Generic success toast
    genericSuccess: (titleKey: string, descriptionKey?: string) =>
      showToast("success", titleKey, {
        description: descriptionKey,
      }),
  };
}

// Export toast directly for use outside of React components
export { toast };
