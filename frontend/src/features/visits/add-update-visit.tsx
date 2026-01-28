import { useState, useEffect, useCallback } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Stethoscope, ArrowLeft, ArrowRight, Loader2, Calendar, User } from "lucide-react";
import visitApi, {
  VisitType,
  type CreateVisitRequest,
  type UpdateVisitRequest,
} from "./visit-api";
import { type Client } from "@/features/clients/client-api";
import { type ClientAnimal } from "@/features/animals/animal-api";
import { type Appointment } from "@/features/appointments/appointment-api";
import {
  ClientAnimalStep,
  StepIndicator,
  type ClientAnimalSelection,
} from "@/features/appointments/components";
import {
  VisitDetailsStep,
  AppointmentSelectionStep,
  type VisitFormData,
  type AppointmentSelection,
} from "./components";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useVisitToast } from "./use-visit-toast";

type VisitMode = "appointment" | "client";

const MODE_CREATE = "create";
const MODE_UPDATE = "update";

const STEP_CLIENT_ANIMAL = 1;
const STEP_DETAILS = 2;

interface AddUpdateVisitProps {
  open: boolean;
  onClose: () => void;
  /** Existing visit ID for update mode */
  visitId?: string | null;
}

// Helper function to convert comma-separated string to array
const parseStringToArray = (value: string): string[] | null => {
  if (!value.trim()) return null;
  return value
    .split(",")
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
};

// Helper function to convert array to comma-separated string
const parseArrayToString = (value: string[] | null | undefined): string => {
  if (!value || value.length === 0) return "";
  return value.join(", ");
};

// Map visit type string to enum value
const getVisitTypeValue = (visitType: string): number => {
  const normalizedType = visitType.toLowerCase();
  switch (normalizedType) {
    case "clinic":
      return VisitType.Clinic;
    case "field":
      return VisitType.Field;
    case "emergency":
      return VisitType.Emergency;
    default:
      return VisitType.Clinic;
  }
};

