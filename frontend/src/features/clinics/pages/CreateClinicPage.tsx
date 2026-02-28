import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useTranslation } from "react-i18next";
import LanguageSwitcher from "@/components/ui/language-switcher";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useCreateClinic } from "@/features/clinics/api/createClinic";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";

const formSchema = z.object({
  name: z.string().min(2, { message: "Name is too short" }),
  phone: z.string().min(5, { message: "Phone is too short" }),
  address: z.string().min(5, { message: "Address is too short" }),
  staffCount: z.number().min(1, { message: "Must have at least 1 staff" }),
});

type FormValues = z.infer<typeof formSchema>;

export default function CreateClinicPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const isRtl = i18n.language === "ar";
  
  const createClinicMutation = useCreateClinic();

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: "",
      phone: "",
      address: "",
      staffCount: 1,
    },
  });

  async function onSubmit(values: FormValues) {
    try {
      await createClinicMutation.mutateAsync(values);
      toast.success(t(i18nKeyContainer.createClinic.successMessage));
      navigate("/dashboard");
    } catch (error: unknown) {
         // Handle Global Errors
         const axiosError = error as { response?: { data?: { errors?: Array<string | { description: string }> } } };
         const errors = axiosError.response?.data?.errors;
         if (Array.isArray(errors)) {
              toast.error(errors.map((e) => typeof e === 'string' ? e : e.description).join(", "));
         } else {
             toast.error("Failed to create clinic.");
         }
    }
  }

  return (
    <div 
        className="min-h-screen flex items-center justify-center bg-slate-50 relative overflow-hidden px-4"
        dir={isRtl ? "rtl" : "ltr"}
    >
      <div className="absolute top-6 end-6 z-20">
        <LanguageSwitcher />
      </div>

      <div className="w-full max-w-md z-10">
        <div className="flex flex-col items-center mb-8">
          <img src="/logo.jpg" alt="AviaMind Vet" className="w-16 h-16 rounded-2xl object-cover shadow-xl shadow-primary/20 mb-4" />
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">AviaMind Vet</h1>
          <p className="text-slate-500 font-medium mt-1">Clinic management, redefined.</p>
        </div>
        <div className="bg-white">
        <Card className="border-slate-200 shadow-2xl shadow-slate-200/50 overflow-hidden">
          <CardHeader className="space-y-1 pb-6 pt-8 text-center border-b border-slate-50">
            <CardTitle className="text-2xl font-bold">{t(i18nKeyContainer.createClinic.title)}</CardTitle>
            <CardDescription className="text-slate-500">
              {t(i18nKeyContainer.createClinic.description)}
            </CardDescription>
          </CardHeader>
        <CardContent className="pt-8 pb-8 px-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5">
              
                <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                    <FormItem>
                        <FormLabel>{t(i18nKeyContainer.createClinic.name)}</FormLabel>
                        <FormControl>
                        <Input placeholder="" {...field} className="bg-slate-50 border-slate-200 h-11 transition-all focus:bg-white" />
                        </FormControl>
                        <FormMessage />
                    </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="phone"
                    render={({ field }) => (
                    <FormItem>
                        <FormLabel>{t(i18nKeyContainer.createClinic.phone)}</FormLabel>
                        <FormControl>
                        <Input placeholder="" {...field} className="bg-slate-50 border-slate-200 h-11 transition-all focus:bg-white" />
                        </FormControl>
                        <FormMessage />
                    </FormItem>
                    )}
                />

              <FormField
                control={form.control}
                name="address"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t(i18nKeyContainer.createClinic.address)}</FormLabel>
                    <FormControl>
                      <Input placeholder="" {...field} className="bg-slate-50 border-slate-200 h-11 transition-all focus:bg-white" />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="staffCount"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{t(i18nKeyContainer.createClinic.staffCount)}</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min={1}
                        placeholder="1"
                        className="bg-slate-50 border-slate-200 h-11 transition-all focus:bg-white"
                        value={field.value}
                        onChange={(e) => field.onChange(Number(e.target.value) || 1)}
                        onBlur={field.onBlur}
                        name={field.name}
                        ref={field.ref}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <Button type="submit" className="w-full cursor-pointer bg-primary text-white h-11 font-bold text-lg shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all" disabled={createClinicMutation.isPending}>
                {createClinicMutation.isPending && <Loader2 className="me-2 h-4 w-4 animate-spin" />}
                {t(i18nKeyContainer.createClinic.submit)}
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>
        </div>
        <p className="mt-8 text-center text-sm text-slate-400 font-medium">
          © 2026 AviaMind Vet. Built for professionals.
        </p>
      </div>
    </div>
  );
}
