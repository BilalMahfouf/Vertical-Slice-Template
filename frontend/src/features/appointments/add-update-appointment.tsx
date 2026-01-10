import { useState, useEffect, useCallback } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { CalendarClock, ArrowLeft, ArrowRight, Loader2 } from "lucide-react";
import appointmentApi, { type Appointment } from "./appointment-api";
import { type Client } from "@/features/clients/client-api";
import { type ClientAnimal } from "@/features/animals/animal-api";
import {
  ClientAnimalStep,
  AppointmentDetailsStep,
  StepIndicator,
  type ClientAnimalSelection,
  type AppointmentFormData,
} from "./components";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useAppointmentToast } from "./use-appointment-toast";

const MODE_CREATE = "create";
const MODE_RESCHEDULE = "reschedule";

const STEP_CLIENT_ANIMAL = 1;
const STEP_DETAILS = 2;

interface AddUpdateAppointmentProps {
  open: boolean;
  onClose: () => void;
  /** Existing appointment for reschedule mode */
  appointment?: Appointment | null;
}

export default function AddUpdateAppointment({
  open,
  onClose,
  appointment,
}: AddUpdateAppointmentProps) {
  const mode = appointment ? MODE_RESCHEDULE : MODE_CREATE;
  const isRescheduleMode = mode === MODE_RESCHEDULE;

  // Step state
  const [currentStep, setCurrentStep] = useState(
    isRescheduleMode ? STEP_DETAILS : STEP_CLIENT_ANIMAL
  );

  // Selection state (Step 1)
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedAnimal, setSelectedAnimal] = useState<ClientAnimal | null>(null);

  // Form data (Step 2)
  const [formData, setFormData] = useState<AppointmentFormData>({
    date: "",
    time: "",
    location: "",
    notes: "",
  });

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const queryClient = useQueryClient();
  const appointmentToast = useAppointmentToast();

  // Define steps
  const steps = [
    {
      number: STEP_CLIENT_ANIMAL,
      title: t(i18nKeyContainer.appointment.step1Title),
      description: t(i18nKeyContainer.appointment.step1Description),
    },
    {
      number: STEP_DETAILS,
      title: t(i18nKeyContainer.appointment.step2Title),
      description: t(i18nKeyContainer.appointment.step2Description),
    },
  ];

  // Reset form when dialog opens/closes or mode changes
  useEffect(() => {
    if (open) {
      if (isRescheduleMode && appointment) {
        // Pre-fill for reschedule mode
        const appointmentDate = new Date(appointment.appointmentDate);
        setFormData({
          date: appointmentDate.toISOString().split("T")[0],
          time: appointmentDate.toLocaleTimeString("en-GB", {
            hour: "2-digit",
            minute: "2-digit",
          }),
          location: "",
          notes: "",
        });
        setCurrentStep(STEP_DETAILS);
      } else {
        // Reset for create mode
        resetForm();
        setCurrentStep(STEP_CLIENT_ANIMAL);
      }
    }
  }, [open, isRescheduleMode, appointment]);

  const resetForm = () => {
    setSelectedClient(null);
    setSelectedAnimal(null);
    setFormData({
      date: "",
      time: "",
      location: "",
      notes: "",
    });
  };

  // Handle selection from Step 1
  const handleClientAnimalSelect = useCallback(
    (selection: ClientAnimalSelection) => {
      setSelectedClient(selection.client);
      setSelectedAnimal(selection.animal);
    },
    []
  );

  // Update form data
  const handleFormDataChange = useCallback((data: Partial<AppointmentFormData>) => {
    setFormData((prev) => ({ ...prev, ...data }));
  }, []);

  // Create mutation
  const createMutation = useMutation({
    mutationFn: () =>
      appointmentApi.createAppointment({
        animalId: selectedAnimal!.animalId,
        appointmentDate: `${formData.date}T${formData.time}:00Z`,
        location: formData.location,
        notes: formData.notes || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      appointmentToast.created();
      resetForm();
      onClose();
    },
    onError: (error) => {
      appointmentToast.error(error);
    },
  });

  // Reschedule mutation
  const rescheduleMutation = useMutation({
    mutationFn: () =>
      appointmentApi.rescheduleAppointment(appointment!.id, {
        newAppointmentDate: `${formData.date}T${formData.time}:00`,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["appointments"] });
      appointmentToast.rescheduled();
      resetForm();
      onClose();
    },
    onError: (error) => {
      appointmentToast.error(error);
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
    if (isRescheduleMode) {
      rescheduleMutation.mutate();
    } else {
      createMutation.mutate();
    }
  };

  // Validation
  const isStep1Valid = selectedClient !== null && selectedAnimal !== null;
  const isStep2Valid =
    formData.date && formData.time && (isRescheduleMode || formData.location);
  const canProceedToStep2 = isStep1Valid;
  const canSubmit = isStep2Valid && (isRescheduleMode || isStep1Valid);

  const isSubmitting = createMutation.isPending || rescheduleMutation.isPending;

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
                <CalendarClock className="h-5 w-5 text-primary" />
              </div>
              <div>
                <DialogTitle className="text-lg font-semibold text-slate-900">
                  {isRescheduleMode
                    ? t(i18nKeyContainer.appointment.rescheduleTitle)
                    : t(i18nKeyContainer.appointment.addTitle)}
                </DialogTitle>
                <DialogDescription className="text-sm text-slate-500">
                  {isRescheduleMode
                    ? t(i18nKeyContainer.appointment.rescheduleDescription)
                    : t(i18nKeyContainer.appointment.addDescription)}
                </DialogDescription>
              </div>
            </div>

            {/* Step Indicator - only show in create mode */}
            {!isRescheduleMode && (
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
            {/* Step 1: Client & Animal Selection */}
            {currentStep === STEP_CLIENT_ANIMAL && !isRescheduleMode && (
              <ClientAnimalStep
                onSelect={handleClientAnimalSelect}
                initialClient={selectedClient}
                initialAnimalId={selectedAnimal?.animalId}
                isRtl={isRtl}
              />
            )}

            {/* Step 2: Appointment Details */}
            {(currentStep === STEP_DETAILS || isRescheduleMode) && (
              <AppointmentDetailsStep
                formData={formData}
                onFormDataChange={handleFormDataChange}
                client={selectedClient}
                animal={selectedAnimal}
                existingAppointment={appointment}
                isRescheduleMode={isRescheduleMode}
              />
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex gap-3 justify-between">
            {/* Left side - Back button (only in step 2 create mode) */}
            <div>
              {currentStep === STEP_DETAILS && !isRescheduleMode && (
                <Button
                  type="button"
                  variant="outline"
                  onClick={goToPrevStep}
                  disabled={isSubmitting}
                  className="gap-2 cursor-pointer border-0 hover:bg-slate-100"
                >
                  <BackIcon className="h-4 w-4" />
                  {t(i18nKeyContainer.appointment.back)}
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
              {currentStep === STEP_CLIENT_ANIMAL && !isRescheduleMode && (
                <Button
                  type="button"
                  onClick={goToNextStep}
                  disabled={!canProceedToStep2}
                  className="gap-2 min-w-25 cursor-pointer"
                >
                  {t(i18nKeyContainer.appointment.next)}
                  <NextIcon className="h-4 w-4" />
                </Button>
              )}

              {/* Submit button (Step 2 or Reschedule) */}
              {(currentStep === STEP_DETAILS || isRescheduleMode) && (
                <Button
                  type="submit"
                  disabled={!canSubmit || isSubmitting}
                  className="min-w-25 cursor-pointer"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="me-2 h-4 w-4 animate-spin" />
                      {isRescheduleMode
                        ? t(i18nKeyContainer.appointment.rescheduling)
                        : t(i18nKeyContainer.appointment.creating)}
                    </>
                  ) : isRescheduleMode ? (
                    t(i18nKeyContainer.appointment.reschedule)
                  ) : (
                    t(i18nKeyContainer.appointment.create)
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
