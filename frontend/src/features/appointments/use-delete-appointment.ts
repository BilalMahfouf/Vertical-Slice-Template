import { useMutation, useQueryClient } from "@tanstack/react-query";
import appointmentApi from "./appointment-api";
import { useAppointmentToast } from "./use-appointment-toast";
import { isNotFoundError } from "@/lib/api/error-types";

/**
 * Hook for deleting an appointment with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 * On 404, invalidates queries to refetch data - use isNotFoundError in onError to close dialog
 */
export function useDeleteAppointment() {
  const queryClient = useQueryClient();
  const appointmentToast = useAppointmentToast();

  const mutation = useMutation({
    mutationFn: (id: string) => appointmentApi.deleteAppointmentById(id),
    onSuccess: () => {
      // Invalidate appointments list to refetch
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      // Show success toast
      appointmentToast.deleted();
    },
    onError: (error) => {
      // If not found, invalidate queries to refetch (item was already deleted)
      if (isNotFoundError(error)) {
        queryClient.invalidateQueries({ queryKey: ["appointments"] });
        return;
      }
      // Show error toast with domain-aware message
      appointmentToast.error(error);
    },
  });

  return {
    deleteAppointment: mutation.mutate,
    deleteAppointmentAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
