import {
  DataTable,
  DataTableColumnHeader,
  DataTableRowActions,
  type DataTableColumn,
  type RowAction,
} from "@/components/tables";
import vaccinationApi, { type VaccinationTableResponse } from "./vaccination-api";
import { Calendar, User, PawPrint, Eye, Pencil, Trash2, Syringe, AlertCircle } from "lucide-react";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface VaccinationDataTableProps {
  onView?: (vaccination: VaccinationTableResponse) => void;
  onEdit?: (vaccination: VaccinationTableResponse) => void;
  onDelete?: (vaccination: VaccinationTableResponse) => void;
}

/**
 * Due Date Badge Component
 * Displays due date with color coding based on urgency
 */
function DueDateBadge({ dueDate }: { dueDate: string | null }) {
  if (!dueDate) {
    return <span className="text-sm text-slate-400">—</span>;
  }

  const due = new Date(dueDate);
  const now = new Date();
  const daysUntilDue = Math.ceil((due.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  let colorClasses: { bg: string; text: string; dot: string };

  if (daysUntilDue < 0) {
    // Overdue
    colorClasses = {
      bg: "bg-red-50",
      text: "text-red-700",
      dot: "bg-red-500",
    };
  } else if (daysUntilDue <= 30) {
    // Due soon
    colorClasses = {
      bg: "bg-amber-50",
      text: "text-amber-700",
      dot: "bg-amber-500",
    };
  } else {
    // Not urgent
    colorClasses = {
      bg: "bg-green-50",
      text: "text-green-700",
      dot: "bg-green-500",
    };
  }

  const formattedDate = due.toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
        colorClasses.bg,
        colorClasses.text
      )}
    >
      <span className={cn("h-1.5 w-1.5 rounded-full", colorClasses.dot)} />
      {formattedDate}
      {daysUntilDue < 0 && <AlertCircle className="h-3 w-3 ms-1" />}
    </span>
  );
}

/**
 * VaccinationDataTable Component
 * 
 * Displays a paginated, searchable, and sortable table of vaccination records.
 * Features:
 * - RTL (Right-to-Left) language support
 * - i18n integration with keyContainer pattern
 * - Color-coded due date badges
 * - Animal and client information display
 * - View, Edit and Delete actions
 */
export default function VaccinationDataTable({
  onView,
  onEdit,
  onDelete,
}: VaccinationDataTableProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  const vaccinationColumns: DataTableColumn<VaccinationTableResponse>[] = [
    {
      // Given Date Column
      accessorKey: "givenAt",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.vaccination.givenAt)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => {
        const date = new Date(row.original.givenAt);
        const formattedDate = date.toLocaleDateString(undefined, {
          weekday: "short",
          month: "short",
          day: "numeric",
          year: "numeric",
        });

        return (
          <div className="flex items-start gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
              <Calendar className="h-5 w-5 text-primary" />
            </div>
            <div className="space-y-0.5">
              <div className="font-medium text-slate-900">{formattedDate}</div>
            </div>
          </div>
        );
      },
    },
    {
      // Vaccine Name Column
      accessorKey: "vaccinationName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.vaccination.vaccineName)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <Syringe className="h-4 w-4 text-primary" />
          <span className="font-medium">{row.original.vaccinationName}</span>
        </div>
      ),
    },
    {
      // Animal Column
      accessorKey: "animalName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.vaccination.animal)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => (
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <PawPrint className="h-5 w-5 text-primary" />
          </div>
          <div className="space-y-0.5">
            <div className="font-medium text-slate-900">
              {row.original.animalName}
            </div>
          </div>
        </div>
      ),
    },
    {
      // Client Column
      accessorKey: "clientName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.vaccination.client)}
          enableSorting={true}
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
          </div>
        </div>
      ),
    },
    {
      // Due Date Column - With urgency highlighting
      accessorKey: "dueTo",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.vaccination.nextDueDate)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => <DueDateBadge dueDate={row.original.dueTo} />,
    },
    {
      // Actions Column
      id: "actions",
      header: () => (
        <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
          {t(i18nKeyContainer.table.openMenu)}
        </span>
      ),
      cell: ({ row }) => {
        const actions: RowAction<VaccinationTableResponse>[] = [];

        if (onView) {
          actions.push({
            label: t(i18nKeyContainer.table.viewDetails),
            onClick: () => onView(row.original),
            icon: Eye,
          });
        }

        if (onEdit) {
          actions.push({
            label: t(i18nKeyContainer.table.edit),
            onClick: () => onEdit(row.original),
            icon: Pencil,
          });
        }

        if (onDelete) {
          actions.push({
            label: t(i18nKeyContainer.table.delete),
            onClick: () => onDelete(row.original),
            icon: Trash2,
            variant: "destructive",
            separator: actions.length > 0,
          });
        }

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
        columns={vaccinationColumns}
        queryFn={vaccinationApi.getAllVaccinations}
        queryKey="vaccinations"
        searchPlaceholder={t(i18nKeyContainer.vaccination.searchPlaceholder)}
        defaultPageSize={10}
        enableSearch={true}
        emptyMessage={t(i18nKeyContainer.vaccination.noVaccinations)}
        searchDebounceMs={500}
      />
    </div>
  );
}
