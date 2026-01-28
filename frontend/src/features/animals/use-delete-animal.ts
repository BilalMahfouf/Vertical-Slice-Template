import { useMutation, useQueryClient } from "@tanstack/react-query";
import animalApi from "./animal-api";
import { useAnimalToast } from "./use-animal-toast";

/**
 * Hook for deleting an animal with toast notifications
 * Handles API call, cache invalidation, and success/error toasts
 */
export function useDeleteAnimal() {
  const queryClient = useQueryClient();
  const animalToast = useAnimalToast();

  const mutation = useMutation({
    mutationFn: (animalId: string) => animalApi.deleteAnimalById(animalId),
    onSuccess: () => {
      // Invalidate animals list to refetch
      queryClient.invalidateQueries({ queryKey: ["animals"] });
      // Show success toast
      animalToast.deleted();
    },
    onError: (error) => {
      // Show error toast with domain-aware message
      animalToast.error(error);
    },
  });

  return {
    deleteAnimal: mutation.mutate,
    deleteAnimalAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}
