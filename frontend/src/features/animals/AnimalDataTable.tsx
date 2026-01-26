import { DataTable, DataTableColumnHeader, DataTableRowActions, type DataTableColumn, type RowAction, TypeBadge, StatusIndicator, DateCell } from "@/components/tables";
import animalApi, { type Animal } from "./animal-api";
import { PawPrint, Eye, Edit, Trash2 } from "lucide-react";
import { Phone } from "lucide-react";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

interface AnimalDataTableProps {
  onView?: (animal: Animal) => void;
  onEdit?: (animal: Animal) => void;
  onDelete?: (animal: Animal) => void;
}

export default function AnimalDataTable({
  onView,
  onEdit,
  onDelete,
}: AnimalDataTableProps) {
    const { t } = useTranslation();

    const animalColumns: DataTableColumn<Animal>[] = [
  {
    accessorKey: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title={t(i18nKeyContainer.animal.name)} enableSorting={false} />
    ),
    cell: ({ row }) => (
      <div className="flex items-start gap-3">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
          <PawPrint className="h-5 w-5 text-primary" />
        </div>
        <div className="space-y-0.5">
          <div className="font-medium text-slate-900">{row.original.name}</div>
          <div className="text-sm text-slate-500">{row.original.breed || "Unknown breed"}</div>
        </div>
      </div>
    ),
  },
  {
    accessorKey: "species",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title={t(i18nKeyContainer.animal.species)} enableSorting={false} />
    ),
    cell: ({ row }) => <TypeBadge type={row.original.species} />,
  },
  {
    accessorKey: "clientName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title={t(i18nKeyContainer.animal.owner)} enableSorting={false} />
    ),
    cell: ({ row }) => (
      <div className="space-y-1">
        <div className="font-medium text-slate-900">{row.original.clientName}</div>
        {row.original.clientPhone && (
          <div className="flex items-center gap-1.5 text-sm text-slate-500">
            <Phone className="h-3.5 w-3.5" />
            <span>{row.original.clientPhone}</span>
          </div>
        )}
      </div>
    ),
  },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title={t(i18nKeyContainer.animal.status)} enableSorting={false} />
    ),
    cell: ({ row }) => <StatusIndicator status={row.original.status} />,
  },
  {
    accessorKey: "createdOnUtc",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title={t(i18nKeyContainer.animal.birthDate)} enableSorting={false} />
    ),
    cell: ({ row }) => <DateCell date={row.original.createdOnUtc} />,
  },
  {
    id: "actions",
    header: () => <span className="text-xs font-medium uppercase tracking-wide text-slate-500">{t(i18nKeyContainer.table.openMenu)}</span>,
    cell: ({ row }) => {
      const actions: RowAction<Animal>[] = [
        { label: t(i18nKeyContainer.table.viewDetails), onClick: () => onView?.(row.original), icon: Eye },
        { label: t(i18nKeyContainer.table.edit), onClick: () => onEdit?.(row.original), icon: Edit },
        { label: t(i18nKeyContainer.table.delete), onClick: () => onDelete?.(row.original), icon: Trash2, variant: "destructive", separator: true },
      ];
      return <DataTableRowActions row={row.original} actions={actions} />;
    },
  },
];

    return (
        <DataTable 
        columns={animalColumns}
        queryFn={animalApi.getAllAnimals}
        queryKey="animals"
        searchPlaceholder={t(i18nKeyContainer.table.search)}
        defaultPageSize={10}
        enableSearch={true}
        searchDebounceMs={1000}
        onView={onView}
        onEdit={onEdit}
        onDelete={onDelete}
        />
    )
}