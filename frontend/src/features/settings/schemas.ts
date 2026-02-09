import { z } from "zod";

export const profileFormSchema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  userName: z.string().min(1, "Username is required"),
  email: z.string().email("Invalid email address"),
});

export type ProfileFormValues = z.infer<typeof profileFormSchema>;

export const clinicFormSchema = z.object({
  name: z.string().min(3, "Clinic name must be at least 3 characters"),
  address: z.string().min(1, "Address is required"),
  phone: z.string().min(1, "Phone number is required"),
  staffCount: z.number().min(0, "Staff count must be 0 or more"),
});

export type ClinicFormValues = z.infer<typeof clinicFormSchema>;
