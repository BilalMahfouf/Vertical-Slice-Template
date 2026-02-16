import { useMutation, useQueryClient } from "@tanstack/react-query";
import vaccinationApi from "./vaccination-api";
import { useVaccinationToast } from "./use-vaccination-toast";
import { isNotFoundError } from "@/lib/api/error-types";

/**
 * Hook for deleting a vaccination with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 * On 404, invalidates queries to refetch data - use isNotFoundError in onError to close dialog
 */
export function useDeleteVaccination() {
  const queryClient = useQueryClient();
  const vaccinationToast = useVaccinationToast();

  const mutation = useMutation({
    mutationFn: (id: string) => vaccinationApi.deleteVaccination(id),
    onSuccess: async() => {
      // Invalidate vaccinations list to refetch
      await queryClient.invalidateQueries({ queryKey: ["vaccinations"] });
      // Show success toast
      vaccinationToast.deleted();
    },
    onError: (error) => {
      // If not found, invalidate queries to refetch (item was already deleted)
      if (isNotFoundError(error)) {
        queryClient.invalidateQueries({ queryKey: ["vaccinations"] });
        return;
      }
      // Show error toast with domain-aware message
      vaccinationToast.error(error);
    },
  });

  return {
    deleteVaccination: mutation.mutate,
    deleteVaccinationAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
