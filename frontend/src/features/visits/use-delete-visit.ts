import { useMutation, useQueryClient } from "@tanstack/react-query";
import visitApi from "./visit-api";
import { useVisitToast } from "./use-visit-toast";

/**
 * Hook for deleting a visit with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 */
export function useDeleteVisit() {
  const queryClient = useQueryClient();
  const visitToast = useVisitToast();

  const mutation = useMutation({
    mutationFn: (id: string) => visitApi.deleteVisit(id),
    onSuccess: () => {
      // Invalidate visits list to refetch
      queryClient.invalidateQueries({ queryKey: ["visits"] });
      // Show success toast
      visitToast.deleted();
    },
    onError: (error) => {
      // Show error toast with domain-aware message
      visitToast.error(error);
    },
  });

  return {
    deleteVisit: mutation.mutate,
    deleteVisitAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
