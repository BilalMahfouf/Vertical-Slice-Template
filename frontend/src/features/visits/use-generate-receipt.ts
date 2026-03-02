import { useMutation } from "@tanstack/react-query";
import visitApi from "./visit-api";
import { useVisitToast } from "./use-visit-toast";

/**
 * Hook for generating and downloading a visit receipt
 * Handles API call, loading state, and success/error toasts
 */
export function useGenerateReceipt() {
  const visitToast = useVisitToast();

  const mutation = useMutation({
    mutationFn: (id: string) => visitApi.getVisitReceipt(id),
    onSuccess: () => {
      visitToast.receiptGenerated();
    },
    onError: (error) => {
      visitToast.error(error);
    },
  });

  return {
    generateReceipt: mutation.mutate,
    generateReceiptAsync: mutation.mutateAsync,
    isGenerating: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
