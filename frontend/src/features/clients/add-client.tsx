import { useState, useEffect } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { User, Phone, FileText, Hash } from "lucide-react";
import clientApi, { type CreateClientRequest } from "./client-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { useClientToast } from "./use-client-toast";

const MODE_ADDNEW = "addnew";
const MODE_UPDATE = "update";

interface AddClientProps {
  open: boolean;
  onClose: () => void;
  clientId?: string;
}

export default function AddClient({ open, onClose, clientId }: AddClientProps) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");
  const [notes, setNotes] = useState("");
  const [mode, setMode] = useState<typeof MODE_ADDNEW | typeof MODE_UPDATE>(MODE_ADDNEW);

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const clientToast = useClientToast();

  // Set mode based on clientId
  useEffect(() => {
    const changeMode = () => {
    setMode(clientId ? MODE_UPDATE : MODE_ADDNEW);
    }
    changeMode();
  }, [clientId]);

  const queryClient = useQueryClient();

  // Fetch client data in update mode
  const { data: clientData } = useQuery({
    queryKey: ["client", clientId],
    queryFn: () => clientApi.getClientById(clientId!),
    enabled: mode === MODE_UPDATE && open && !!clientId,
  });

  // Fill form when client data is loaded
  useEffect(() => {
  const fillForm = ()=>{
if (clientData && mode === MODE_UPDATE) {
      const [first, ...rest] = clientData.fullName.split(" ");
      setFirstName(first || "");
      setLastName(rest.join(" ") || "");
      setPhone(clientData.phone);
      setNotes(clientData.notes || "");
    }
  }  
    fillForm();
  }, [clientData, mode]);

  const mutation = useMutation({
    mutationFn: (data: { id?: string; request: CreateClientRequest }) => {
      if (mode === MODE_UPDATE && data.id) {
         clientApi.updateClient(data.id, data.request);
      }
      return clientApi.addClient(data.request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["clients"] });
      if (mode === MODE_UPDATE) {
        queryClient.invalidateQueries({ queryKey: ["client", clientId] });
        clientToast.updated();
      } else {
        clientToast.added();
      }
      resetForm();
      onClose();
    },
    onError: (error) => {
      clientToast.error(error);
    },
  });

  const resetForm = () => {
    setFirstName("");
    setLastName("");
    setPhone("");
    setNotes("");
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (firstName && lastName && phone) {
      const request: CreateClientRequest = {
        firstName,
        lastName,
        phone,
        notes: notes || undefined,
      };
      
      mutation.mutate({
        id: mode === MODE_UPDATE ? clientId : undefined,
        request,
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent 
        className="max-w-lg p-0 overflow-hidden bg-white" 
        dir={isRtl ? "rtl" : "ltr"}
        onInteractOutside={(e) => e.preventDefault()}
      >
        <div className="w-full">
          <div className="space-y-1 pb-6 pt-8 text-center border-b border-slate-100 px-8">
            <div className="flex justify-center mb-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
                <User className="h-6 w-6 text-primary" />
              </div>
            </div>
            <h2 className="text-2xl font-bold">
              {t(mode === MODE_UPDATE ? i18nKeyContainer.client.updateTitle : i18nKeyContainer.client.addTitle)}
            </h2>
            <p className="text-slate-500">
              {t(mode === MODE_UPDATE ? i18nKeyContainer.client.updateDescription : i18nKeyContainer.client.addDescription)}
            </p>
          </div>
          <div className="pt-8 pb-8 px-8">
            <form onSubmit={handleSubmit} className="space-y-5">
              {mode === MODE_UPDATE && clientId && (
                <div className="space-y-2">
                  <Label htmlFor="clientId">
                    {t(i18nKeyContainer.client.clientId)}
                  </Label>
                  <div className="relative">
                    <Hash className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                    <Input
                      id="clientId"
                      type="text"
                      className="bg-slate-100 border-slate-200 h-11 ps-10 text-slate-500"
                      value={clientId}
                      readOnly
                    />
                  </div>
                </div>
              )}
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">
                    {t(i18nKeyContainer.client.firstName)}
                  </Label>
                  <div className="relative">
                    <User className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                    <Input
                      id="firstName"
                      type="text"
                      placeholder={t(i18nKeyContainer.client.firstNamePlaceholder)}
                      className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
                      value={firstName}
                      onChange={(e) => setFirstName(e.target.value)}
                      required
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">
                    {t(i18nKeyContainer.client.lastName)}
                  </Label>
                  <div className="relative">
                    <User className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                    <Input
                      id="lastName"
                      type="text"
                      placeholder={t(i18nKeyContainer.client.lastNamePlaceholder)}
                      className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
                      value={lastName}
                      onChange={(e) => setLastName(e.target.value)}
                      required
                    />
                  </div>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="phone">
                  {t(i18nKeyContainer.client.phoneNumber)}
                </Label>
                <div className="relative">
                  <Phone className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                  <Input
                    id="phone"
                    type="tel"
                    placeholder={t(i18nKeyContainer.client.phonePlaceholder)}
                    className="bg-slate-50 border-slate-200 h-11 ps-10 transition-all focus:bg-white"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    required
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="notes">
                  {t(i18nKeyContainer.client.notesOptional)}
                </Label>
                <div className="relative">
                  <FileText className="absolute start-3 top-3 h-4 w-4 text-slate-400" />
                  <textarea
                    id="notes"
                    placeholder={t(i18nKeyContainer.client.notesPlaceholder)}
                    className="flex min-h-24 w-full rounded-md border border-slate-200 bg-slate-50 px-3 py-2 ps-10 text-base ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 transition-all focus:bg-white md:text-sm"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                  />
                </div>
              </div>
              <div className="flex gap-3 pt-4">
                <Button
                  type="button"
                  variant="outline"
                  className="flex-1 h-11 cursor-pointer"
                  onClick={onClose}
                  disabled={mutation.isPending}
                >
                  {t(i18nKeyContainer.client.cancel)}
                </Button>
                <Button
                  type="submit"
                  className="flex-1 h-11 font-bold shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-all cursor-pointer"
                  disabled={mutation.isPending}
                >
                  {mutation.isPending
                    ? t(mode === MODE_UPDATE ? i18nKeyContainer.client.updating : i18nKeyContainer.client.adding)
                    : t(mode === MODE_UPDATE ? i18nKeyContainer.client.updateClient : i18nKeyContainer.client.addClient)}
                </Button>
              </div>
            </form>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
