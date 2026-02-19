import {
  DataTable,
  DataTableColumnHeader,
  DataTableRowActions,
  type DataTableColumn,
  type RowAction,
} from "@/components/tables";
import visitApi, { type VisitTableResponse } from "./visit-api";
import { Calendar, User, PawPrint, Eye, Pencil, Trash2, DollarSign} from "lucide-react";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";
import { cn } from "@/lib/utils";

interface VisitDataTableProps {
  onView?: (visit: VisitTableResponse) => void;
  onEdit?: (visit: VisitTableResponse) => void;
  onDelete?: (visit: VisitTableResponse) => void;
}

/**
 * Visit Type Badge Component
 * Displays visit type with color-coded badge styling
 */
// function VisitTypeBadge({ visitType }: { visitType: string }) {
//   const { t } = useTranslation();
//   const normalizedType = visitType.toLowerCase();

//   // Configuration for different visit types with corresponding colors
//   const typeConfig: Record<string, { bg: string; text: string; dot: string; label: string }> = {
//     clinic: {
//       bg: "bg-blue-50",
//       text: "text-blue-700",
//       dot: "bg-blue-500",
//       label: t(i18nKeyContainer.visit.clinic),
//     },
//     field: {
//       bg: "bg-green-50",
//       text: "text-green-700",
//       dot: "bg-green-500",
//       label: t(i18nKeyContainer.visit.field),
//     },
//     emergency: {
//       bg: "bg-red-50",
//       text: "text-red-700",
//       dot: "bg-red-500",
//       label: t(i18nKeyContainer.visit.emergency),
//     },
//   };

//   const config = typeConfig[normalizedType] || {
//     bg: "bg-slate-50",
//     text: "text-slate-700",
//     dot: "bg-slate-400",
//     label: visitType,
//   };

//   return (
//     <span
//       className={cn(
//         "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
//         config.bg,
//         config.text
//       )}
//     >
//       <span className={cn("h-1.5 w-1.5 rounded-full", config.dot)} />
//       {config.label}
//     </span>
//   );
// }

/**
 * Payment Status Badge Component
 * Displays payment status with color-coded badge styling
 */
function PaymentStatusBadge({ paymentStatus }: { paymentStatus: string }) {
  const { t } = useTranslation();
  const normalizedStatus = paymentStatus.toLowerCase();

  const statusConfig: Record<string, { bg: string; text: string; dot: string; label: string }> = {
    pending: {
      bg: "bg-amber-50",
      text: "text-amber-700",
      dot: "bg-amber-500",
      label: t(i18nKeyContainer.visit.paymentStatusPending),
    },
    paid: {
      bg: "bg-green-50",
      text: "text-green-700",
      dot: "bg-green-500",
      label: t(i18nKeyContainer.visit.paymentStatusPaid),
    },
    partiallypaid: {
      bg: "bg-blue-50",
      text: "text-blue-700",
      dot: "bg-blue-500",
      label: t(i18nKeyContainer.visit.paymentStatusPartiallyPaid),
    },
    refunded: {
      bg: "bg-slate-50",
      text: "text-slate-700",
      dot: "bg-slate-400",
      label: t(i18nKeyContainer.visit.paymentStatusRefunded),
    },
  };

  const config = statusConfig[normalizedStatus] || {
    bg: "bg-slate-50",
    text: "text-slate-700",
    dot: "bg-slate-400",
    label: paymentStatus,
  };

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
        config.bg,
        config.text
      )}
    >
      <span className={cn("h-1.5 w-1.5 rounded-full", config.dot)} />
      {config.label}
    </span>
  );
}

/**
 * VisitDataTable Component
 * 
 * Displays a paginated, searchable, and sortable table of visit records.
 * Features:
 * - RTL (Right-to-Left) language support
 * - i18n integration with keyContainer pattern
 * - Responsive design with icon-based visual hierarchy
 * - Color-coded visit type badges
 * - Date formatting with locale support
 * - Animal and owner information display
 * - View, Edit and Delete actions
 * 
 * @param onView - Callback function when view action is triggered
 * @param onEdit - Callback function when edit action is triggered
 * @param onDelete - Callback function when delete action is triggered
 * @returns A data table component for displaying visit records
 */
export default function VisitDataTable({
  onView,
  onEdit,
  onDelete,
}: VisitDataTableProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === "ar";

  // Define table columns with headers, sorting, and cell rendering
  const visitColumns: DataTableColumn<VisitTableResponse>[] = [
    {
      // Visit Date Column - Displays formatted date with calendar icon
      accessorKey: "visitDate",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.visit.visitDate)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => {
        const date = new Date(row.original.visitDate);
        const formattedDate = date.toLocaleDateString(undefined, {
          weekday: "short",
          month: "short",
          day: "numeric",
          year: "numeric",
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
      // Animal Column - Displays animal name and species with paw icon
      accessorKey: "animalName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.visit.animal)}
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
            <div className="text-sm text-slate-500">
              {row.original.animalSpecies}
            </div>
          </div>
        </div>
      ),
    },
    {
      // Owner Name Column - Displays owner name with user icon
      accessorKey: "ownerName",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.visit.owner)}
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
              {row.original.ownerName}
            </div>
          </div>
        </div>
      ),
    },
    {
      // Payment Amount Column - Displays formatted payment amount
      accessorKey: "paymentAmount",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.visit.paymentAmount)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => {
        const amount = row.original.paymentAmount;
        const formattedAmount = new Intl.NumberFormat(i18n.language, {
          style: "currency",
          currency: "DZD",
          minimumFractionDigits: 2,
        }).format(amount);

        return (
          <div className="flex items-center gap-2">
            <DollarSign className="h-4 w-4 text-slate-400" />
            <span className="font-medium text-slate-900">{formattedAmount}</span>
          </div>
        );
      },
    },
    {
      // Payment Status Column - Displays color-coded badge
      accessorKey: "paymentStatus",
      header: ({ column }) => (
        <DataTableColumnHeader
          column={column}
          title={t(i18nKeyContainer.visit.paymentStatus)}
          enableSorting={true}
        />
      ),
      cell: ({ row }) => <PaymentStatusBadge paymentStatus={row.original.paymentStatus} />,
    },
    {
      // Actions Column - Edit and Delete actions
      id: "actions",
      header: () => (
        <span className="text-xs font-medium uppercase tracking-wide text-slate-500">
          {t(i18nKeyContainer.table.openMenu)}
        </span>
      ),
      cell: ({ row }) => {
        const actions: RowAction<VisitTableResponse>[] = [];

        // View action
        if (onView) {
          actions.push({
            label: t(i18nKeyContainer.table.viewDetails),
            onClick: () => onView(row.original),
            icon: Eye,
          });
        }

        // Edit action
        if (onEdit) {
          actions.push({
            label: t(i18nKeyContainer.table.edit),
            onClick: () => onEdit(row.original),
            icon: Pencil,
          });
        }

        // Delete action
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
    // RTL wrapper - adjusts layout direction based on language
    <div dir={isRtl ? "rtl" : "ltr"}>
      <DataTable
        columns={visitColumns}
        queryFn={visitApi.getAllVisits}
        queryKey="visits"
        searchPlaceholder={t(i18nKeyContainer.visit.searchPlaceholder)}
        defaultPageSize={10}
        enableSearch={true}
        emptyMessage={t(i18nKeyContainer.visit.noVisits)}
        searchDebounceMs={500}
      />
    </div>
  );
}
