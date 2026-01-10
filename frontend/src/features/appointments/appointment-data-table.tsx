import {
  DataTable,
  DataTableColumnHeader,
  DataTableRowActions,
  DateCell,
  type DataTableColumn,
  type RowAction,
} from "@/components/tables";
import appointmentApi, { type Appointment } from "./appointment-api";
import { Calendar, User, PawPrint, CalendarClock, XCircle, Trash2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface AppointmentDataTableProps {
  onReschedule?: (appointment: Appointment) => void;
  onCancel?: (appointment: Appointment) => void;
  onDelete?: (appointment: Appointment) => void;
}

// Status badge component for appointments
function AppointmentStatusBadge({ status }: { status: string }) {
  const normalizedStatus = status.toLowerCase();

  const statusConfig: Record<string, { bg: string; text: string; dot: string }> = {
    confirmed: {
      bg: "bg-emerald-50",
      text: "text-emerald-700",
      dot: "bg-emerald-500",
    },
    cancelled: {
      bg: "bg-red-50",
      text: "text-red-700",
      dot: "bg-red-500",
    },
    completed: {
      bg: "bg-blue-50",
      text: "text-blue-700",
      dot: "bg-blue-500",
    },
    rescheduled: {
      bg: "bg-amber-50",
      text: "text-amber-700",
      dot: "bg-amber-500",
    },
    pending: {
      bg: "bg-slate-50",
      text: "text-slate-700",
      dot: "bg-slate-400",
    },
  };

  const config = statusConfig[normalizedStatus] || statusConfig.pending;

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
        config.bg,
        config.text
      )}
    >
      <span className={cn("h-1.5 w-1.5 rounded-full", config.dot)} />
      {status}
    </span>
  );
}

export default function AppointmentDataTable({
  onReschedule,
  onCancel,
  onDelete,
}: AppointmentDataTableProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  // Check if appointment can be rescheduled (only confirmed appointments)
  const canReschedule = (appointment: Appointment): boolean => {
    const status = appointment.status.toLowerCase();
    return status === "confirmed" || status === "rescheduled";
  };

  // Check if appointment can be canceled
  const canCancel = (appointment: Appointment): boolean => {
    const status = appointment.status.toLowerCase();
    return status !== "cancelled" && status !== "completed";
  };

  const appointmentColumns: DataTableColumn<Appointment>[] = [
    {
      accessorKey: "appointmentDate",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.appointment.dateTime)}
          enableSorting={false}
        />
      ),
      cell: ({ row }) => {
        const date = new Date(row.original.appointmentDate);
        const formattedDate = date.toLocaleDateString(undefined, {
          weekday: "short",
          month: "short",
          day: "numeric",
        });
        const formattedTime = date.toLocaleTimeString(undefined, {
          hour: "2-digit",
          minute: "2-digit",
        });

        return (
          <div className="flex items-start gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
              <Calendar className="h-5 w-5 text-primary" />
            </div>
            <div className="space-y-0.5">
              <div className="font-medium text-slate-900">{formattedDate}</div>
              <div className="text-sm text-slate-500">{formattedTime}</div>
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "clientName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.appointment.client)}
          enableSorting={false}
        />
      ),
      cell: ({ row }) => (
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <User className="h-5 w-5 text-primary" />
          </div>
          <div className="space-y-0.5">
            <div className="font-medium text-slate-900">
              {row.original.clientName}
            </div>
            {row.original.animalName && (
              <div className="flex items-center gap-1.5 text-sm text-slate-500">
                <PawPrint className="h-3.5 w-3.5" />
                <span>{row.original.animalName}</span>
              </div>
            )}
          </div>
        </div>
      ),
    },
    {
      accessorKey: "status",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.appointment.status)}
          enableSorting={false}
        />
      ),
      cell: ({ row }) => <AppointmentStatusBadge status={row.original.status} />,
    },
    {
      accessorKey: "createdOnUtc",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.appointment.createdOn)}
          enableSorting={false}
        />
      ),
      cell: ({ row }) => <DateCell date={row.original.createdOnUtc} />,
    },
    {
      id: "actions",
      header: () => (
        <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
          {t(i18nKeyContainer.table.openMenu)}
        </span>
      ),
      cell: ({ row }) => {
        const actions: RowAction<Appointment>[] = [];

        if (canReschedule(row.original)) {
          actions.push({
            label: t(i18nKeyContainer.appointment.reschedule),
            onClick: () => onReschedule?.(row.original),
            icon: CalendarClock,
          });
        }

        if (canCancel(row.original)) {
          actions.push({
            label: t(i18nKeyContainer.appointment.cancel),
            onClick: () => onCancel?.(row.original),
            icon: XCircle,
            variant: "destructive",
            separator: actions.length > 0,
          });
        }

        // Delete action - always available
        actions.push({
          label: t(i18nKeyContainer.appointment.delete),
          onClick: () => onDelete?.(row.original),
          icon: Trash2,
          variant: "destructive",
          separator: actions.length > 0,
        });

        if (actions.length === 0) {
          return <span className="text-sm text-slate-400">—</span>;
        }

        return <DataTableRowActions row={row.original} actions={actions} />;
      },
    },
  ];

  return (
    <div dir={isRtl ? "rtl" : "ltr"}>
      <DataTable
        columns={appointmentColumns}
        queryFn={appointmentApi.getAllAppointments}
        queryKey="appointments"
        searchPlaceholder={t(i18nKeyContainer.appointment.searchPlaceholder)}
        defaultPageSize={10}
        enableSearch={true}
        emptyMessage={t(i18nKeyContainer.appointment.noAppointments)}
        searchDebounceMs={500}
      />
    </div>
  );
}
