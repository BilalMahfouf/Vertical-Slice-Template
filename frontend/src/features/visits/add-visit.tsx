import { useState, useEffect, useCallback } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Stethoscope, ArrowLeft, ArrowRight, Loader2 } from "lucide-react";
import visitApi, { VisitType, type CreateVisitRequest } from "./visit-api";
import { type Client } from "@/features/clients/client-api";
import { type ClientAnimal } from "@/features/animals/animal-api";
import {
  ClientAnimalStep,
  StepIndicator,
  type ClientAnimalSelection,
} from "@/features/appointments/components";
import { VisitDetailsStep, type VisitFormData } from "./components";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useVisitToast } from "./use-visit-toast";

const STEP_CLIENT_ANIMAL = 1;
const STEP_DETAILS = 2;

interface AddVisitProps {
  open: boolean;
  onClose: () => void;
}

export default function AddVisit({ open, onClose }: AddVisitProps) {
  // Step state
  const [currentStep, setCurrentStep] = useState(STEP_CLIENT_ANIMAL);

  // Selection state (Step 1)
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedAnimal, setSelectedAnimal] = useState<ClientAnimal | null>(
    null
  );

  // Form data (Step 2)
  const [formData, setFormData] = useState<VisitFormData>({
    visitType: VisitType.Clinic,
    symptoms: "",
    diagnosis: "",
    treatment: "",
    notes: "",
  });

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const queryClient = useQueryClient();
  const visitToast = useVisitToast();

  // Define steps
  const steps = [
    {
      number: STEP_CLIENT_ANIMAL,
      title: t(i18nKeyContainer.visit.step1Title),
      description: t(i18nKeyContainer.visit.step1Description),
    },
    {
      number: STEP_DETAILS,
      title: t(i18nKeyContainer.visit.step2Title),
      description: t(i18nKeyContainer.visit.step2Description),
    },
  ];

  const resetForm = () => {
    setSelectedClient(null);
    setSelectedAnimal(null);
    setFormData({
      visitType: VisitType.Clinic,
      symptoms: "",
      diagnosis: "",
      treatment: "",
      notes: "",
    });
  };

  // Reset form when dialog opens/closes
  useEffect(() => {
    const foo= ()=>{
    if (open) {
      resetForm();
      setCurrentStep(STEP_CLIENT_ANIMAL);
    }
    }
    foo();

  }, [open]);

  // Handle selection from Step 1
  const handleClientAnimalSelect = useCallback(
    (selection: ClientAnimalSelection) => {
      setSelectedClient(selection.client);
      setSelectedAnimal(selection.animal);
    },
    []
  );

  // Update form data
  const handleFormDataChange = useCallback((data: Partial<VisitFormData>) => {
    setFormData((prev) => ({ ...prev, ...data }));
  }, []);

  // Helper function to convert comma-separated string to array
  const parseStringToArray = (value: string): string[] | null => {
    if (!value.trim()) return null;
    return value
      .split(",")
      .map((item) => item.trim())
      .filter((item) => item.length > 0);
  };

  // Create mutation
  const createMutation = useMutation({
    mutationFn: () => {
      const request: CreateVisitRequest = {
        animalId: selectedAnimal!.animalId,
        clientId: selectedClient!.id,
        visitType: formData.visitType,
        symptoms: parseStringToArray(formData.symptoms),
        diagnosis: parseStringToArray(formData.diagnosis),
        treatment: parseStringToArray(formData.treatment),
        notes: formData.notes.trim() || null,
      };
      return visitApi.createVisit(request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["visits"] });
      visitToast.created();
      resetForm();
      onClose();
    },
    onError: (error) => {
      visitToast.error(error);
    },
  });

  // Navigation
  const goToNextStep = () => {
    if (currentStep < STEP_DETAILS) {
      setCurrentStep(currentStep + 1);
    }
  };

  const goToPrevStep = () => {
    if (currentStep > STEP_CLIENT_ANIMAL) {
      setCurrentStep(currentStep - 1);
    }
  };

  // Submit handler
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate();
  };

  // Validation
  const isStep1Valid = selectedClient !== null && selectedAnimal !== null;
  const isStep2Valid = formData.visitType > 0;
  const canProceedToStep2 = isStep1Valid;
  const canSubmit = isStep1Valid && isStep2Valid;

  const isSubmitting = createMutation.isPending;

  // Close handler
  const handleClose = () => {
    if (!isSubmitting) {
      resetForm();
      onClose();
    }
  };

  // Arrow icons based on RTL
  const BackIcon = isRtl ? ArrowRight : ArrowLeft;
  const NextIcon = isRtl ? ArrowLeft : ArrowRight;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent
        className="max-w-lg p-0 overflow-hidden bg-white"
        dir={isRtl ? "rtl" : "ltr"}
      >
        {/* Header */}
        <div className="px-6 pt-6 pb-4 border-b border-slate-100">
          <DialogHeader>
            <div className="flex items-center gap-3 mb-4">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                <Stethoscope className="h-5 w-5 text-primary" />
              </div>
              <div>
                <DialogTitle className="text-lg font-semibold text-slate-900">
                  {t(i18nKeyContainer.visit.addTitle)}
                </DialogTitle>
                <DialogDescription className="text-sm text-slate-500">
                  {t(i18nKeyContainer.visit.addDescription)}
                </DialogDescription>
              </div>
            </div>

            {/* Step Indicator */}
            <StepIndicator
              steps={steps}
              currentStep={currentStep}
              className="pt-2"
            />
          </DialogHeader>
        </div>

        {/* Form Content */}
        <form onSubmit={handleSubmit}>
          <div className="px-6 py-5 max-h-[60vh] overflow-y-auto">
            {/* Step 1: Client & Animal Selection */}
            {currentStep === STEP_CLIENT_ANIMAL && (
              <ClientAnimalStep
                onSelect={handleClientAnimalSelect}
                initialClient={selectedClient}
                initialAnimalId={selectedAnimal?.animalId}
                isRtl={isRtl}
              />
            )}

            {/* Step 2: Visit Details */}
            {currentStep === STEP_DETAILS && (
              <VisitDetailsStep
                formData={formData}
                onFormDataChange={handleFormDataChange}
                client={selectedClient}
                animal={selectedAnimal}
                isRtl={isRtl}
              />
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex gap-3 justify-between">
            {/* Left side - Back button (only in step 2) */}
            <div>
              {currentStep === STEP_DETAILS && (
                <Button
                  type="button"
                  variant="outline"
                  onClick={goToPrevStep}
                  disabled={isSubmitting}
                  className="gap-2 cursor-pointer border-0 hover:bg-slate-100"
                >
                  <BackIcon className="h-4 w-4" />
                  {t(i18nKeyContainer.visit.back)}
                </Button>
              )}
            </div>

            {/* Right side - Cancel / Next / Submit */}
            <div className="flex gap-3">
              <Button
                type="button"
                variant="outline"
                onClick={handleClose}
                disabled={isSubmitting}
                className="min-w-20 cursor-pointer border-0 hover:bg-slate-100"
              >
                {t(i18nKeyContainer.common.cancel)}
              </Button>

              {/* Next button (Step 1) */}
              {currentStep === STEP_CLIENT_ANIMAL && (
                <Button
                  type="button"
                  onClick={goToNextStep}
                  disabled={!canProceedToStep2}
                  className="gap-2 min-w-25 cursor-pointer"
                >
                  {t(i18nKeyContainer.visit.next)}
                  <NextIcon className="h-4 w-4" />
                </Button>
              )}

              {/* Submit button (Step 2) */}
              {currentStep === STEP_DETAILS && (
                <Button
                  type="submit"
                  disabled={!canSubmit || isSubmitting}
                  className="min-w-25 cursor-pointer"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="me-2 h-4 w-4 animate-spin" />
                      {t(i18nKeyContainer.visit.creating)}
                    </>
                  ) : (
                    t(i18nKeyContainer.visit.create)
                  )}
                </Button>
              )}
            </div>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
