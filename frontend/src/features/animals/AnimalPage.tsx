import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import AnimalDataTable from "./AnimalDataTable";
import AddUpdateAnimal from "./add-update-animal";
import ConfirmDeleteDialog from "@/components/ui/confirm-delete-dialog";
import { useDeleteAnimal } from "./use-delete-animal";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import type { Animal } from "./animal-api";

export default function AnimalPage() {
    const [addAnimalOpen, setAddAnimalOpen] = useState(false);
    const [editAnimalId, setEditAnimalId] = useState<string | null>(null);
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [animalToDelete, setAnimalToDelete] = useState<Animal | null>(null);
    
    const { t, i18n } = useTranslation();
    const isRtl = i18n.language === "ar";
    const { deleteAnimal, isDeleting } = useDeleteAnimal();

    const handleOpenAdd = () => {
        setEditAnimalId(null);
        setAddAnimalOpen(true);
    };

    const handleEdit = (animal: Animal) => {
        setEditAnimalId(animal.id);
        setAddAnimalOpen(true);
    };

    const handleView = (animal: Animal) => {
        console.log("View animal:", animal);
        // TODO: Implement view functionality
    };

    const handleDelete = (animal: Animal) => {
        setAnimalToDelete(animal);
        setDeleteDialogOpen(true);
    };

    const handleConfirmDelete = () => {
        if (animalToDelete) {
            deleteAnimal(animalToDelete.id, {
                onSuccess: () => {
                    setDeleteDialogOpen(false);
                    setAnimalToDelete(null);
                },
                onError: () => {
                    // Keep dialog open on error so user can retry or cancel
                },
            });
        }
    };

    const handleCancelDelete = () => {
        setDeleteDialogOpen(false);
        setAnimalToDelete(null);
    };

    const handleClose = () => {
        setAddAnimalOpen(false);
        setEditAnimalId(null);
    };

    return (
        <div dir={isRtl ? "rtl" : "ltr"}>
            <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-6">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">
                        {t(i18nKeyContainer.animal.title)}
                    </h1>
                    <p className="text-slate-500">
                        {t(i18nKeyContainer.animal.description)}
                    </p>
                </div>
                <Button
                    className="sm:ms-auto gap-2 cursor-pointer w-full sm:w-auto"
                    onClick={handleOpenAdd}
                >
                    <Plus className="h-4 w-4" />
                    {t(i18nKeyContainer.animal.addAnimal)}
                </Button>
            </div>
            <div>
                <AnimalDataTable
                    onView={handleView}
                    onEdit={handleEdit}
                    onDelete={handleDelete}
                />
            </div>
            <AddUpdateAnimal
                open={addAnimalOpen}
                onClose={handleClose}
                animalId={editAnimalId || undefined}
            />
            <ConfirmDeleteDialog
                open={deleteDialogOpen}
                onClose={handleCancelDelete}
                onConfirm={handleConfirmDelete}
                title={t(i18nKeyContainer.deleteDialog.animal.title)}
                description={t(i18nKeyContainer.deleteDialog.animal.description)}
                itemName={animalToDelete?.name}
                isLoading={isDeleting}
            />
        </div>
    );
}