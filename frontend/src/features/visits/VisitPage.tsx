import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus, Stethoscope, Syringe } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import ConfirmDeleteDialog from "@/components/ui/confirm-delete-dialog";
import VisitDataTable from "./visit-data-table";
import AddUpdateVisit from "./add-update-visit";
import ViewVisit from "./view-visit";
import { useDeleteVisit } from "./use-delete-visit";
import { type VisitTableResponse } from "./visit-api";
import VaccinationDataTable from "@/features/vaccinations/vaccination-data-table";
import AddUpdateVaccination from "@/features/vaccinations/add-vaccination";
import ViewVaccination from "@/features/vaccinations/view-vaccination";
import { useDeleteVaccination } from "@/features/vaccinations/use-delete-vaccination";
import { type VaccinationTableResponse } from "@/features/vaccinations/vaccination-api";
import { isNotFoundError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export default function VisitPage() {
  // Tab state
  const [activeTab, setActiveTab] = useState<"visits" | "vaccinations">("visits");

  // Visit dialog states
  const [addUpdateOpen, setAddUpdateOpen] = useState(false);
  const [viewOpen, setViewOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  
  // Selected visit for view/edit/delete
  const [selectedVisitId, setSelectedVisitId] = useState<string | null>(null);
  const [visitToDelete, setVisitToDelete] = useState<VisitTableResponse | null>(null);

  // Vaccination dialog states
  const [vaccinationAddUpdateOpen, setVaccinationAddUpdateOpen] = useState(false);
  const [vaccinationViewOpen, setVaccinationViewOpen] = useState(false);
  const [vaccinationDeleteDialogOpen, setVaccinationDeleteDialogOpen] = useState(false);
  
  // Selected vaccination for view/edit/delete
  const [selectedVaccinationId, setSelectedVaccinationId] = useState<string | null>(null);
  const [vaccinationToDelete, setVaccinationToDelete] = useState<VaccinationTableResponse | null>(null);
  
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const { deleteVisit, isDeleting } = useDeleteVisit();
  const { deleteVaccination, isDeleting: isDeletingVaccination } = useDeleteVaccination();

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

  // Handlers for View dialog
  const handleOpenView = (visit: VisitTableResponse) => {
    setSelectedVisitId(visit.id);
    setViewOpen(true);
  };

  const handleCloseView = () => {
    setViewOpen(false);
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
        onError: (error) => {
          // Close dialog on 404 (item already deleted), otherwise keep open for retry
          if (isNotFoundError(error)) {
            handleCloseDelete();
          }
        },
      });
    }
  };

  // Handlers for Vaccination Add/Update dialog
  const handleOpenAddVaccination = () => {
    setSelectedVaccinationId(null);
    setVaccinationAddUpdateOpen(true);
  };

  const handleOpenEditVaccination = (vaccination: VaccinationTableResponse) => {
    setSelectedVaccinationId(vaccination.id);
    setVaccinationAddUpdateOpen(true);
  };

  const handleCloseVaccinationAddUpdate = () => {
    setVaccinationAddUpdateOpen(false);
    setSelectedVaccinationId(null);
  };

  // Handlers for Vaccination View dialog
  const handleOpenViewVaccination = (vaccination: VaccinationTableResponse) => {
    setSelectedVaccinationId(vaccination.id);
    setVaccinationViewOpen(true);
  };

  const handleCloseViewVaccination = () => {
    setVaccinationViewOpen(false);
    setSelectedVaccinationId(null);
  };

  // Handlers for Vaccination Delete dialog
  const handleOpenDeleteVaccination = (vaccination: VaccinationTableResponse) => {
    setVaccinationToDelete(vaccination);
    setVaccinationDeleteDialogOpen(true);
  };

  const handleCloseDeleteVaccination = () => {
    if (!isDeletingVaccination) {
      setVaccinationDeleteDialogOpen(false);
      setVaccinationToDelete(null);
    }
  };

  const handleConfirmDeleteVaccination = () => {
    if (vaccinationToDelete) {
      deleteVaccination(vaccinationToDelete.id, {
        onSuccess: () => {
          handleCloseDeleteVaccination();
        },
        onError: (error) => {
          // Close dialog on 404 (item already deleted), otherwise keep open for retry
          if (isNotFoundError(error)) {
            handleCloseDeleteVaccination();
          }
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
        <div className="sm:ms-auto flex flex-col sm:flex-row gap-2 w-full sm:w-auto">
          {activeTab === "visits" && (
            <Button
              className="gap-2 cursor-pointer w-full sm:w-auto"
              onClick={handleOpenAdd}
            >
              <Plus className="h-4 w-4" />
              {t(i18nKeyContainer.visit.addVisit)}
            </Button>
          )}
          {activeTab === "vaccinations" && (
            <Button
              className="gap-2 cursor-pointer w-full sm:w-auto"
              onClick={handleOpenAddVaccination}
            >
              <Plus className="h-4 w-4" />
              {t(i18nKeyContainer.vaccination.addVaccination)}
            </Button>
          )}
        </div>
      </div>

      {/* Tabbed Content */}
      <Tabs
        value={activeTab}
        onValueChange={(value) => setActiveTab(value as "visits" | "vaccinations")}
        className="w-full"
      >
        <TabsList className="grid w-full max-w-md grid-cols-2 mb-6">
          <TabsTrigger value="visits" className="gap-2 cursor-pointer">
            <Stethoscope className="h-4 w-4" />
            {t(i18nKeyContainer.visit.tabVisits)}
          </TabsTrigger>
          <TabsTrigger value="vaccinations" className="gap-2 cursor-pointer">
            <Syringe className="h-4 w-4" />
            {t(i18nKeyContainer.vaccination.tabVaccinations)}
          </TabsTrigger>
        </TabsList>

        {/* Visits Tab Content */}
        <TabsContent value="visits" className="mt-0">
          <VisitDataTable
            onView={handleOpenView}
            onEdit={handleOpenEdit}
            onDelete={handleOpenDelete}
          />
        </TabsContent>

        {/* Vaccinations Tab Content */}
        <TabsContent value="vaccinations" className="mt-0">
          <VaccinationDataTable
            onView={handleOpenViewVaccination}
            onEdit={handleOpenEditVaccination}
            onDelete={handleOpenDeleteVaccination}
          />
        </TabsContent>
      </Tabs>

      {/* View Visit Modal */}
      {selectedVisitId && viewOpen && (
        <ViewVisit
          open={viewOpen}
          onClose={handleCloseView}
          visitId={selectedVisitId}
        />
      )}

      {/* Add/Update Visit Modal */}
      <AddUpdateVisit
        open={addUpdateOpen}
        onClose={handleCloseAddUpdate}
        visitId={selectedVisitId}
      />

      {/* Delete Visit Confirmation Dialog */}
      <ConfirmDeleteDialog
        open={deleteDialogOpen}
        onClose={handleCloseDelete}
        onConfirm={handleConfirmDelete}
        title={t(i18nKeyContainer.deleteDialog.visit.title)}
        description={t(i18nKeyContainer.deleteDialog.visit.description)}
        itemName={visitToDelete?.animalName}
        isLoading={isDeleting}
      />

      {/* View Vaccination Modal */}
      {selectedVaccinationId && vaccinationViewOpen && (
        <ViewVaccination
          open={vaccinationViewOpen}
          onClose={handleCloseViewVaccination}
          vaccinationId={selectedVaccinationId}
        />
      )}

      {/* Add/Update Vaccination Modal */}
      <AddUpdateVaccination
        open={vaccinationAddUpdateOpen}
        onClose={handleCloseVaccinationAddUpdate}
        vaccinationId={selectedVaccinationId}
      />

      {/* Delete Vaccination Confirmation Dialog */}
      <ConfirmDeleteDialog
        open={vaccinationDeleteDialogOpen}
        onClose={handleCloseDeleteVaccination}
        onConfirm={handleConfirmDeleteVaccination}
        title={t(i18nKeyContainer.deleteDialog.vaccination.title)}
        description={t(i18nKeyContainer.deleteDialog.vaccination.description)}
        itemName={vaccinationToDelete?.vaccinationName}
        isLoading={isDeletingVaccination}
      />
    </div>
  );
}