import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import ClientDataTable from "./client-table";
import AddClient from "./add-client";
import ViewClient from "./view-client";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import type { Client } from "./client-api";

export default function ClientPage() {
    const [addClientOpen, setAddClientOpen] = useState(false);
    const [viewClientOpen, setViewClientOpen] = useState(false);
    const [selectedClientId, setSelectedClientId] = useState<string | null>(null);
    const [editClientId, setEditClientId] = useState<string | null>(null);
    const { t, i18n } = useTranslation();
    const isRtl = i18n.language === "ar";

    const handleView = (client: Client) => {
        setSelectedClientId(client.id);
        setViewClientOpen(true);
    };

    const handleEdit = (client: Client) => {
        setEditClientId(client.id);
        setAddClientOpen(true);
    };

    const handleDelete = (client: Client) => {
        console.log("Delete client:", client);
        // TODO: Implement delete functionality
    };

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
                <ClientDataTable
                    onView={handleView}
                    onEdit={handleEdit}
                    onDelete={handleDelete}
                />
            </div>
            <AddClient 
                open={addClientOpen} 
                onClose={() => {
                    setAddClientOpen(false);
                    setEditClientId(null);
                }} 
                clientId={editClientId || undefined}
            />
            {selectedClientId && (
                <ViewClient
                    open={viewClientOpen}
                    onClose={() => {
                        setViewClientOpen(false);
                        setSelectedClientId(null);
                    }}
                    clientId={selectedClientId}
                />
            )}
        </div>
    );
}