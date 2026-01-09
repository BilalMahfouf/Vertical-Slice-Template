import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

export type Appointment = {
  id: string;
  clinicId: string;
  clientId: string;
  clientName: string;
  animalName: string;
  appointmentDate: string;
  status: string;
  createdOnUtc: string;
}

export interface CreateAppointmentRequest {
  animalId: string;
  appointmentDate: string;
  appointmentTime: string;
  location: string;
  notes?: string | null;
}

export interface RescheduleAppointmentRequest {
  newAppointmentDate: string;
}

export interface CancelAppointmentRequest {
  notes?: string | null;
}

export const AppointmentStatus = {
  Confirmed: 1,
  Cancelled: 2,
  Completed: 3,
  Rescheduled: 4,
} as const;

const appointmentApi = {
  getAllAppointments: async (request: TableRequest): Promise<PagedList<Appointment>> => {
    const params = getTableRequsestParams(request);

    const response = await api.get<PagedList<Appointment>>('/appointments', { params });
    if (response.status !== 200) {
      throw new Error('Failed to fetch appointments');
    }
    return response.data;
  },
  createAppointment: async (data: CreateAppointmentRequest): Promise<string> => {
    const response = await api.post<{ id: string }>('/appointments', data);
    if (response.status !== 201) {
      throw new Error('Failed to create appointment');
    }
    return response.data.id;
  },
  getAppointmentById: async (id: string): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/appointments/${id}`);
    if (response.status !== 200) {
      throw new Error('Failed to fetch appointment');
    }
    return response.data;
  },
  rescheduleAppointment: async (id: string, data: RescheduleAppointmentRequest): Promise<void> => {
    const response = await api.put<void>(`/appointments/${id}/reschedule`, data);
    if (response.status !== 204) {
      throw new Error('Failed to reschedule appointment');
    }
    return;
  },
  cancelAppointment: async (id: string, data: CancelAppointmentRequest): Promise<void> => {
    const response = await api.patch<void>(`/appointments/${id}/cancel`, data);
    if (response.status !== 204) {
      throw new Error('Failed to cancel appointment');
    }
    return;
  },
  deleteAppointmentById: async (id: string): Promise<void> => {
    const response = await api.delete<void>(`/appointments/${id}`);
    if (response.status !== 204) {
      throw new Error('Failed to delete appointment');
    }
    return;
  },
}

export default appointmentApi;