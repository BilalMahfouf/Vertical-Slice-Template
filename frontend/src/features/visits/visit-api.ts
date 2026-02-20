import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

export const PaymentStatus = {
  Pending: 1,
  Paid: 2,
  PartiallyPaid: 3,
  Refunded: 4,
} as const;

export type PaymentStatus = (typeof PaymentStatus)[keyof typeof PaymentStatus];

export type VisitTableResponse = {
  id: string;
  visitType: string;
  animalId: string;
  animalName: string;
  animalSpecies: string;
  ownerId: string;
  ownerName: string;
  visitDate: string; // ISO 8601 datetime string
  paymentAmount: number;
  paymentStatus: string;
}


export type VisitResponse = {
  id: string;
  animalId: string;
  animalName: string;
  animalSpecies: string;
  animalBreed: string | null;
  clientId: string;
  clientFullName: string;
  clientPhone: string;
  appointmentId: string | null;
  appointmentDate: string | null; // ISO 8601 datetime string
  appointmentStatus: string | null;
  visitType: string;
  symptoms: string[] | null;
  diagnosis: string[] | null;
  treatment: string[] | null;
  notes: string | null;
  createdOnUtc: string; // ISO 8601 datetime string
  updatedOnUtc: string | null; // ISO 8601 datetime string
  paymentAmount: number;
  paymentStatus: string;
}

export interface CreateVisitRequest {
  animalId?: string;
  clientId?: string;
  appointmentId?: string | null;
  visitType: number; // Enum value: 1=Clinic, 2=Field, 3=Emergency
  symptoms?: string[] | null;
  diagnosis?: string[] | null;
  treatment?: string[] | null;
  notes?: string | null;
  paymentAmount: number;
  paymentStatus: number;
}

export interface UpdateVisitRequest {
  visitType: number; // Enum value: 1=Clinic, 2=Field, 3=Emergency
  symptoms?: string[] | null;
  diagnosis?: string[] | null;
  treatment?: string[] | null;
  notes?: string | null;
  paymentAmount: number;
  paymentStatus: number;
}


export const VisitType = {
  Clinic: 1,
  Field: 2,
  Emergency: 3,
} as const;

const visitApi = {
  /**
   * Fetches paginated list of visits with optional search and sorting
   * @param request - Table request with pagination, sorting, and search parameters
   * @returns PagedList of VisitTableResponse objects
   */
  getAllVisits: async (request: TableRequest): Promise<PagedList<VisitTableResponse>> => {
    const params = getTableRequsestParams(request);

    const response = await api.get<PagedList<VisitTableResponse>>('/visits', { params });
    if (response.status !== 200) {
      throw new Error('Failed to fetch visits');
    }
    return response.data;
  },

  /**
   * Fetches detailed information for a specific visit by ID
   * @param id - Visit ID (GUID)
   * @returns VisitResponse object with complete visit information
   */
  getVisitById: async (id: string): Promise<VisitResponse> => {
    const response = await api.get<VisitResponse>(`/visits/${id}`);
    if (response.status !== 200) {
      throw new Error('Failed to fetch visit');
    }
    return response.data;
  },

  /**
   * Creates a new visit record
   * @param data - CreateVisitRequest payload
   * @returns Created visit ID (GUID)
   */
  createVisit: async (data: CreateVisitRequest): Promise<string> => {
    const response = await api.post<{ id: string }>('/visits', data);
    if (response.status !== 201) {
      throw new Error('Failed to create visit');
    }
    return response.data.id;
  },

  /**
   * Updates an existing visit record
   * @param id - Visit ID (GUID)
   * @param data - UpdateVisitRequest payload
   * @returns void
   */
  updateVisit: async (id: string, data: UpdateVisitRequest): Promise<void> => {
    const response = await api.put(`/visits/${id}`, data);
    if (response.status !== 200 && response.status !== 204) {
      throw new Error('Failed to update visit');
    }
  },

  /**
   * Deletes a visit record by ID
   * @param id - Visit ID (GUID)
   * @returns void
   */
  deleteVisit: async (id: string): Promise<void> => {
    const response = await api.delete(`/visits/${id}`);
    if (response.status !== 200 && response.status !== 204) {
      throw new Error('Failed to delete visit');
    }
  },
}

export default visitApi;