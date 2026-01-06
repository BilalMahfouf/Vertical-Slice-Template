import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import ClientDataTable from "./client-table";
import AddClient from "./add-client";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export default function ClientPage() {
    const [addClientOpen, setAddClientOpen] = useState(false);
    const { t, i18n } = useTranslation();
    const isRtl = i18n.language === "ar";

    return (
        <div dir={isRtl ? "rtl" : "ltr"}>
            <div className="flex flex-row items-center mb-6">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">
                        {t(i18nKeyContainer.client.title)}
                    </h1>
                    <p className="text-slate-500">
                        {t(i18nKeyContainer.client.description)}
                    </p>
                </div>
                <Button
                    className="ms-auto gap-2 cursor-pointer"
                    onClick={() => setAddClientOpen(true)}
                >
                    <Plus className="h-4 w-4" />
                    {t(i18nKeyContainer.client.addNewClient)}
                </Button>
            </div>
            <div>
                <ClientDataTable />
            </div>
            <AddClient open={addClientOpen} onClose={() => setAddClientOpen(false)} />
        </div>
    );
}