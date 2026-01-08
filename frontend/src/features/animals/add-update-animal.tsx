import { useState, useEffect } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  PawPrint,
  Search,
  User,
  Phone,
  Hash,
  ArrowLeft,
  ArrowRight,
  CheckCircle,
  XCircle,
  Calendar,
  Palette,
  Cpu,
  Activity,
} from "lucide-react";
import animalApi, {
  type CreateAnimalRequest,
  type UpdateAnimalRequest,
  AnimalStatus,
  Gender,
} from "./animal-api";
import clientApi, { type Client } from "../clients/client-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useAnimalToast } from "./use-animal-toast";

const MODE_ADDNEW = "addnew";
const MODE_UPDATE = "update";

const STEP_CLIENT_SEARCH = 1;
const STEP_ANIMAL_FORM = 2;

interface AddUpdateAnimalProps {
  open: boolean;
  onClose: () => void;
  animalId?: string;
}

export default function AddUpdateAnimal({
  open,
  onClose,
  animalId,
}: AddUpdateAnimalProps) {
  // Mode and step state
  const [mode, setMode] = useState<typeof MODE_ADDNEW | typeof MODE_UPDATE>(
    MODE_ADDNEW
  );
  const [currentStep, setCurrentStep] = useState(STEP_CLIENT_SEARCH);

  // Client search state
  const [searchQuery, setSearchQuery] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [searchError, setSearchError] = useState<string | null>(null);

  // Animal form state
  const [name, setName] = useState("");
  const [species, setSpecies] = useState("");
  const [breed, setBreed] = useState("");
  const [gender, setGender] = useState<string>("");
  const [birthDate, setBirthDate] = useState("");
  const [color, setColor] = useState("");
  const [microchipNumber, setMicrochipNumber] = useState("");
  const [status, setStatus] = useState<string>("1");

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const queryClient = useQueryClient();
  const animalToast = useAnimalToast();

  // Set mode based on animalId
  useEffect(() => {
    if (animalId) {
      setMode(MODE_UPDATE);
      setCurrentStep(STEP_ANIMAL_FORM);
    } else {
      setMode(MODE_ADDNEW);
      setCurrentStep(STEP_CLIENT_SEARCH);
    }
  }, [animalId]);

  // Fetch animal data in update mode
  const { data: animalData, isLoading: isLoadingAnimal } = useQuery({
    queryKey: ["animal", animalId],
    queryFn: () => animalApi.getAnimalById(animalId!),
    enabled: mode === MODE_UPDATE && open && !!animalId,
  });

  // Fetch client data when animal is loaded (for update mode)
  const { data: clientFromAnimal } = useQuery({
    queryKey: ["client", animalData?.clientId],
    queryFn: () => clientApi.getClientById(animalData!.clientId),
    enabled: mode === MODE_UPDATE && !!animalData?.clientId,
  });

  // Fill form when animal data is loaded
  useEffect(() => {
    if (animalData && mode === MODE_UPDATE) {
      setName(animalData.name);
      setSpecies(animalData.species);
      setBreed(animalData.breed || "");
      setGender(
        Gender.find((g) => g.label=== animalData.gender)?.value.toString() ||
          ""
      );
      setBirthDate(
        animalData.birthDate ? animalData.birthDate.split("T")[0] : ""
      );
      setColor(animalData.color || "");
      setMicrochipNumber(animalData.microchipNumber || "");
      setStatus(
        AnimalStatus.find((s) => s.label.toLowerCase() === animalData.status.toLowerCase())?.value.toString() ||
          "1"
      );
    }
  }, [animalData, mode]);

  // Set client from animal data in update mode
  useEffect(() => {
    if (clientFromAnimal && mode === MODE_UPDATE) {
      setSelectedClient(clientFromAnimal);
    }
  }, [clientFromAnimal, mode]);

  // Search for client
  const handleSearch = async () => {
    if (!searchQuery.trim()) return;

    setIsSearching(true);
    setSearchError(null);

    try {
      // Try to search by ID first (if it looks like a GUID)
      const isGuid =
        /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
          searchQuery.trim()
        );

      if (isGuid) {
        const client = await clientApi.getClientById(searchQuery.trim());
        setSelectedClient(client);
      } else {
        // Search by name using dedicated search endpoint
        const result = await clientApi.getClientBySearch(searchQuery.trim());

        if (result.item.length > 0) {
          // Take the first matching client
          setSelectedClient(result.item[0]);
        } else {
          setSearchError(t(i18nKeyContainer.animal.clientNotFound));
          setSelectedClient(null);
        }
      }
    } catch {
      setSearchError(t(i18nKeyContainer.animal.clientNotFound));
      setSelectedClient(null);
    } finally {
      setIsSearching(false);
    }
  };

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (data: CreateAnimalRequest) => animalApi.createAnimal(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["animals"] });
      animalToast.added();
      resetForm();
      onClose();
    },
    onError: (error) => {
      animalToast.error(error);
    },
  });

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: (data: { id: string; request: UpdateAnimalRequest }) =>
      animalApi.updateAnimal(data.id, data.request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["animals"] });
      queryClient.invalidateQueries({ queryKey: ["animal", animalId] });
      animalToast.updated();
      resetForm();
      onClose();
    },
    onError: (error) => {
      animalToast.error(error);
    },
  });

  const resetForm = () => {
    setSearchQuery("");
    setSelectedClient(null);
    setSearchError(null);
    setName("");
    setSpecies("");
    setBreed("");
    setGender("");
    setBirthDate("");
    setColor("");
    setMicrochipNumber("");
    setStatus("1");
    setCurrentStep(STEP_CLIENT_SEARCH);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedClient || !name || !species || !gender) return;

    if (mode === MODE_ADDNEW) {
      const request: CreateAnimalRequest = {
        clientId: selectedClient.id,
        name,
        species,
        breed: breed || null,
        gender: parseInt(gender),
        birthDate: birthDate || null,
        color: color || null,
        microchipNumber: microchipNumber || null,
        status: parseInt(status),
      };
      createMutation.mutate(request);
    } else if (mode === MODE_UPDATE && animalId) {
      const request: UpdateAnimalRequest = {
        name,
        species,
        breed: breed || null,
        gender: parseInt(gender),
        birthDate: birthDate || null,
        color: color || null,
        microchipNumber: microchipNumber || null,
        status: parseInt(status),
      };
      updateMutation.mutate({ id: animalId, request });
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  const ArrowIcon = isRtl ? ArrowLeft : ArrowRight;
  const BackArrowIcon = isRtl ? ArrowRight : ArrowLeft;

  // Render Step 1: Client Search
  const renderClientSearch = () => (
    <div className="space-y-6">
      {/* Search Input */}
      <div className="space-y-2">
        <Label htmlFor="searchClient">
          {t(i18nKeyContainer.animal.searchClient)}
        </Label>
        <div className="flex gap-2">
          <div className="relative flex-1">
            <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="searchClient"
              type="text"
              placeholder={t(i18nKeyContainer.animal.searchPlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  handleSearch();
                }
              }}
            />
          </div>
          <Button
            type="button"
            onClick={handleSearch}
            disabled={isSearching || !searchQuery.trim()}
            className="h-11 px-6 cursor-pointer"
          >
            {isSearching
              ? t(i18nKeyContainer.animal.searching)
              : t(i18nKeyContainer.animal.search)}
          </Button>
        </div>
      </div>

      {/* Search Result */}
      {searchError && (
        <div className="flex items-center gap-2 p-4 rounded-lg bg-red-50 border border-red-200 text-red-700">
          <XCircle className="h-5 w-5 shrink-0" />
          <span>{searchError}</span>
        </div>
      )}

      {selectedClient && (
        <div className="space-y-3">
          <div className="flex items-center gap-2 text-green-700">
            <CheckCircle className="h-5 w-5" />
            <span className="font-medium">
              {t(i18nKeyContainer.animal.clientFound)}
            </span>
          </div>
          <div className="p-4 rounded-lg bg-slate-50 border border-slate-200 space-y-3">
            <h4 className="font-semibold text-slate-700">
              {t(i18nKeyContainer.animal.clientInfo)}
            </h4>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div className="flex items-center gap-2">
                <Hash className="h-4 w-4 text-slate-400" />
                <span className="text-sm text-slate-600">
                  {selectedClient.id}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <User className="h-4 w-4 text-slate-400" />
                <span className="text-sm text-slate-600">
                  {selectedClient.fullName}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Phone className="h-4 w-4 text-slate-400" />
                <span className="text-sm text-slate-600">
                  {selectedClient.phone}
                </span>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Navigation Buttons */}
      <div className="flex gap-3 pt-4">
        <Button
          type="button"
          variant="outline"
          className="flex-1 h-11 cursor-pointer border-0 bg-slate-100 hover:bg-slate-200"
          onClick={handleClose}
        >
          {t(i18nKeyContainer.animal.cancel)}
        </Button>
        <Button
          type="button"
          className="flex-1 h-11 font-bold shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all cursor-pointer"
          disabled={!selectedClient}
          onClick={() => setCurrentStep(STEP_ANIMAL_FORM)}
        >
          {t(i18nKeyContainer.animal.next)}
          <ArrowIcon className="ms-2 h-4 w-4" />
        </Button>
      </div>
    </div>
  );

  // Render Step 2: Animal Form
  const renderAnimalForm = () => (
    <form onSubmit={handleSubmit} className="space-y-5">
      {/* Client Info (Read-only) */}
      {selectedClient && (
        <div className="p-4 rounded-lg bg-slate-50 border border-slate-200 space-y-2">
          <h4 className="font-semibold text-slate-700 text-sm">
            {t(i18nKeyContainer.animal.clientInfo)}
          </h4>
          <div className="flex flex-wrap gap-4 text-sm">
            <div className="flex items-center gap-2">
              <Hash className="h-4 w-4 text-slate-400" />
              <span className="text-slate-600">{selectedClient.id}</span>
            </div>
            <div className="flex items-center gap-2">
              <User className="h-4 w-4 text-slate-400" />
              <span className="text-slate-600">{selectedClient.fullName}</span>
            </div>
            <div className="flex items-center gap-2">
              <Phone className="h-4 w-4 text-slate-400" />
              <span className="text-slate-600">{selectedClient.phone}</span>
            </div>
          </div>
        </div>
      )}

      {/* Animal Fields */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {/* Name */}
        <div className="space-y-2">
          <Label htmlFor="name">{t(i18nKeyContainer.animal.name)}</Label>
          <div className="relative">
            <PawPrint className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="name"
              type="text"
              placeholder={t(i18nKeyContainer.animal.namePlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>
        </div>

        {/* Species */}
        <div className="space-y-2">
          <Label htmlFor="species">{t(i18nKeyContainer.animal.species)}</Label>
          <div className="relative">
            <PawPrint className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="species"
              type="text"
              placeholder={t(i18nKeyContainer.animal.speciesPlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={species}
              onChange={(e) => setSpecies(e.target.value)}
              required
            />
          </div>
        </div>

        {/* Breed */}
        <div className="space-y-2">
          <Label htmlFor="breed">{t(i18nKeyContainer.animal.breed)}</Label>
          <div className="relative">
            <PawPrint className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="breed"
              type="text"
              placeholder={t(i18nKeyContainer.animal.breedPlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={breed}
              onChange={(e) => setBreed(e.target.value)}
            />
          </div>
        </div>

        {/* Gender */}
        <div className="space-y-2">
          <Label htmlFor="gender">{t(i18nKeyContainer.animal.gender)}</Label>
          <Select value={gender} onValueChange={setGender} required>
            <SelectTrigger className="h-11 bg-slate-50 border-slate-200">
              <SelectValue
                placeholder={t(i18nKeyContainer.animal.selectGender)}
              />
            </SelectTrigger>
            <SelectContent className="bg-white border-0">
              {Gender.map((g) => (
                <SelectItem key={g.value} value={g.value.toString()} className="cursor-pointer hover:!bg-primary/10 transition-colors">
                  {t(
                    i18nKeyContainer.animal[
                      g.label.toLowerCase() as "male" | "female"
                    ]
                  )}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Birth Date */}
        <div className="space-y-2">
          <Label htmlFor="birthDate">
            {t(i18nKeyContainer.animal.birthDate)}
          </Label>
          <div className="relative">
            <Calendar className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="birthDate"
              type="date"
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={birthDate}
              onChange={(e) => setBirthDate(e.target.value)}
            />
          </div>
        </div>

        {/* Color */}
        <div className="space-y-2">
          <Label htmlFor="color">{t(i18nKeyContainer.animal.color)}</Label>
          <div className="relative">
            <Palette className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="color"
              type="text"
              placeholder={t(i18nKeyContainer.animal.colorPlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={color}
              onChange={(e) => setColor(e.target.value)}
            />
          </div>
        </div>

        {/* Microchip Number */}
        <div className="space-y-2">
          <Label htmlFor="microchipNumber">
            {t(i18nKeyContainer.animal.microchipNumber)}
          </Label>
          <div className="relative">
            <Cpu className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="microchipNumber"
              type="text"
              placeholder={t(i18nKeyContainer.animal.microchipPlaceholder)}
              className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
              value={microchipNumber}
              onChange={(e) => setMicrochipNumber(e.target.value)}
            />
          </div>
        </div>

        {/* Status */}
        <div className="space-y-2">
          <Label htmlFor="status">{t(i18nKeyContainer.animal.status)}</Label>
          <div className="relative">
            <Activity className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 z-10 pointer-events-none" />
            <Select value={status} onValueChange={setStatus}>
              <SelectTrigger className="h-11 bg-slate-50 border-slate-200 ps-10">
                <SelectValue
                  placeholder={t(i18nKeyContainer.animal.selectStatus)}
                />
              </SelectTrigger>
              <SelectContent className="bg-white border-0">
                {AnimalStatus.map((s) => (
                  <SelectItem key={s.value} value={s.value.toString()} className="cursor-pointer hover:!bg-primary/10 transition-colors">
                    {t(
                      i18nKeyContainer.animal[
                        `status${s.label}` as keyof typeof i18nKeyContainer.animal
                      ]
                    )}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      {/* Navigation Buttons */}
      <div className="flex gap-3 pt-4">
        {mode === MODE_ADDNEW && (
          <Button
            type="button"
            variant="outline"
            className="flex-1 h-11 cursor-pointer border-0 bg-slate-100 hover:bg-slate-200"
            onClick={() => setCurrentStep(STEP_CLIENT_SEARCH)}
            disabled={isPending}
          >
            <BackArrowIcon className="me-2 h-4 w-4" />
            {t(i18nKeyContainer.animal.back)}
          </Button>
        )}
        {mode === MODE_UPDATE && (
          <Button
            type="button"
            variant="outline"
            className="flex-1 h-11 cursor-pointer border-0 bg-slate-100 hover:bg-slate-200"
            onClick={handleClose}
            disabled={isPending}
          >
            {t(i18nKeyContainer.animal.cancel)}
          </Button>
        )}
        <Button
          type="submit"
          className="flex-1 h-11 font-bold shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all cursor-pointer"
          disabled={isPending || !name || !species || !gender}
        >
          {isPending
            ? t(
                mode === MODE_UPDATE
                  ? i18nKeyContainer.animal.updating
                  : i18nKeyContainer.animal.adding
              )
            : t(
                mode === MODE_UPDATE
                  ? i18nKeyContainer.animal.updateAnimal
                  : i18nKeyContainer.animal.addAnimal
              )}
        </Button>
      </div>
    </form>
  );

  // Loading state for update mode
  if (mode === MODE_UPDATE && isLoadingAnimal) {
    return (
      <Dialog open={open} onOpenChange={handleClose}>
        <DialogContent
          className="max-w-2xl p-0 overflow-hidden bg-white"
          dir={isRtl ? "rtl" : "ltr"}
          onInteractOutside={(e) => e.preventDefault()}
        >
          <div className="flex items-center justify-center py-20">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent
        className="max-w-2xl p-0 overflow-hidden bg-white max-h-[90vh]"
        dir={isRtl ? "rtl" : "ltr"}
        onInteractOutside={(e) => e.preventDefault()}
      >
        <div className="w-full overflow-y-auto max-h-[90vh]">
          {/* Header */}
          <div className="space-y-1 pb-6 pt-8 text-center border-b border-slate-100 px-8 sticky top-0 bg-white z-10">
            <div className="flex justify-center mb-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
                <PawPrint className="h-6 w-6 text-primary" />
              </div>
            </div>
            <h2 className="text-2xl font-bold">
              {mode === MODE_UPDATE
                ? t(i18nKeyContainer.animal.updateTitle)
                : currentStep === STEP_CLIENT_SEARCH
                ? t(i18nKeyContainer.animal.step1Title)
                : t(i18nKeyContainer.animal.addTitle)}
            </h2>
            <p className="text-slate-500">
              {mode === MODE_UPDATE
                ? t(i18nKeyContainer.animal.updateDescription)
                : currentStep === STEP_CLIENT_SEARCH
                ? t(i18nKeyContainer.animal.step1Description)
                : t(i18nKeyContainer.animal.addDescription)}
            </p>

            {/* Step Indicator (only in AddNew mode) */}
            {mode === MODE_ADDNEW && (
              <div className="flex justify-center items-center gap-3 pt-4">
                <div
                  className={`flex items-center justify-center w-8 h-8 rounded-full text-sm font-semibold transition-colors ${
                    currentStep === STEP_CLIENT_SEARCH
                      ? "bg-primary text-white"
                      : "bg-primary/20 text-primary"
                  }`}
                >
                  1
                </div>
                <div className="w-12 h-1 bg-slate-200 rounded-full overflow-hidden">
                  <div
                    className={`h-full bg-primary transition-all duration-300 ${
                      currentStep === STEP_ANIMAL_FORM ? "w-full" : "w-0"
                    }`}
                  />
                </div>
                <div
                  className={`flex items-center justify-center w-8 h-8 rounded-full text-sm font-semibold transition-colors ${
                    currentStep === STEP_ANIMAL_FORM
                      ? "bg-primary text-white"
                      : "bg-slate-200 text-slate-400"
                  }`}
                >
                  2
                </div>
              </div>
            )}
          </div>

          {/* Content */}
          <div className="pt-8 pb-8 px-8">
            {currentStep === STEP_CLIENT_SEARCH && mode === MODE_ADDNEW
              ? renderClientSearch()
              : renderAnimalForm()}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