export default function AddUpdateVisit({
  open,
  onClose,
  visitId,
}: AddUpdateVisitProps) {
  const mode = visitId ? MODE_UPDATE : MODE_CREATE;
  const isUpdateMode = mode === MODE_UPDATE;

  // Visit mode state (appointment vs client) - default to appointment
  const [visitMode, setVisitMode] = useState<VisitMode>("appointment");

  // Step state
  const [currentStep, setCurrentStep] = useState(
    isUpdateMode ? STEP_DETAILS : STEP_CLIENT_ANIMAL
  );

  // Selection state (Step 1 - Client mode)
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedAnimal, setSelectedAnimal] = useState<ClientAnimal | null>(
    null
  );

  // Selection state (Step 1 - Appointment mode)
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);

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

  // Fetch existing visit data for update mode
  const { data: existingVisit, isLoading: isLoadingVisit } = useQuery({
    queryKey: ["visit", visitId],
    queryFn: () => visitApi.getVisitById(visitId!),
    enabled: isUpdateMode && open && !!visitId,
  });

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
    setSelectedAppointment(null);
    setVisitMode("appointment");
    setFormData({
      visitType: VisitType.Clinic,
      symptoms: "",
      diagnosis: "",
      treatment: "",
      notes: "",
    });
  };

  // Populate form when existing visit data is loaded
  useEffect(() => {
    const populateForm = () => {
      if (isUpdateMode && existingVisit && open) {
        // Pre-fill form data from existing visit
        setFormData({
          visitType: getVisitTypeValue(existingVisit.visitType),
          symptoms: parseArrayToString(existingVisit.symptoms),
          diagnosis: parseArrayToString(existingVisit.diagnosis),
          treatment: parseArrayToString(existingVisit.treatment),
          notes: existingVisit.notes || "",
        });
        setCurrentStep(STEP_DETAILS);
      }
    };
    populateForm();
  }, [existingVisit, isUpdateMode, open]);

  // Reset form when dialog opens/closes in create mode
  useEffect(() => {
    const initializeForm = () => {
      if (open && !isUpdateMode) {
        resetForm();
        setCurrentStep(STEP_CLIENT_ANIMAL);
      }
    };
    initializeForm();
  }, [open, isUpdateMode]);

  // Handle selection from Step 1
  const handleClientAnimalSelect = useCallback(
    (selection: ClientAnimalSelection) => {
      setSelectedClient(selection.client);
      setSelectedAnimal(selection.animal);
    },
    []
  );

  // Handle appointment selection from Step 1 (Appointment mode)
  const handleAppointmentSelect = useCallback(
    (selection: AppointmentSelection) => {
      setSelectedAppointment(selection.appointment);
    },
    []
  );

  // Handle mode switch
  const handleModeChange = useCallback((newMode: string) => {
    setVisitMode(newMode as VisitMode);
    // Reset selections when switching modes
    setSelectedClient(null);
    setSelectedAnimal(null);
    setSelectedAppointment(null);
  }, []);

  // Update form data
  const handleFormDataChange = useCallback((data: Partial<VisitFormData>) => {
    setFormData((prev) => ({ ...prev, ...data }));
  }, []);

  // Create mutation
  const createMutation = useMutation({
    mutationFn: () => {
      // Build request based on mode
      const request: CreateVisitRequest = visitMode === "appointment"
        ? {
            // Appointment mode: use appointmentId, backend will resolve client/animal
            appointmentId: selectedAppointment!.id,
            visitType: formData.visitType,
            symptoms: parseStringToArray(formData.symptoms),
            diagnosis: parseStringToArray(formData.diagnosis),
            treatment: parseStringToArray(formData.treatment),
            notes: formData.notes.trim() || null,
          }
        : {
            // Client mode: use selected client/animal
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
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      visitToast.created();
      resetForm();
      onClose();
    },
    onError: (error) => {
      visitToast.error(error);
    },
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: () => {
      const request: UpdateVisitRequest = {
        visitType: formData.visitType,
        symptoms: parseStringToArray(formData.symptoms),
        diagnosis: parseStringToArray(formData.diagnosis),
        treatment: parseStringToArray(formData.treatment),
        notes: formData.notes.trim() || null,
      };
      return visitApi.updateVisit(visitId!, request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["visits"] });
      queryClient.invalidateQueries({ queryKey: ["visit", visitId] });
      visitToast.updated();
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
    if (isUpdateMode) {
      updateMutation.mutate();
    } else {
      createMutation.mutate();
    }
  };

  // Validation
  const isStep1ValidClient = selectedClient !== null && selectedAnimal !== null;
  const isStep1ValidAppointment = selectedAppointment !== null;
  const isStep1Valid = visitMode === "appointment" ? isStep1ValidAppointment : isStep1ValidClient;
  const isStep2Valid = formData.visitType > 0;
  const canProceedToStep2 = isStep1Valid;
  const canSubmit = isUpdateMode
    ? isStep2Valid
    : isStep1Valid && isStep2Valid;

  const isSubmitting = createMutation.isPending || updateMutation.isPending;

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

  // Create a mock client/animal object for display in update mode or appointment mode
  const displayClient = isUpdateMode && existingVisit
    ? {
        id: existingVisit.clientId,
        fullName: existingVisit.clientFullName,
        phone: existingVisit.clientPhone,
      } as Client
    : visitMode === "appointment" && selectedAppointment
    ? {
        id: selectedAppointment.clientId,
        fullName: selectedAppointment.clientName,
        phone: "",
      } as Client
    : selectedClient;

  const displayAnimal = isUpdateMode && existingVisit
    ? {
        animalId: existingVisit.animalId,
        name: existingVisit.animalName,
        species: existingVisit.animalSpecies,
        breed: existingVisit.animalBreed,
      } as ClientAnimal
    : visitMode === "appointment" && selectedAppointment
    ? {
        animalId: "",
        name: selectedAppointment.animalName,
        species: "",
        breed: null,
      } as ClientAnimal
    : selectedAnimal;

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
                  {isUpdateMode
                    ? t(i18nKeyContainer.visit.updateTitle)
                    : t(i18nKeyContainer.visit.addTitle)}
                </DialogTitle>
                <DialogDescription className="text-sm text-slate-500">
                  {isUpdateMode
                    ? t(i18nKeyContainer.visit.updateDescription)
                    : t(i18nKeyContainer.visit.addDescription)}
                </DialogDescription>
              </div>
            </div>

            {/* Step Indicator - only show in create mode */}
            {!isUpdateMode && (
              <StepIndicator
                steps={steps}
                currentStep={currentStep}
                className="pt-2"
              />
            )}
          </DialogHeader>
        </div>

        {/* Form Content */}
        <form onSubmit={handleSubmit}>
          <div className="px-6 py-5 max-h-[60vh] overflow-y-auto">
            {/* Loading state for update mode */}
            {isUpdateMode && isLoadingVisit && (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
              </div>
            )}

            {/* Step 1: Client & Animal Selection (Create mode only) */}
            {currentStep === STEP_CLIENT_ANIMAL && !isUpdateMode && (
              <div className="space-y-5">
                {/* Mode Selector Tabs */}
                <Tabs
                  value={visitMode}
                  onValueChange={handleModeChange}
                  dir={isRtl ? "rtl" : "ltr"}
                  className="w-full"
                >
                  <TabsList className="grid w-full grid-cols-2 h-11 p-1 bg-slate-100 rounded-lg">
                    <TabsTrigger
                      value="appointment"
                      className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm rounded-md cursor-pointer"
                    >
                      <Calendar className="h-4 w-4" />
                      {t(i18nKeyContainer.visit.modeAppointment)}
                    </TabsTrigger>
                    <TabsTrigger
                      value="client"
                      className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm rounded-md cursor-pointer"
                    >
                      <User className="h-4 w-4" />
                      {t(i18nKeyContainer.visit.modeClient)}
                    </TabsTrigger>
                  </TabsList>
                </Tabs>

                {/* Appointment Selection Step */}
                {visitMode === "appointment" && (
                  <AppointmentSelectionStep
                    onSelect={handleAppointmentSelect}
                    initialAppointment={selectedAppointment}
                    isRtl={isRtl}
                    onSwitchToClientMode={() => handleModeChange("client")}
                  />
                )}

                {/* Client & Animal Selection Step */}
                {visitMode === "client" && (
                  <ClientAnimalStep
                    onSelect={handleClientAnimalSelect}
                    initialClient={selectedClient}
                    initialAnimalId={selectedAnimal?.animalId}
                    isRtl={isRtl}
                  />
                )}
              </div>
            )}

            {/* Step 2: Visit Details */}
            {(currentStep === STEP_DETAILS || isUpdateMode) &&
              (!isLoadingVisit || !isUpdateMode) && (
                <VisitDetailsStep
                  formData={formData}
                  onFormDataChange={handleFormDataChange}
                  client={displayClient}
                  animal={displayAnimal}
                  isRtl={isRtl}
                />
              )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex gap-3 justify-between">
            {/* Left side - Back button (only in step 2 create mode) */}
            <div>
              {currentStep === STEP_DETAILS && !isUpdateMode && (
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
              {currentStep === STEP_CLIENT_ANIMAL && !isUpdateMode && (
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

              {/* Submit button (Step 2 or Update mode) */}
              {(currentStep === STEP_DETAILS || isUpdateMode) && (
                <Button
                  type="submit"
                  disabled={!canSubmit || isSubmitting || (isUpdateMode && isLoadingVisit)}
                  className="min-w-25 cursor-pointer"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="me-2 h-4 w-4 animate-spin" />
                      {isUpdateMode
                        ? t(i18nKeyContainer.visit.updating)
                        : t(i18nKeyContainer.visit.creating)}
                    </>
                  ) : isUpdateMode ? (
                    t(i18nKeyContainer.visit.update)
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
