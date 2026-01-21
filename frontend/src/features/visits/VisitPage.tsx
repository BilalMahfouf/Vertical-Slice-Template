import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus, Stethoscope } from "lucide-react";
import { Button } from "@/components/ui/button";
import ConfirmDeleteDialog from "@/components/ui/confirm-delete-dialog";
import VisitDataTable from "./visit-data-table";
import AddUpdateVisit from "./add-update-visit";
import { useDeleteVisit } from "./use-delete-visit";
import { type VisitTableResponse } from "./visit-api";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export default function VisitPage() {
  // Dialog states
  const [addUpdateOpen, setAddUpdateOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  
  // Selected visit for edit/delete
  const [selectedVisitId, setSelectedVisitId] = useState<string | null>(null);
  const [visitToDelete, setVisitToDelete] = useState<VisitTableResponse | null>(null);
  
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const { deleteVisit, isDeleting } = useDeleteVisit();

  // Handlers for Add/Update dialog
  const handleOpenAdd = () => {
    setSelectedVisitId(null);
    setAddUpdateOpen(true);
  };

  const handleOpenEdit = (visit: VisitTableResponse) => {
    setSelectedVisitId(visit.id);
    setAddUpdateOpen(true);
  };

  const handleCloseAddUpdate = () => {
    setAddUpdateOpen(false);
    setSelectedVisitId(null);
  };

  // Handlers for Delete dialog
  const handleOpenDelete = (visit: VisitTableResponse) => {
    setVisitToDelete(visit);
    setDeleteDialogOpen(true);
  };

  const handleCloseDelete = () => {
    if (!isDeleting) {
      setDeleteDialogOpen(false);
      setVisitToDelete(null);
    }
  };

  const handleConfirmDelete = () => {
    if (visitToDelete) {
      deleteVisit(visitToDelete.id, {
        onSuccess: () => {
          handleCloseDelete();
        },
      });
    }
  };

  return (
    <div dir={isRtl ? "rtl" : "ltr"}>
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-6">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
            <Stethoscope className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">
              {t(i18nKeyContainer.visit.title)}
            </h1>
            <p className="text-slate-500">
              {t(i18nKeyContainer.visit.description)}
            </p>
          </div>
        </div>
        <Button
          className="sm:ms-auto gap-2 cursor-pointer w-full sm:w-auto"
          onClick={handleOpenAdd}
        >
          <Plus className="h-4 w-4" />
          {t(i18nKeyContainer.visit.addVisit)}
        </Button>
      </div>

      {/* Data Table */}
      <div>
        <VisitDataTable
          onEdit={handleOpenEdit}
          onDelete={handleOpenDelete}
        />
      </div>

      {/* Add/Update Visit Modal */}
      <AddUpdateVisit
        open={addUpdateOpen}
        onClose={handleCloseAddUpdate}
        visitId={selectedVisitId}
      />

      {/* Delete Confirmation Dialog */}
      <ConfirmDeleteDialog
        open={deleteDialogOpen}
        onClose={handleCloseDelete}
        onConfirm={handleConfirmDelete}
        title={t(i18nKeyContainer.deleteDialog.visit.title)}
        description={t(i18nKeyContainer.deleteDialog.visit.description)}
        itemName={visitToDelete?.animalName}
        isLoading={isDeleting}
      />
    </div>
  );
}