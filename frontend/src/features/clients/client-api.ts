import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

export type Client = {
    id: string;           // Guid
  clinicId: string;     // Guid
  clinicName: string;   // string
  fullName: string;     // string
  phone: string;        // string
  notes?: string;       // string | null
  numberOfAnimals: number; // number of animals for this client
  createdOnUtc: string; // ISO Date string 
}
export type CreateClientRequest = {
    firstName: string;
  lastName: string;
  phone: string;
  notes?: string;
}

const clientApi ={
    getAllClients: async (request: TableRequest) : Promise<PagedList<Client>> => {
       const params = getTableRequsestParams(request);
       console.log("fetching with params",params);
        const result = await api.get<PagedList<Client>>('/clients',{params});
        if(result.status !== 200) {
            throw new Error('Failed to fetch clients');
        }
        console.log("data: ",result.data);
        return result.data; 
    },
    addClient: async (client:CreateClientRequest): Promise<string> => {
         const result = await api.post<string>('/clients',client);
         if(result.status !== 201) {
            throw new Error('Failed to add client');
         }
         return result.data;
    },
    getClientById: async (id:string): Promise<Client> => {
            const result = await api.get<Client>(`/clients/${id}`);
            if(result.status !== 200) {
                throw new Error('Failed to fetch client');
            }
            return result.data;
    },
    updateClient: async(id:string,request : CreateClientRequest) : Promise<void> => {
        const result = await api.put<void>(`/clients/${id}`,request);
        if(result.status !== 204) {
            throw new Error('Failed to update client');
        }
        return;
    },
    getClientBySearch: async (search:string): Promise<PagedList<Client>> => {
        const params = new URLSearchParams();
        params.append('search', search);
        const result = await api.get<PagedList<Client>>('/clients',{params});
        if(result.status !== 200) {
            throw new Error('Failed to fetch clients');
        }
        return result.data;
    },
    deleteClientById: async (id:string): Promise<void> => {
        const result = await api.delete<void>(`/clients/${id}`);
        if(result.status !== 204) {
            throw new Error('Failed to delete client');
        }
        return;
    },
    getClientByName: async (name:string): Promise<Client> => {
        const result = await api.get<Client>(`/clients/by-name/${name}`);
        if(result.status !== 200) {
            throw new Error('Failed to fetch client by name');
        }
        return result.data;
    }
}
export default clientApi;