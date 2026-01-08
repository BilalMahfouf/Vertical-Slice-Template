import { useMutation, useQueryClient } from "@tanstack/react-query";
import clientApi from "./client-api";
import { useClientToast } from "./use-client-toast";

/**
 * Hook for deleting a client with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
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
