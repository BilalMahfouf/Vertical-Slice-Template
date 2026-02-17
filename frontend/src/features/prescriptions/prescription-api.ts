import api from "@/lib/api/api";

export type PrescriptionRequest = {
  patientFullName: string | null;
  animalType: string | null;
  patientAge: string | null;
  patientWeight: string | null;
  date: string | null;
  medicines: string[] | null;
};

const downloadBlob = (blob: Blob, filename: string) => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.setAttribute("download", filename);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
};

export const prescriptionApi = {
  getPrescription: async (data: PrescriptionRequest) => {
    const response = await api.post("/prescriptions", data, {
      responseType: "blob",
    });
    const filename = "prescription.pdf";
    downloadBlob(new Blob([response.data], { type: "application/pdf" }), filename);
  },

  getEmptyPrescription: async () => {
    const response = await api.get("/prescriptions/empty", {
      responseType: "blob",
    });
    const filename = "empty-prescription.pdf";
    downloadBlob(new Blob([response.data], { type: "application/pdf" }), filename);
  },
};
