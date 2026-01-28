import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import ClientDataTable from "./client-table";
import AddClient from "./add-client";
import ViewClient from "./view-client";
import ConfirmDeleteDialog from "@/components/ui/confirm-delete-dialog";
import { useDeleteClient } from "./use-delete-client";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import type { Client } from "./client-api";

export default function ClientPage() {
    const [addClientOpen, setAddClientOpen] = useState(false);
    const [viewClientOpen, setViewClientOpen] = useState(false);
    const [selectedClientId, setSelectedClientId] = useState<string | null>(null);
    const [editClientId, setEditClientId] = useState<string | null>(null);
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [clientToDelete, setClientToDelete] = useState<Client | null>(null);
    
    const { t, i18n } = useTranslation();
    const isRtl = i18n.language === "ar";
    const { deleteClient, isDeleting } = useDeleteClient();

    const handleView = (client: Client) => {
        setSelectedClientId(client.id);
        setViewClientOpen(true);
    };

    const handleEdit = (client: Client) => {
        setEditClientId(client.id);
        setAddClientOpen(true);
    };

    const handleDelete = (client: Client) => {
        setClientToDelete(client);
        setDeleteDialogOpen(true);
    };

    const handleConfirmDelete = () => {
        if (clientToDelete) {
            deleteClient(clientToDelete.id, {
                onSuccess: () => {
                    setDeleteDialogOpen(false);
                    setClientToDelete(null);
                },
                onError: () => {
                    // Keep dialog open on error so user can retry or cancel
                },
            });
        }
    };

    const handleCancelDelete = () => {
        setDeleteDialogOpen(false);
        setClientToDelete(null);
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
            <ConfirmDeleteDialog
                open={deleteDialogOpen}
                onClose={handleCancelDelete}
                onConfirm={handleConfirmDelete}
                title={t(i18nKeyContainer.deleteDialog.client.title)}
                description={t(i18nKeyContainer.deleteDialog.client.description)}
                itemName={clientToDelete?.fullName}
                isLoading={isDeleting}
            />
        </div>
    );
}