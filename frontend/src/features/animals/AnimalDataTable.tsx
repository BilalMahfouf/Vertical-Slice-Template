import { DataTable, DataTableColumnHeader, DataTableRowActions, type DataTableColumn, type RowAction, TypeBadge, StatusIndicator, DateCell } from "@/components/tables";
import animalApi, { type Animal } from "./animal-api";
import { PawPrint, Eye, Edit, Trash2 } from "lucide-react";
import { Phone } from "lucide-react";

export default function AnimalDataTable() {
    return (
        <DataTable 
        columns={animalColumns}
        queryFn={animalApi.getAllAnimals}
        queryKey="animals"
        searchPlaceholder=""
        defaultPageSize={10}
        enableSearch={true}
        searchDebounceMs={1000}
        />
    )
}

const animalColumns: DataTableColumn<Animal>[] = [
  {
    accessorKey: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Patient" enableSorting={false} />
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
      <DataTableColumnHeader column={column} title="Species" enableSorting={false} />
    ),
    cell: ({ row }) => <TypeBadge type={row.original.species} />,
  },
  {
    accessorKey: "clientName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Owner" enableSorting={false} />
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
      <DataTableColumnHeader column={column} title="Status" enableSorting={false} />
    ),
    cell: ({ row }) => <StatusIndicator status={row.original.status} />,
  },
  {
    accessorKey: "createdOnUtc",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Date" enableSorting={false} />
    ),
    cell: ({ row }) => <DateCell date={row.original.createdOnUtc} />,
  },
  {
    id: "actions",
    header: () => <span className="text-xs font-medium uppercase tracking-wide text-slate-500">Actions</span>,
    cell: ({ row }) => {
      const actions: RowAction<Animal>[] = [
        { label: "View details", onClick: () => console.log("View", row.original), icon: Eye },
        { label: "Edit", onClick: () => console.log("Edit", row.original), icon: Edit },
        { label: "Delete", onClick: () => console.log("Delete", row.original), icon: Trash2, variant: "destructive", separator: true },
      ];
      return <DataTableRowActions row={row.original} actions={actions} />;
    },
  },
];