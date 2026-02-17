import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";
import { FileText, Plus, Trash2, Loader2, User, PawPrint, Calendar, Scale, Pill } from "lucide-react";
import { prescriptionApi, type PrescriptionRequest } from "./prescription-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { toast } from "sonner";

const MAX_MEDICINES = 10;

const inputClasses = "rounded-md border border-slate-200 bg-white px-3 py-2.5 text-sm placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary resize-none placeholder:font-normal";
const buttonClasses = "cursor-pointer transition-colors border-slate-200 hover:bg-slate-100 focus:bg-slate-100 disabled:cursor-not-allowed disabled:bg-transparent disabled:opacity-50";

const prescriptionSchema = z.object({
  patientFullName: z.string().optional(),
  animalType: z.string().optional(),
  patientAge: z.string().optional(),
  patientWeight: z.string().optional(),
  date: z.string().optional(),
});

type PrescriptionFormValues = z.infer<typeof prescriptionSchema>;

interface GetPrescriptionDialogProps {  
  open: boolean;
  onClose: () => void;
}

export default function GetPrescriptionDialog({
  open,
  onClose,
}: GetPrescriptionDialogProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  const [medicines, setMedicines] = useState<string[]>([]);
  const [medicineInput, setMedicineInput] = useState("");

  const form = useForm<PrescriptionFormValues>({
    resolver: zodResolver(prescriptionSchema),
    defaultValues: {
      patientFullName: "",
      animalType: "",
      patientAge: "",
      patientWeight: "",
      date: "",
    },
  });

  // Mutation for full prescription
  const fullPrescriptionMutation = useMutation({
    mutationFn: (data: PrescriptionRequest) => prescriptionApi.getPrescription(data),
    onSuccess: () => {
      toast.success(t(i18nKeyContainer.prescription.successMessage));
      handleClose();
    },
    onError: () => {
      toast.error(t(i18nKeyContainer.prescription.errorMessage));
    },
  });

  // Mutation for empty prescription
  const emptyPrescriptionMutation = useMutation({
    mutationFn: () => prescriptionApi.getEmptyPrescription(),
    onSuccess: () => {
      toast.success(t(i18nKeyContainer.prescription.successMessage));
      handleClose();
    },
    onError: () => {
      toast.error(t(i18nKeyContainer.prescription.errorMessage));
    },
  });

  const isLoading =
    fullPrescriptionMutation.isPending || emptyPrescriptionMutation.isPending;

  const handleClose = () => {
    if (isLoading) return;
    form.reset();
    setMedicines([]);
    setMedicineInput("");
    onClose();
  };

  const handleAddMedicine = () => {
    const trimmedValue = medicineInput.trim();
    if (!trimmedValue) return;
    if (medicines.length >= MAX_MEDICINES) return;

    setMedicines((prev) => [...prev, trimmedValue]);
    setMedicineInput("");
  };

  const handleRemoveMedicine = (index: number) => {
    setMedicines((prev) => prev.filter((_, i) => i !== index));
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleAddMedicine();
    }
  };

  const onSubmitFull = (values: PrescriptionFormValues) => {
    const request: PrescriptionRequest = {
      patientFullName: values.patientFullName || null,
      animalType: values.animalType || null,
      patientAge: values.patientAge || null,
      patientWeight: values.patientWeight || null,
      date: values.date || null,
      medicines: medicines.length > 0 ? medicines : null,
    };
    fullPrescriptionMutation.mutate(request);
  };

  const handleGetEmpty = () => {
    emptyPrescriptionMutation.mutate();
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && handleClose()}>
      <DialogContent
        dir={isRtl ? "rtl" : "ltr"}
        className="sm:max-w-lg max-h-[90vh] overflow-y-auto bg-white border-slate-200"
      >
        <DialogHeader>
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10">
              <FileText className="h-5 w-5 text-primary" />
            </div>
            <div>
              <DialogTitle>{t(i18nKeyContainer.prescription.title)}</DialogTitle>
              <DialogDescription>
                {t(i18nKeyContainer.prescription.description)}
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(onSubmitFull)}
            className="space-y-4 mt-4"
          >
            {/* Patient Full Name */}
            <FormField
              control={form.control}
              name="patientFullName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="flex items-center gap-2">
                    <User className="h-4 w-4 text-muted-foreground" />
                    {t(i18nKeyContainer.prescription.patientFullName)}
                  </FormLabel>
                  <FormControl>
                    <Input {...field} disabled={isLoading} className={inputClasses} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Animal Type */}
            <FormField
              control={form.control}
              name="animalType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="flex items-center gap-2">
                    <PawPrint className="h-4 w-4 text-muted-foreground" />
                    {t(i18nKeyContainer.prescription.animalType)}
                  </FormLabel>
                  <FormControl>
                    <Input {...field} disabled={isLoading} className={inputClasses} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Two column layout for Age and Weight */}
            <div className="grid grid-cols-2 gap-4">
              {/* Patient Age */}
              <FormField
                control={form.control}
                name="patientAge"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      {t(i18nKeyContainer.prescription.patientAge)}
                    </FormLabel>
                    <FormControl>
                      <Input {...field} disabled={isLoading} className={inputClasses} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Patient Weight */}
              <FormField
                control={form.control}
                name="patientWeight"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="flex items-center gap-2">
                      <Scale className="h-4 w-4 text-muted-foreground" />
                      {t(i18nKeyContainer.prescription.patientWeight)}
                    </FormLabel>
                    <FormControl>
                      <Input {...field} disabled={isLoading} className={inputClasses} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Date */}
            <FormField
              control={form.control}
              name="date"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    {t(i18nKeyContainer.prescription.date)}
                  </FormLabel>
                  <FormControl>
                    <Input type="date" {...field} disabled={isLoading} className={inputClasses} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Medicines Section */}
            <div className="space-y-2">
              <Label className="flex items-center gap-2">
                <Pill className="h-4 w-4 text-muted-foreground" />
                {t(i18nKeyContainer.prescription.medicines)}
              </Label>

              {/* Medicine Input */}
              <div className="flex gap-2">
                <Input
                  value={medicineInput}
                  onChange={(e) => setMedicineInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder={t(i18nKeyContainer.prescription.medicineName)}
                  disabled={isLoading || medicines.length >= MAX_MEDICINES}
                  className={cn("flex-1", inputClasses)}
                />
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={handleAddMedicine}
                  disabled={
                    isLoading ||
                    !medicineInput.trim() ||
                    medicines.length >= MAX_MEDICINES
                  }
                  className={cn("shrink-0", buttonClasses)}
                >
                  <Plus className="h-4 w-4" />
                </Button>
              </div>

              {/* Max medicines hint */}
              {medicines.length >= MAX_MEDICINES && (
                <p className="text-xs text-muted-foreground">
                  {t(i18nKeyContainer.prescription.maxMedicines)}
                </p>
              )}

              {/* Medicines List */}
              {medicines.length > 0 ? (
                <ul className="space-y-1 mt-2">
                  {medicines.map((medicine, index) => (
                    <li
                      key={index}
                      className="flex items-center justify-between gap-2 rounded-md border border-slate-200 px-3 py-2 text-sm"
                    >
                      <span>{medicine}</span>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => handleRemoveMedicine(index)}
                        disabled={isLoading}
                        className={cn("h-6 w-6 shrink-0 text-destructive hover:text-destructive", buttonClasses)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-sm text-muted-foreground">
                  {t(i18nKeyContainer.prescription.noMedicines)}
                </p>
              )}
            </div>

            {/* Footer Buttons */}
            <DialogFooter className="flex-row gap-2 sm:gap-2">
              <Button
                type="button"
                variant="ghost"
                onClick={handleClose}
                disabled={isLoading}
                className={cn("me-auto", buttonClasses)}
              >
                {t(i18nKeyContainer.common.cancel)}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={handleGetEmpty}
                disabled={isLoading}
                className={buttonClasses}
              >
                {emptyPrescriptionMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 me-2 animate-spin" />
                    {t(i18nKeyContainer.prescription.downloading)}
                  </>
                ) : (
                  t(i18nKeyContainer.prescription.getEmptyPrescription)
                )}
              </Button>
              <Button
                type="submit"
                disabled={isLoading}
                className="cursor-pointer hover:bg-primary focus:bg-primary disabled:cursor-not-allowed isabled:opacity-50 "
              >
                {fullPrescriptionMutation.isPending ? (
                  <>
                    <Loader2 className="h-4 w-4 me-2 animate-spin" />
                    {t(i18nKeyContainer.prescription.generating)}
                  </>
                ) : (
                  t(i18nKeyContainer.prescription.getFullPrescription)
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
