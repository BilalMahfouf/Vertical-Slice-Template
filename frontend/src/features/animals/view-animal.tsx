import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { PawPrint, Phone, User, Calendar, Hash, Palette, Cpu, Heart } from "lucide-react";
import animalApi from "./animal-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

interface ViewAnimalProps {
  open: boolean;
  onClose: () => void;
  animalId: string;
}

export default function ViewAnimal({ open, onClose, animalId }: ViewAnimalProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  const { data: animal, isLoading } = useQuery({
    queryKey: ["animal", animalId],
    queryFn: () => animalApi.getAnimalById(animalId),
    enabled: open && !!animalId,
  });

  const formatDate = (dateString: string | null) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    return date.toLocaleDateString(i18n.language, {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const getStatusLabel = (status: string) => {
    const statusMap: Record<string, string> = {
      Active: t(i18nKeyContainer.animal.statusActive),
      UnderTreatment: t(i18nKeyContainer.animal.statusUnderTreatment),
      Recovered: t(i18nKeyContainer.animal.statusRecovered),
      Critical: t(i18nKeyContainer.animal.statusCritical),
      Deceased: t(i18nKeyContainer.animal.statusDeceased),
    };
    return statusMap[status] || status;
  };

  const getGenderLabel = (gender: string) => {
    const genderMap: Record<string, string> = {
      Male: t(i18nKeyContainer.animal.male),
      Female: t(i18nKeyContainer.animal.female),
    };
    return genderMap[gender] || gender;
  };

  const getStatusColor = (status: string) => {
    const colorMap: Record<string, string> = {
      Active: "bg-green-100 text-green-700",
      UnderTreatment: "bg-amber-100 text-amber-700",
      Recovered: "bg-blue-100 text-blue-700",
      Critical: "bg-red-100 text-red-700",
      Deceased: "bg-slate-100 text-slate-700",
    };
    return colorMap[status] || "bg-slate-100 text-slate-700";
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent
        className="max-w-2xl p-0 bg-white max-h-[90vh] flex flex-col"
        dir={isRtl ? "rtl" : "ltr"}
        onInteractOutside={(e) => e.preventDefault()}
      >
        <div className="w-full flex flex-col overflow-hidden">
          {/* Header Section */}
          <div className="border-b border-slate-200 px-6 py-6 shrink-0">
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                <PawPrint className="h-6 w-6 text-primary" />
              </div>
              <div className="flex-1">
                <h2 className="text-xl font-semibold text-slate-900">
                  {t(i18nKeyContainer.animal.viewTitle)}
                </h2>
                <p className="text-sm text-slate-500 mt-0.5">
                  {t(i18nKeyContainer.animal.viewDescription)}
                </p>
              </div>
            </div>
          </div>

          {/* Content Section */}
          <div className="px-6 py-6 overflow-y-auto flex-1">
            {isLoading ? (
              <div className="space-y-4">
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
              </div>
            ) : animal ? (
              <div className="space-y-4">
                {/* Animal ID */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.animal.animalId)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Hash className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-mono text-sm">{animal.id}</span>
                  </div>
                </div>

                {/* Name and Species */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Name */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.name)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <PawPrint className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">{animal.name}</span>
                    </div>
                  </div>

                  {/* Species */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.species)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <span className="text-slate-900">{animal.species}</span>
                    </div>
                  </div>
                </div>

                {/* Breed and Gender */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Breed */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.breed)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <span className="text-slate-900">
                        {animal.breed || <span className="text-slate-400 italic">{t(i18nKeyContainer.animal.noData)}</span>}
                      </span>
                    </div>
                  </div>

                  {/* Gender */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.gender)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <span className="text-slate-900">{getGenderLabel(animal.gender)}</span>
                    </div>
                  </div>
                </div>

                {/* Color and Birth Date */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Color */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.color)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Palette className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {animal.color || <span className="text-slate-400 italic">{t(i18nKeyContainer.animal.noData)}</span>}
                      </span>
                    </div>
                  </div>

                  {/* Birth Date */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.animal.birthDate)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Calendar className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">
                        {formatDate(animal.birthDate) || <span className="text-slate-400 italic">{t(i18nKeyContainer.animal.noData)}</span>}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Microchip Number */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.animal.microchipNumber)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Cpu className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-mono text-sm">
                      {animal.microchipNumber || <span className="text-slate-400 italic font-sans">{t(i18nKeyContainer.animal.noData)}</span>}
                    </span>
                  </div>
                </div>

                {/* Status */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.animal.status)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Heart className="h-4 w-4 text-slate-400" />
                    <span className={`px-2.5 py-0.5 rounded-full text-sm font-medium ${getStatusColor(animal.status)}`}>
                      {getStatusLabel(animal.status)}
                    </span>
                  </div>
                </div>

                {/* Owner Info */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.animal.owner)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <User className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">{animal.clientName}</span>
                    <span className="text-slate-400">•</span>
                    <Phone className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-600">{animal.clientPhone}</span>
                  </div>
                </div>

                {/* Registered Date */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.animal.registeredOn)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Calendar className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">
                      {formatDate(animal.createdOnUtc)}
                    </span>
                  </div>
                </div>

                {/* Close Button */}
                <div className="pt-2 sticky bottom-0 bg-white pb-1">
                  <Button
                    onClick={onClose}
                    className="w-full cursor-pointer"
                  >
                    {t(i18nKeyContainer.animal.close)}
                  </Button>
                </div>
              </div>
            ) : null}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
