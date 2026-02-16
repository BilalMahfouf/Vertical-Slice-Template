import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

export type VaccinationTableResponse = {
  id: string;
  vaccinationName: string;
  givenAt: string; // ISO 8601 datetime string
  dueTo: string | null; // ISO 8601 datetime string
  clientName: string;
  animalName: string;
  createdOnUtc: string;
};

export type VaccinationResponse = {
  id: string;
  animalId: string;
  animalName: string;
  clientId: string;
  clientName: string;
  visitId: string | null;
  vaccinationName: string;
  givenAt: string; // ISO 8601 datetime string
  dueTo: string | null; // ISO 8601 datetime string
  notes: string | null;
};

export interface CreateVaccinationRequest {
  animalId?: string | null;
  visitId?: string |null;
  name: string;
  givenAt: string;
  dueTo?: string | null;
  notes?: string | null;
}

export interface UpdateVaccinationRequest {
  name: string;
  givenAt: string;
  dueTo?: string | null;
  notes?: string | null;
}

const vaccinationApi = {
  /**
   * Fetches paginated list of vaccinations with optional search and sorting
   * @param request - Table request with pagination, sorting, and search parameters
   * @returns PagedList of VaccinationTableResponse objects
   */
  getAllVaccinations: async (request: TableRequest): Promise<PagedList<VaccinationTableResponse>> => {
    const params = getTableRequsestParams(request);
    const response = await api.get<PagedList<VaccinationTableResponse>>('/vaccinations', { params });
    if (response.status !== 200) {
      throw new Error('Failed to fetch vaccinations');
    }
    return response.data;
  },

  /**
   * Fetches detailed information for a specific vaccination by ID
   * @param id - Vaccination ID (GUID)
   * @returns VaccinationResponse object with complete vaccination information
   */
  getVaccinationById: async (id: string): Promise<VaccinationResponse> => {
    const response = await api.get<VaccinationResponse>(`/vaccinations/${id}`);
    if (response.status !== 200) {
      throw new Error('Failed to fetch vaccination');
    }
    return response.data;
  },

  /**
   * Creates a new vaccination record
   * @param data - CreateVaccinationRequest payload
   * @returns Created vaccination ID (GUID)
   */
  createVaccination: async (data: CreateVaccinationRequest): Promise<string> => {
    const response = await api.post<{ id: string }>('/vaccinations', data);
    console.log('Create response:', response);
    if (response.status !== 201 && response.status !== 200) {
      throw new Error('Failed to create vaccination');
    }
    return response.data.id;
  },

  /**
   * Updates an existing vaccination record
   * @param id - Vaccination ID (GUID)
   * @param data - UpdateVaccinationRequest payload
   */
  updateVaccination: async (id: string, data: UpdateVaccinationRequest): Promise<void> => {
    const response = await api.put(`/vaccinations/${id}`, data);
    console.log('Update response:', response);
    if (response.status !== 200 && response.status !== 204) {
      throw new Error('Failed to update vaccination');
    }
  },

  /**
   * Deletes a vaccination record
   * @param id - Vaccination ID (GUID)
   */
  deleteVaccination: async (id: string): Promise<void> => {
    const response = await api.delete(`/vaccinations/${id}`);
    if (response.status !== 200 && response.status !== 204) {
      throw new Error('Failed to delete vaccination');
    }
  },
};

export default vaccinationApi;
