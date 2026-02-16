import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { User, Phone, Building, Calendar, Hash } from "lucide-react";
import clientApi from "./client-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

interface ViewClientProps {
  open: boolean;
  onClose: () => void;
  clientId: string;
}

export default function ViewClient({ open, onClose, clientId }: ViewClientProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  const { data: client, isLoading } = useQuery({
    queryKey: ["client", clientId],
    queryFn: () => clientApi.getClientById(clientId),
    enabled: open && !!clientId,
  });

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(i18n.language, {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent
        className="max-w-2xl p-0 bg-white"
        dir={isRtl ? "rtl" : "ltr"}
        onInteractOutside={(e) => e.preventDefault()}
      >
        <div className="w-full">
          {/* Header Section */}
          <div className="border-b border-slate-200 px-6 py-6">
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                <User className="h-6 w-6 text-primary" />
              </div>
              <div className="flex-1">
                <h2 className="text-xl font-semibold text-slate-900">
                  {t(i18nKeyContainer.client.viewTitle)}
                </h2>
                <p className="text-sm text-slate-500 mt-0.5">
                  {t(i18nKeyContainer.client.viewDescription)}
                </p>
              </div>
            </div>
          </div>

          {/* Content Section */}
          <div className="px-6 py-6">
            {isLoading ? (
              <div className="space-y-4">
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-16 bg-slate-100 rounded animate-pulse" />
                <div className="h-24 bg-slate-100 rounded animate-pulse" />
              </div>
            ) : client ? (
              <div className="space-y-4">
                {/* Client ID */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.client.clientId)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Hash className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900 font-mono text-sm">{client.id}</span>
                  </div>
                </div>

                {/* Full Name */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.client.fullName)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <User className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">{client.fullName}</span>
                  </div>
                </div>

                {/* Two Column Grid for Phone and Clinic */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {/* Phone Number */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.client.phoneNumber)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Phone className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">{client.phone}</span>
                    </div>
                  </div>

                  {/* Clinic Name */}
                  <div className="space-y-1.5">
                    <label className="text-sm font-medium text-slate-700">
                      {t(i18nKeyContainer.client.clinic)}
                    </label>
                    <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                      <Building className="h-4 w-4 text-slate-400" />
                      <span className="text-slate-900">{client.clinicName}</span>
                    </div>
                  </div>
                </div>

                {/* Registered Date */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.client.registeredOn)}
                  </label>
                  <div className="flex items-center gap-2 p-3 rounded-lg bg-slate-50 border border-slate-200">
                    <Calendar className="h-4 w-4 text-slate-400" />
                    <span className="text-slate-900">
                      {formatDate(client.createdOnUtc)}
                    </span>
                  </div>
                </div>

                {/* Notes */}
                <div className="space-y-1.5">
                  <label className="text-sm font-medium text-slate-700">
                    {t(i18nKeyContainer.client.notes)}
                  </label>
                  <div className="p-3 rounded-lg bg-slate-50 border border-slate-200 min-h-24">
                    {client.notes ? (
                      <p className="text-slate-700 whitespace-pre-wrap wrap-break">
                        {client.notes}
                      </p>
                    ) : (
                      <p className="text-slate-400 italic">
                        {t(i18nKeyContainer.client.noNotes)}
                      </p>
                    )}
                  </div>
                </div>

                {/* Close Button */}
                <div className="pt-2">
                  <Button
                    onClick={onClose}
                    className="w-full cursor-pointer"
                  >
                    {t(i18nKeyContainer.client.close)}
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