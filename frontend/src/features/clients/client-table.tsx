import {
  DataTable,
  DataTableColumnHeader,
  DataTableRowActions,
  type DataTableColumn,
  type RowAction,
  TextCell,
  DateCell,
} from "@/components/tables";
import clientApi, { type Client } from "./client-api";
import { User, Eye, Edit, Trash2, Phone, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

interface ClientDataTableProps {
  onView?: (client: Client) => void;
  onEdit?: (client: Client) => void;
  onDelete?: (client: Client) => void;
}

export default function ClientDataTable({
  onView,
  onEdit,
  onDelete,
}: ClientDataTableProps) {
  const { t } = useTranslation();

  const clientColumns: DataTableColumn<Client>[] = [
    {
      accessorKey: "fullName",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Client" enableSorting={false} />
      ),
      cell: ({ row }) => (
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
            <User className="h-5 w-5 text-primary" />
          </div>
          <div className="space-y-0.5">
            <div className="font-medium text-slate-900">{row.original.fullName}</div>
            <div className="flex items-center gap-1.5 text-sm text-slate-500">
              <Phone className="h-3.5 w-3.5" />
              <span>{row.original.phone}</span>
            </div>
          </div>
        </div>
      ),
    },
    {
      accessorKey: "clinicName",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Clinic" enableSorting={false} />
      ),
      cell: ({ row }) => <TextCell primary={row.original.clinicName} />,
    },
    {
      accessorKey: "notes",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Notes" enableSorting={false} />
      ),
      cell: ({ row }) => (
        <div className="max-w-50">
          {row.original.notes ? (
            <div className="flex items-start gap-2">
              <FileText className="h-4 w-4 shrink-0 text-slate-400 mt-0.5" />
              <span className="text-sm text-slate-600 truncate">{row.original.notes}</span>
            </div>
          ) : (
            <span className="text-sm text-slate-400">No notes</span>
          )}
        </div>
      ),
    },
    {
      accessorKey: "createdOnUtc",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Registered" enableSorting={false} />
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
        const actions: RowAction<Client>[] = [
          {
            label: t(i18nKeyContainer.table.viewDetails),
            onClick: () => onView?.(row.original),
            icon: Eye,
          },
          {
            label: t(i18nKeyContainer.table.edit),
            onClick: () => onEdit?.(row.original),
            icon: Edit,
          },
          {
            label: t(i18nKeyContainer.table.delete),
            onClick: () => onDelete?.(row.original),
            icon: Trash2,
            variant: "destructive",
            separator: true,
          },
        ];
        return <DataTableRowActions row={row.original} actions={actions} />;
      },
    },
  ];

  return (
    <DataTable
      columns={clientColumns}
      queryFn={clientApi.getAllClients}
      queryKey="clients"
      searchPlaceholder={t(i18nKeyContainer.table.search)}
      defaultPageSize={10}
      enableSearch={true}
      searchDebounceMs={1000}
      onView={onView}
      onEdit={onEdit}
      onDelete={onDelete}
    />
  );
}
