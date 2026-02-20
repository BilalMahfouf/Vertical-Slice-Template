import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Plus, CalendarDays } from "lucide-react";
import { Button } from "@/components/ui/button";
import AppointmentDataTable from "./appointment-data-table";
import AddUpdateAppointment from "./add-update-appointment";
import ViewAppointment from "./view-appointment";
import ConfirmDeleteDialog from "@/components/ui/confirm-delete-dialog";
import { useCancelAppointment } from "./use-cancel-appointment";
import { useDeleteAppointment } from "./use-delete-appointment";
import { isNotFoundError } from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import type { Appointment } from "./appointment-api";

export default function AppointmentPage() {
  // Modal states
  const [addAppointmentOpen, setAddAppointmentOpen] = useState(false);
  const [rescheduleAppointment, setRescheduleAppointment] = useState<Appointment | null>(null);
  const [viewAppointmentOpen, setViewAppointmentOpen] = useState(false);
  const [appointmentToView, setAppointmentToView] = useState<Appointment | null>(null);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [appointmentToCancel, setAppointmentToCancel] = useState<Appointment | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [appointmentToDelete, setAppointmentToDelete] = useState<Appointment | null>(null);

  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";
  const { cancelAppointment, isCanceling } = useCancelAppointment();
  const { deleteAppointment, isDeleting } = useDeleteAppointment();

  // Handlers
  const handleOpenAdd = () => {
    setRescheduleAppointment(null);
    setAddAppointmentOpen(true);
  };

  const handleReschedule = (appointment: Appointment) => {
    setRescheduleAppointment(appointment);
    setAddAppointmentOpen(true);
  };

  const handleView = (appointment: Appointment) => {
    setAppointmentToView(appointment);
    setViewAppointmentOpen(true);
  };

  const handleCancel = (appointment: Appointment) => {
    setAppointmentToCancel(appointment);
    setCancelDialogOpen(true);
  };

  const handleConfirmCancel = () => {
    if (appointmentToCancel) {
      cancelAppointment(
        { id: appointmentToCancel.id },
        {
          onSuccess: () => {
            setCancelDialogOpen(false);
            setAppointmentToCancel(null);
          },
          onError: () => {
            // Keep dialog open on error so user can retry or cancel
          },
        }
      );
    }
  };

  const handleCancelDialogClose = () => {
    if (!isCanceling) {
      setCancelDialogOpen(false);
      setAppointmentToCancel(null);
    }
  };

  const handleDelete = (appointment: Appointment) => {
    setAppointmentToDelete(appointment);
    setDeleteDialogOpen(true);
  };

  const handleConfirmDelete = () => {
    if (appointmentToDelete) {
      deleteAppointment(appointmentToDelete.id, {
        onSuccess: () => {
          setDeleteDialogOpen(false);
          setAppointmentToDelete(null);
        },
        onError: (error) => {
          // Close dialog on 404 (item already deleted), otherwise keep open for retry
          if (isNotFoundError(error)) {
            setDeleteDialogOpen(false);
            setAppointmentToDelete(null);
          }
        },
      });
    }
  };

  const handleDeleteDialogClose = () => {
    if (!isDeleting) {
      setDeleteDialogOpen(false);
      setAppointmentToDelete(null);
    }
  };

  const handleCloseAddUpdate = () => {
    setAddAppointmentOpen(false);
    setRescheduleAppointment(null);
  };

  return (
    <div dir={isRtl ? "rtl" : "ltr"}>
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center gap-4 mb-6">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
            <CalendarDays className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">
              {t(i18nKeyContainer.appointment.title)}
            </h1>
            <p className="text-slate-500">
              {t(i18nKeyContainer.appointment.description)}
            </p>
          </div>
        </div>
        <Button
          className="sm:ms-auto gap-2 cursor-pointer w-full sm:w-auto"
          onClick={handleOpenAdd}
        >
          <Plus className="h-4 w-4" />
          {t(i18nKeyContainer.appointment.addAppointment)}
        </Button>
      </div>

      {/* Data Table */}
      <div>
        <AppointmentDataTable
          onView={handleView}
          onReschedule={handleReschedule}
          onCancel={handleCancel}
          onDelete={handleDelete}
        />
      </div>

      {/* Add/Reschedule Modal */}
      <AddUpdateAppointment
        open={addAppointmentOpen}
        onClose={handleCloseAddUpdate}
        appointment={rescheduleAppointment}
      />

      {/* View Appointment Modal */}
      {appointmentToView && (
        <ViewAppointment
          open={viewAppointmentOpen}
          onClose={() => {
            setViewAppointmentOpen(false);
            setAppointmentToView(null);
          }}
          appointmentId={appointmentToView.id}
        />
      )}

      {/* Cancel Confirmation Dialog */
      <ConfirmDeleteDialog
        open={cancelDialogOpen}
        onClose={handleCancelDialogClose}
        onConfirm={handleConfirmCancel}
        title={t(i18nKeyContainer.appointment.cancelDialogTitle)}
        description={t(i18nKeyContainer.appointment.cancelDialogDescription)}
        itemName={
          appointmentToCancel
            ? `${appointmentToCancel.clientName} - ${new Date(
                appointmentToCancel.appointmentDate
              ).toLocaleDateString()}`
            : undefined
        }
        isLoading={isCanceling}
        confirmAction={t(i18nKeyContainer.appointment.confirmCancel)}
        actionInProgress={t(i18nKeyContainer.appointment.canceling)}
      />

      /* Delete Confirmation Dialog */}
      <ConfirmDeleteDialog
        open={deleteDialogOpen}
        onClose={handleDeleteDialogClose}
        onConfirm={handleConfirmDelete}
        title={t(i18nKeyContainer.appointment.deleteDialogTitle)}
        description={t(i18nKeyContainer.appointment.deleteDialogDescription)}
        itemName={
          appointmentToDelete
            ? `${appointmentToDelete.clientName} - ${new Date(
                appointmentToDelete.appointmentDate
              ).toLocaleDateString()}`
            : undefined
        }
        isLoading={isDeleting}
      />
    </div>
  );
}