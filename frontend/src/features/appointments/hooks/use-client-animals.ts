import { useQuery } from "@tanstack/react-query";
import animalApi, { type ClientAnimal } from "@/features/animals/animal-api";

/**
 * Hook to fetch animals belonging to a specific client
 * Used in the appointment form to select an animal after client selection
 */
export function useClientAnimals(clientId: string | null) {
  return useQuery<ClientAnimal[], Error>({
    queryKey: ["animals", "by-client", clientId],
    queryFn: () => animalApi.getAnimalsByClientId(clientId!),
    enabled: !!clientId,
    staleTime: 30000, // 30 seconds
  });
}
