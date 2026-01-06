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
        const result = await api.get<PagedList<Client>>('/clients',{params});
        if(result.status !== 200) {
            throw new Error('Failed to fetch clients');
        }
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
    }
}
export default clientApi;