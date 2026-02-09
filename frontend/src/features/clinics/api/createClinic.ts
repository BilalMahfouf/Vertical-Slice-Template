import api from "@/lib/api/api";
import { useMutation } from "@tanstack/react-query";

export interface CreateClinicRequest {
  name: string;
  phone: string;
  address: string;
  staffCount: number;
}

export interface CreateClinicResponse {
    clinicId: string;
}

export const createClinic = async (data: CreateClinicRequest): Promise<CreateClinicResponse> => {
  const response = await api.post<CreateClinicResponse>("/clinics", data);
  return response.data;
};

export const useCreateClinic = () => {
    return useMutation({
        mutationFn: createClinic,
    });
};
