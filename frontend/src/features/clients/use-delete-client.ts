import { useMutation, useQueryClient } from "@tanstack/react-query";
import clientApi from "./client-api";
import { useClientToast } from "./use-client-toast";
import { isNotFoundError } from "@/lib/api/error-types";

/**
 * Hook for deleting a client with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 * On 404, invalidates queries to refetch data - use isNotFoundError in onError to close dialog
 */
export function useDeleteClient() {
  const queryClient = useQueryClient();
  const clientToast = useClientToast();

  const mutation = useMutation({
    mutationFn: (clientId: string) => clientApi.deleteClientById(clientId),
    onSuccess: () => {
      // Invalidate clients list to refetch
      queryClient.invalidateQueries({ queryKey: ["clients"] });
      // Show success toast
      clientToast.deleted();
    },
    onError: (error) => {
      // If not found, invalidate queries to refetch (item was already deleted)
      if (isNotFoundError(error)) {
        queryClient.invalidateQueries({ queryKey: ["clients"] });
        return;
      }
      // Show error toast with domain-aware message
      clientToast.error(error);
    },
  });

  return {
    deleteClient: mutation.mutate,
    deleteClientAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
