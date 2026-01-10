import { useMutation, useQueryClient } from "@tanstack/react-query";
import appointmentApi from "./appointment-api";
import { useAppointmentToast } from "./use-appointment-toast";

/**
 * Hook for canceling an appointment with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 */
export function useCancelAppointment() {
  const queryClient = useQueryClient();
  const appointmentToast = useAppointmentToast();

  const mutation = useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      appointmentApi.cancelAppointment(id, { notes }),
    onSuccess: () => {
      // Invalidate appointments list to refetch
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      // Show success toast
      appointmentToast.canceled();
    },
    onError: (error) => {
      // Show error toast with domain-aware message
      appointmentToast.error(error);
    },
  });

  return {
    cancelAppointment: mutation.mutate,
    cancelAppointmentAsync: mutation.mutateAsync,
    isCanceling: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
