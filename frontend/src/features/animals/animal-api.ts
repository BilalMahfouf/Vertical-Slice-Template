import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

export type Animal = {
  id: string;
  clinicId: string;
  clientId: string;
  clientName: string;
  clientPhone: string;
  name: string;
  species: string;
  breed: string | null;
  gender: string;
  birthDate: string | null;
  color: string | null;
  microchipNumber: string | null;
  createdOnUtc: string;
  status: string;
}

// Simplified animal type for GetAnimalsByClientId endpoint
export type ClientAnimal = {
  animalId: string;
  name: string;
  species: string;
}
export interface CreateAnimalRequest {
  clientId: string;
  name: string;
  species: string;
  breed?: string | null;
  gender: number;
  birthDate?: string | null;
  color?: string | null;
  microchipNumber?: string | null;
  status: number;
}
export interface UpdateAnimalRequest {
  name: string;
  species: string;
  breed?: string | null;
  gender: number;
  birthDate?: string | null;
  color?: string | null;
  microchipNumber?: string | null;
  status: number;
}

export const AnimalStatus = [
  { value: 1, label: "Active" },
  { value: 2, label: "UnderTreatment" },
  { value: 3, label: "Recovered" },
  { value: 4, label: "Critical" },
  { value: 5, label: "Deceased" }
] as const;
export const Gender = [
  { value: 1, label: "Male" },
  { value: 2, label: "Female" }
] as const;

const animalApi = {
    getAllAnimals: async (request: TableRequest):Promise<PagedList<Animal>> => {
        const params = getTableRequsestParams(request);

        const response = await api.get<PagedList<Animal>>('/animals',{params});
        if(response.status !== 200){
            throw new Error('Failed to fetch animals');
        }
        return response.data;
    },
    createAnimal: async(data: CreateAnimalRequest):Promise<string> => {
        const response = await api.post<{id: string}>('/animals',data);
        if(response.status !== 201){
            throw new Error('Failed to create animal');
        }
        return response.data.id;
    },
    updateAnimal: async(id:string, data: UpdateAnimalRequest):Promise<void> => {
        const response = await api.put<void>(`/animals/${id}`,data);
        if(response.status !== 204){
            throw new Error('Failed to update animal');
        }
        return;
    },
    getAnimalById: async(id:string):Promise<Animal> => {
        const response = await api.get<Animal>(`/animals/${id}`);
        if(response.status !== 200){
            throw new Error('Failed to fetch animal');
        }
        return response.data;
    },
    deleteAnimalById: async(id:string):Promise<void> => {
        const response = await api.delete<void>(`/animals/${id}`);
        if(response.status !== 204){
            throw new Error('Failed to delete animal');
        }
        return;
    },
    getAnimalsByClientId: async(clientId: string): Promise<ClientAnimal[]> => {
        console.log("Fetching animals for clientId:", clientId);
        const response = await api.get<PagedList<ClientAnimal>>(`/animals/by-client/${clientId}`);
        if(response.status !== 200){
            throw new Error('Failed to fetch client animals');
        }
        console.log("Fetched animals:", response.data.item);
        return response.data.item;
    },
}
export default  animalApi;