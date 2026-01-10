import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Search,
  User,
  Phone,
  PawPrint,
  CheckCircle,
  XCircle,
  Loader2,
} from "lucide-react";
import clientApi, { type Client } from "@/features/clients/client-api";
import { type Animal } from "@/features/animals/animal-api";
import { useClientAnimals } from "../hooks/use-client-animals";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export interface ClientAnimalSelection {
  client: Client;
  animal: Animal;
}

interface ClientAnimalStepProps {
  /** Callback when both client and animal are selected */
  onSelect: (selection: ClientAnimalSelection) => void;
  /** Initial client (for edit mode) */
  initialClient?: Client | null;
  /** Initial animal ID (for edit mode) */
  initialAnimalId?: string | null;
  /** Is RTL layout */
  isRtl?: boolean;
}

export default function ClientAnimalStep({
  onSelect,
  initialClient = null,
  initialAnimalId = null,
  isRtl = false,
}: ClientAnimalStepProps) {
  const { t } = useTranslation();

  // Client search state
  const [searchQuery, setSearchQuery] = useState("");
  const [isSearching, setIsSearching] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | null>(initialClient);
  const [searchError, setSearchError] = useState<string | null>(null);

  // Animal selection state
  const [selectedAnimalId, setSelectedAnimalId] = useState<string>(initialAnimalId || "");

  // Fetch animals for selected client
  const {
    data: clientAnimals = [],
    isLoading: isLoadingAnimals,
  } = useClientAnimals(selectedClient?.id ?? null);

  // Handle client search
  const handleSearch = async () => {
    if (!searchQuery.trim()) return;

    setIsSearching(true);
    setSearchError(null);

    try {
      // Check if query looks like a GUID
      const isGuid =
        /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(
          searchQuery.trim()
        );

      if (isGuid) {
        const client = await clientApi.getClientById(searchQuery.trim());
        setSelectedClient(client);
        setSelectedAnimalId(""); // Reset animal selection
      } else {
        // Search by name
        const result = await clientApi.getClientBySearch(searchQuery.trim());

        if (result.item.length > 0) {
          setSelectedClient(result.item[0]);
          setSelectedAnimalId(""); // Reset animal selection
        } else {
          setSearchError(t(i18nKeyContainer.appointment.clientNotFound));
          setSelectedClient(null);
        }
      }
    } catch {
      setSearchError(t(i18nKeyContainer.appointment.clientNotFound));
      setSelectedClient(null);
    } finally {
      setIsSearching(false);
    }
  };

  // Handle Enter key in search
  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleSearch();
    }
  };

  // Handle animal selection
  const handleAnimalSelect = (animalId: string) => {
    setSelectedAnimalId(animalId);
    const selectedAnimal = clientAnimals.find((a) => a.id === animalId);
    
    if (selectedClient && selectedAnimal) {
      onSelect({ client: selectedClient, animal: selectedAnimal });
    }
  };

  // Clear selection to search again
  const handleClearClient = () => {
    setSelectedClient(null);
    setSelectedAnimalId("");
    setSearchQuery("");
    setSearchError(null);
  };

  return (
    <div className="space-y-6">
      {/* Client Search Section */}
      {!selectedClient ? (
        <div className="space-y-4">
          <div className="space-y-2">
            <Label className="text-sm font-medium text-slate-700">
              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-slate-400" />
                {t(i18nKeyContainer.appointment.searchClient)}
              </div>
            </Label>
            <div className="flex gap-2">
              <Input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={handleKeyPress}
                placeholder={t(i18nKeyContainer.appointment.clientSearchPlaceholder)}
                className="h-11 flex-1 border-slate-200 focus:border-primary focus:ring-primary"
                disabled={isSearching}
              />
              <Button
                type="button"
                onClick={handleSearch}
                disabled={!searchQuery.trim() || isSearching}
                className="h-11 min-w-25 cursor-pointer"
              >
                {isSearching ? (
                  <>
                    <Loader2 className="me-2 h-4 w-4 animate-spin" />
                    {t(i18nKeyContainer.appointment.searching)}
                  </>
                ) : (
                  t(i18nKeyContainer.appointment.search)
                )}
              </Button>
            </div>
          </div>

          {/* Search Error */}
          {searchError && (
            <div className="flex items-center gap-2 rounded-lg bg-red-50 p-3 text-sm text-red-700">
              <XCircle className="h-4 w-4 shrink-0" />
              <span>{searchError}</span>
            </div>
          )}
        </div>
      ) : (
        /* Client Found - Show Info Card */
        <div className="space-y-4">
          {/* Client Info Card */}
          <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4">
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-start gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-emerald-100">
                  <User className="h-5 w-5 text-emerald-600" />
                </div>
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <CheckCircle className="h-4 w-4 text-emerald-600" />
                    <span className="text-xs font-medium uppercase tracking-wide text-emerald-600">
                      {t(i18nKeyContainer.appointment.clientFound)}
                    </span>
                  </div>
                  <p className="font-medium text-slate-900">
                    {selectedClient.fullName}
                  </p>
                  <div className="flex items-center gap-1.5 text-sm text-slate-600">
                    <Phone className="h-3.5 w-3.5" />
                    <span dir="ltr">{selectedClient.phone}</span>
                  </div>
                </div>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleClearClient}
                className="text-slate-500 hover:text-slate-700 cursor-pointer"
              >
                {t(i18nKeyContainer.appointment.search)}
              </Button>
            </div>
          </div>

          {/* Animal Selection */}
          <div className="space-y-2">
            <Label className="text-sm font-medium text-slate-700">
              <div className="flex items-center gap-2">
                <PawPrint className="h-4 w-4 text-slate-400" />
                {t(i18nKeyContainer.appointment.selectAnimal)}
              </div>
            </Label>

            {isLoadingAnimals ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-6 w-6 animate-spin text-primary" />
              </div>
            ) : clientAnimals.length === 0 ? (
              <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-center">
                <PawPrint className="mx-auto h-8 w-8 text-amber-400" />
                <p className="mt-2 text-sm text-amber-700">
                  {t(i18nKeyContainer.appointment.noAnimals)}
                </p>
              </div>
            ) : (
              <Select
                value={selectedAnimalId}
                onValueChange={handleAnimalSelect}
                dir={isRtl ? "rtl" : "ltr"}
              >
                <SelectTrigger className="h-11 border-slate-200 cursor-pointer">
                  <SelectValue
                    placeholder={t(i18nKeyContainer.appointment.selectAnimalPlaceholder)}
                  />
                </SelectTrigger>
                <SelectContent className="bg-white border-0 shadow-lg">
                  {clientAnimals.map((animal) => (
                    <SelectItem 
                      key={animal.id} 
                      value={animal.id}
                      className="cursor-pointer hover:bg-slate-100 focus:bg-slate-100"
                    >
                      <div className="flex items-center gap-2">
                        <PawPrint className="h-4 w-4 text-slate-400" />
                        <span className="font-medium">{animal.name}</span>
                        <span className="text-slate-500">•</span>
                        <span className="text-slate-500">{animal.species}</span>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>

          {/* Selected Animal Preview */}
          {selectedAnimalId && (
            <div className="rounded-lg border border-primary/20 bg-primary/5 p-3">
              {(() => {
                const animal = clientAnimals.find((a) => a.id === selectedAnimalId);
                if (!animal) return null;
                return (
                  <div className="flex items-center gap-3">
                    <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10">
                      <PawPrint className="h-4 w-4 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium text-slate-900">{animal.name}</p>
                      <p className="text-sm text-slate-500">
                        {animal.species}
                        {animal.breed && ` • ${animal.breed}`}
                      </p>
                    </div>
                  </div>
                );
              })()}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
