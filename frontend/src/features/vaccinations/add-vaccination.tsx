import { useState, useEffect, useCallback } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Syringe,
  ArrowLeft,
  ArrowRight,
  Loader2,
  Calendar,
  User,
  Search,
  CheckCircle,
  XCircle,
  PawPrint,
} from "lucide-react";
import vaccinationApi, {
  type CreateVaccinationRequest,
  type UpdateVaccinationRequest,
} from "./vaccination-api";
import visitApi from "../visits/visit-api";
import { type Client } from "@/features/clients/client-api";
import { type ClientAnimal } from "@/features/animals/animal-api";
import {
  ClientAnimalStep,
  StepIndicator,
  type ClientAnimalSelection,
} from "@/features/appointments/components";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useVaccinationToast } from "./use-vaccination-toast";

type VaccinationMode = "visit" | "client";

const MODE_CREATE = "create";
const MODE_UPDATE = "update";

const STEP_SELECTION = 1;
const STEP_DETAILS = 2;

interface AddUpdateVaccinationProps {
  open: boolean;
  onClose: () => void;
  /** Existing vaccination ID for update mode */
  vaccinationId?: string | null;
}

export default function AddUpdateVaccination({
  open,
  onClose,
  vaccinationId,
}: AddUpdateVaccinationProps) {
  const mode = vaccinationId ? MODE_UPDATE : MODE_CREATE;
  const isUpdateMode = mode === MODE_UPDATE;

  // Mode state (visit vs client)
  const [vaccinationMode, setVaccinationMode] = useState<VaccinationMode>("client");

  // Step state
  const [currentStep, setCurrentStep] = useState(
    isUpdateMode ? STEP_DETAILS : STEP_SELECTION
  );

  // Selection state (Step 1 - Client mode)
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedAnimal, setSelectedAnimal] = useState<ClientAnimal | null>(null);

  // Selection state (Step 1 - Visit mode)
  const [visitSearchQuery, setVisitSearchQuery] = useState("");
  const [selectedVisitId, setSelectedVisitId] = useState<string | null>(null);
  const [visitSearchResult, setVisitSearchResult] = useState<{
    id: string;
    clientName: string;
    animalName: string;
  } | null>(null);
  const [isSearchingVisit, setIsSearchingVisit] = useState(false);
  const [visitSearchError, setVisitSearchError] = useState<string | null>(null);

  // Form data (Step 2)
  const [formData, setFormData] = useState({
    name: "",
    givenAt: new Date().toISOString().split("T")[0],
    dueTo: "",
    notes: "",
  });

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const queryClient = useQueryClient();
  const vaccinationToast = useVaccinationToast();

  // Fetch existing vaccination data for update mode
  const { data: existingVaccination, isLoading: isLoadingVaccination } = useQuery({
    queryKey: ["vaccination", vaccinationId],
    queryFn: () => vaccinationApi.getVaccinationById(vaccinationId!),
    enabled: isUpdateMode && open && !!vaccinationId,
  });

  // Define steps
  const steps = [
    {
      number: STEP_SELECTION,
      title: t(i18nKeyContainer.vaccination.step1Title),
      description: t(i18nKeyContainer.vaccination.step1Description),
    },
    {
      number: STEP_DETAILS,
      title: t(i18nKeyContainer.vaccination.step2Title),
      description: t(i18nKeyContainer.vaccination.step2Description),
    },
  ];

  const resetForm = () => {
    setSelectedClient(null);
    setSelectedAnimal(null);
    setSelectedVisitId(null);
    setVisitSearchResult(null);
    setVisitSearchQuery("");
    setVisitSearchError(null);
    setVaccinationMode("client");
    setFormData({
      name: "",
      givenAt: new Date().toISOString().split("T")[0],
      dueTo: "",
      notes: "",
    });
  };

  // Populate form when existing vaccination data is loaded
  useEffect(() => {
    if (isUpdateMode && existingVaccination && open) {
      setFormData({
        name: existingVaccination.vaccinationName,
        givenAt: existingVaccination.givenAt.split("T")[0],
        dueTo: existingVaccination.dueTo ? existingVaccination.dueTo.split("T")[0] : "",
        notes: existingVaccination.notes || "",
      });
      setCurrentStep(STEP_DETAILS);
    }
  }, [existingVaccination, isUpdateMode, open]);

  // Reset form when dialog opens/closes in create mode
  useEffect(() => {
    if (open && !isUpdateMode) {
      resetForm();
      setCurrentStep(STEP_SELECTION);
    }
  }, [open, isUpdateMode]);

  // Handle client/animal selection from Step 1
  const handleClientAnimalSelect = useCallback(
    (selection: ClientAnimalSelection) => {
      setSelectedClient(selection.client);
      setSelectedAnimal(selection.animal);
    },
    []
  );

  // Handle mode switch
  const handleModeChange = useCallback((newMode: string) => {
    setVaccinationMode(newMode as VaccinationMode);
    // Reset selections when switching modes
    setSelectedClient(null);
    setSelectedAnimal(null);
    setSelectedVisitId(null);
    setVisitSearchResult(null);
    setVisitSearchQuery("");
    setVisitSearchError(null);
  }, []);

  // Handle visit search
  const handleVisitSearch = async () => {
    if (!visitSearchQuery.trim()) return;

    setIsSearchingVisit(true);
    setVisitSearchError(null);
    setVisitSearchResult(null);

    try {
      const visit = await visitApi.getVisitById(visitSearchQuery.trim());
      setVisitSearchResult({
        id: visit.id,
        clientName: visit.clientFullName,
        animalName: visit.animalName,
      });
    } catch {
      setVisitSearchError(t(i18nKeyContainer.vaccination.visitNotFound));
    } finally {
      setIsSearchingVisit(false);
    }
  };

  // Handle Enter key in visit search
  const handleVisitSearchKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleVisitSearch();
    }
  };

  // Select the found visit
  const handleSelectVisit = () => {
    if (visitSearchResult) {
      setSelectedVisitId(visitSearchResult.id);
    }
  };

  // Create mutation
  const createMutation = useMutation({
    mutationFn: () => {
      const request: CreateVaccinationRequest =
        vaccinationMode === "visit"
          ? {
              visitId: selectedVisitId!,
              name: formData.name,
              givenAt: new Date(formData.givenAt).toISOString(),
              dueTo: formData.dueTo === "" ? null : new Date(formData.dueTo).toISOString(),
              notes: formData.notes.trim() || null,
            }
          : {
              animalId: selectedAnimal!.animalId,
              name: formData.name,
              givenAt: new Date(formData.givenAt).toISOString(),
              dueTo: formData.dueTo === "" ? null : new Date(formData.dueTo).toISOString(),
              notes: formData.notes.trim() || null,
            };
      return vaccinationApi.createVaccination(request);
    },
    onSuccess: async() => {
      await queryClient.invalidateQueries({ queryKey: ["vaccinations"] });
      vaccinationToast.created();
      resetForm();
      onClose();
    },
    onError: (error) => {
      vaccinationToast.error(error);
    },
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: () => {
      const request: UpdateVaccinationRequest = {
        name: formData.name,
        givenAt: new Date(formData.givenAt).toISOString(),
        dueTo: formData.dueTo === "" ? null : new Date(formData.dueTo).toISOString(),
        notes: formData.notes.trim() || null,
      };
      return vaccinationApi.updateVaccination(vaccinationId!, request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vaccinations"] });
      queryClient.invalidateQueries({ queryKey: ["vaccination", vaccinationId] });
      vaccinationToast.updated();
      resetForm();
      onClose();
    },
    onError: (error) => {
      vaccinationToast.error(error);
    },
  });

  // Navigation
  const goToNextStep = () => {
    if (currentStep < STEP_DETAILS) {
      setCurrentStep(currentStep + 1);
    }
  };

  const goToPrevStep = () => {
    if (currentStep > STEP_SELECTION) {
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
  const isStep1ValidVisit = selectedVisitId !== null;
  const isStep1Valid = vaccinationMode === "visit" ? isStep1ValidVisit : isStep1ValidClient;
  const isStep2Valid = formData.name.trim() !== "" && formData.givenAt !== "";
  const canProceedToStep2 = isStep1Valid;
  const canSubmit = isUpdateMode ? isStep2Valid : isStep1Valid && isStep2Valid;

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

  // Display info for update mode
  const displayClient =
    isUpdateMode && existingVaccination
      ? {
          id: existingVaccination.clientId,
          fullName: existingVaccination.clientName,
          phone: "",
        }
      : selectedClient;

  const displayAnimal =
    isUpdateMode && existingVaccination
      ? {
          animalId: existingVaccination.animalId,
          name: existingVaccination.animalName,
          species: "",
          breed: null,
        }
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
                <Syringe className="h-5 w-5 text-primary" />
              </div>
              <div>
                <DialogTitle className="text-lg font-semibold text-slate-900">
                  {isUpdateMode
                    ? t(i18nKeyContainer.vaccination.updateTitle)
                    : t(i18nKeyContainer.vaccination.addTitle)}
                </DialogTitle>
                <DialogDescription className="text-sm text-slate-500">
                  {isUpdateMode
                    ? t(i18nKeyContainer.vaccination.updateDescription)
                    : t(i18nKeyContainer.vaccination.addDescription)}
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
            {isUpdateMode && isLoadingVaccination && (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
              </div>
            )}

            {/* Step 1: Selection (Create mode only) */}
            {currentStep === STEP_SELECTION && !isUpdateMode && (
              <div className="space-y-5">
                {/* Mode Selector Tabs */}
                <Tabs
                  value={vaccinationMode}
                  onValueChange={handleModeChange}
                  dir={isRtl ? "rtl" : "ltr"}
                  className="w-full"
                >
                  <TabsList className="grid w-full grid-cols-2 h-11 p-1 bg-slate-100 rounded-lg">
                    <TabsTrigger
                      value="visit"
                      className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm rounded-md cursor-pointer"
                    >
                      <Calendar className="h-4 w-4" />
                      {t(i18nKeyContainer.vaccination.modeVisit)}
                    </TabsTrigger>
                    <TabsTrigger
                      value="client"
                      className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:shadow-sm rounded-md cursor-pointer"
                    >
                      <User className="h-4 w-4" />
                      {t(i18nKeyContainer.vaccination.modeClient)}
                    </TabsTrigger>
                  </TabsList>
                </Tabs>

                {/* Visit Selection Step */}
                {vaccinationMode === "visit" && (
                  <div className="space-y-4">
                    <div className="space-y-3">
                      <Label className="text-sm font-medium text-slate-700">
                        <div className="flex items-center gap-2">
                          <Search className="h-4 w-4 text-slate-400" />
                          {t(i18nKeyContainer.vaccination.searchVisit)}
                        </div>
                      </Label>
                      <div className="flex gap-2">
                        <Input
                          type="text"
                          value={visitSearchQuery}
                          onChange={(e) => setVisitSearchQuery(e.target.value)}
                          onKeyDown={handleVisitSearchKeyPress}
                          placeholder={t(i18nKeyContainer.vaccination.searchVisitPlaceholder)}
                          className="h-11 flex-1 border-slate-200 focus:border-primary focus:ring-primary"
                          disabled={isSearchingVisit}
                        />
                        <Button
                          type="button"
                          onClick={handleVisitSearch}
                          disabled={!visitSearchQuery.trim() || isSearchingVisit}
                          className="h-11 min-w-25 cursor-pointer"
                        >
                          {isSearchingVisit ? (
                            <Loader2 className="h-4 w-4 animate-spin" />
                          ) : (
                            <Search className="h-4 w-4" />
                          )}
                        </Button>
                      </div>
                    </div>

                    {/* Visit Search Result */}
                    {visitSearchResult && (
                      <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-lg">
                        <div className="flex items-start gap-3">
                          <CheckCircle className="h-5 w-5 text-emerald-600 mt-0.5" />
                          <div className="flex-1">
                            <p className="font-medium text-emerald-800">
                              {t(i18nKeyContainer.vaccination.visitFound)}
                            </p>
                            <div className="mt-2 space-y-1 text-sm text-emerald-700">
                              <div className="flex items-center gap-2">
                                <User className="h-4 w-4" />
                                <span>{visitSearchResult.clientName}</span>
                              </div>
                              <div className="flex items-center gap-2">
                                <PawPrint className="h-4 w-4" />
                                <span>{visitSearchResult.animalName}</span>
                              </div>
                            </div>
                            <Button
                              type="button"
                              size="sm"
                              className="mt-3 cursor-pointer"
                              onClick={handleSelectVisit}
                              disabled={selectedVisitId === visitSearchResult.id}
                            >
                              {selectedVisitId === visitSearchResult.id
                                ? "✓ Selected"
                                : t(i18nKeyContainer.vaccination.useThisVisit)}
                            </Button>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Visit Search Error */}
                    {visitSearchError && (
                      <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
                        <div className="flex items-center gap-2 text-red-700">
                          <XCircle className="h-5 w-5" />
                          <span>{visitSearchError}</span>
                        </div>
                      </div>
                    )}
                  </div>
                )}

                {/* Client & Animal Selection Step */}
                {vaccinationMode === "client" && (
                  <ClientAnimalStep
                    onSelect={handleClientAnimalSelect}
                    initialClient={selectedClient}
                    initialAnimalId={selectedAnimal?.animalId}
                    isRtl={isRtl}
                  />
                )}
              </div>
            )}

            {/* Step 2: Vaccination Details */}
            {(currentStep === STEP_DETAILS || isUpdateMode) &&
              (!isLoadingVaccination || !isUpdateMode) && (
                <div className="space-y-5">
                  {/* Client/Animal Info (read-only summary) */}
                  {!isUpdateMode && (
                    <div className="p-4 bg-slate-50 rounded-lg space-y-2">
                      <div className="flex items-center gap-2 text-sm">
                        <User className="h-4 w-4 text-slate-400" />
                        <span className="font-medium">
                          {vaccinationMode === "visit"
                            ? visitSearchResult?.clientName
                            : displayClient?.fullName}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-sm">
                        <PawPrint className="h-4 w-4 text-slate-400" />
                        <span className="font-medium">
                          {vaccinationMode === "visit"
                            ? visitSearchResult?.animalName
                            : displayAnimal?.name}
                        </span>
                      </div>
                    </div>
                  )}

                  {/* Update mode info */}
                  {isUpdateMode && existingVaccination && (
                    <div className="p-4 bg-slate-50 rounded-lg space-y-2">
                      <div className="flex items-center gap-2 text-sm">
                        <User className="h-4 w-4 text-slate-400" />
                        <span className="font-medium">{existingVaccination.clientName}</span>
                      </div>
                      <div className="flex items-center gap-2 text-sm">
                        <PawPrint className="h-4 w-4 text-slate-400" />
                        <span className="font-medium">{existingVaccination.animalName}</span>
                      </div>
                    </div>
                  )}

                  {/* Vaccine Name */}
                  <div className="space-y-2">
                    <Label htmlFor="vaccineName" className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.vaccination.vaccineName)} *
                    </Label>
                    <Input
                      id="vaccineName"
                      type="text"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder={t(i18nKeyContainer.vaccination.vaccineNamePlaceholder)}
                      className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
                      required
                    />
                  </div>

                  {/* Date Fields */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="givenAt" className="text-sm font-medium text-slate-700">
                        {t(i18nKeyContainer.vaccination.givenAt)} *
                      </Label>
                      <Input
                        id="givenAt"
                        type="date"
                        value={formData.givenAt}
                        onChange={(e) => setFormData({ ...formData, givenAt: e.target.value })}
                        className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
                        required
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="dueTo" className="text-sm font-medium text-slate-700">
                        {t(i18nKeyContainer.vaccination.nextDueDate)} *
                      </Label>
                      <Input
                        id="dueTo"
                        type="date"
                        value={formData.dueTo} 
                        onChange={(e) => setFormData({ ...formData, dueTo: e.target.value })}
                        className="h-11 border-slate-200 focus:border-primary focus:ring-primary"
                      />
                    </div>
                  </div>

                  {/* Notes */}
                  <div className="space-y-2">
                    <Label htmlFor="notes" className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.vaccination.notes)}
                    </Label>
                    <Textarea
                      id="notes"
                      value={formData.notes}
                      onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                      placeholder={t(i18nKeyContainer.vaccination.notesPlaceholder)}
                      rows={3}
                      className="resize-none"
                    />
                  </div>
                </div>
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
                  {t(i18nKeyContainer.vaccination.back)}
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
              {currentStep === STEP_SELECTION && !isUpdateMode && (
                <Button
                  type="button"
                  onClick={goToNextStep}
                  disabled={!canProceedToStep2}
                  className="gap-2 min-w-25 cursor-pointer"
                >
                  {t(i18nKeyContainer.vaccination.next)}
                  <NextIcon className="h-4 w-4" />
                </Button>
              )}

              {/* Submit button (Step 2 or Update mode) */}
              {(currentStep === STEP_DETAILS || isUpdateMode) && (
                <Button
                  type="submit"
                  disabled={!canSubmit || isSubmitting || (isUpdateMode && isLoadingVaccination)}
                  className="min-w-25 cursor-pointer"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="me-2 h-4 w-4 animate-spin" />
                      {isUpdateMode
                        ? t(i18nKeyContainer.vaccination.updating)
                        : t(i18nKeyContainer.vaccination.creating)}
                    </>
                  ) : isUpdateMode ? (
                    t(i18nKeyContainer.vaccination.update)
                  ) : (
                    t(i18nKeyContainer.vaccination.create)
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
