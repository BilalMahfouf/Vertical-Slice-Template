/**
 * Example Usage: Generic DataTable Component
 *
 * This file demonstrates how to use the DataTable component
 * for the three different table designs shown in the UI mockups:
 * 1. Animals/Patients Table
 * 2. Visits Table
 * 3. Clinics Table
 */

import { Building2, Eye, Edit, Trash2 } from "lucide-react";
import {
  DataTable,
  DataTableColumnHeader,
  DataTableRowActions,
  StatusIndicator,
  StatusBadge,
  TextCell,
  IconTextCell,
  DateCell,
  ContactCell,
  LocationCell,
  TimeRangeCell,
  StaffCountCell,
  TypeBadge,
  type TableRequest,
  type PagedList,
  type RowAction,
  type DataTableColumn,
} from "@/components/tables";
import api from "@/lib/api/api";

// ==================== 1. Animals/Patients Table ====================

interface Animal {
  id: string;
  name: string;
  breed: string;
  species: string;
  ownerName: string;
  ownerPhone: string;
  status: string;
  lastVisitDate: string;
}

// API function for animals
async function fetchAnimals(params: TableRequest): Promise<PagedList<Animal>> {
  const response = await api.get("/animals", { params });
  return response.data;
}

// Column definitions for animals table
const animalColumns: DataTableColumn<Animal>[] = [
  {
    accessorKey: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Patient" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <IconTextCell
        primary={row.original.name}
        secondary={row.original.breed}
      />
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
    accessorKey: "ownerName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Owner" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <ContactCell
        name={row.original.ownerName}
        phone={row.original.ownerPhone}
      />
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
    accessorKey: "lastVisitDate",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Date" enableSorting={false} />
    ),
    cell: ({ row }) => <DateCell date={row.original.lastVisitDate} />,
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

// Usage in component:
export function AnimalsTable() {
  return (
    <DataTable
      columns={animalColumns}
      queryFn={fetchAnimals}
      queryKey="animals"
      searchPlaceholder="Search..."
      defaultPageSize={10}
    />
  );
}

// ==================== 2. Visits Table ====================

interface Visit {
  id: string;
  date: string;
  patientName: string;
  patientSpecies: string;
  patientOwner: string;
  diagnosis: string;
  doctorName: string;
  status: string;
}

// API function for visits
async function fetchVisits(params: TableRequest): Promise<PagedList<Visit>> {
  const response = await api.get("/visits", { params });
  return response.data;
}

// Column definitions for visits table
const visitColumns: DataTableColumn<Visit>[] = [
  {
    accessorKey: "date",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Date" />
    ),
    cell: ({ row }) => (
      <span className="font-medium text-slate-900">
        {new Date(row.original.date).toLocaleDateString("en-CA")}
      </span>
    ),
  },
  {
    accessorKey: "patientName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Patient" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <TextCell
        primary={row.original.patientName}
        secondary={`${row.original.patientSpecies} • ${row.original.patientOwner}`}
      />
    ),
  },
  {
    accessorKey: "diagnosis",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Diagnosis" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <span className="text-primary font-medium">{row.original.diagnosis}</span>
    ),
  },
  {
    accessorKey: "doctorName",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Doctor" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <div className="flex items-center gap-2">
        <span className="text-slate-400">👤</span>
        <span className="text-slate-700">{row.original.doctorName}</span>
      </div>
    ),
  },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" enableSorting={false} />
    ),
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    id: "actions",
    cell: ({ row }) => {
      const actions: RowAction<Visit>[] = [
        { label: "View details", onClick: () => console.log("View", row.original), icon: Eye },
        { label: "Edit", onClick: () => console.log("Edit", row.original), icon: Edit },
      ];
      return <DataTableRowActions row={row.original} actions={actions} />;
    },
  },
];

// Usage in component:
export function VisitsTable() {
  return (
    <DataTable
      columns={visitColumns}
      queryFn={fetchVisits}
      queryKey="visits"
      searchPlaceholder="Search..."
      defaultPageSize={10}
    />
  );
}

// ==================== 3. Clinics Table ====================

interface Clinic {
  id: string;
  name: string;
  openingTime: string;
  closingTime: string;
  address: string;
  city: string;
  phone: string;
  email: string;
  type: string;
  status: string;
  staffCount: number;
}
async function fetchClinics(params: TableRequest): Promise<PagedList<Clinic>> {
  // Simulate network delay
  await new Promise((resolve) => setTimeout(resolve, 500));
 const mockClinicsData: Clinic[] = [
  {
    id: "1",
    name: "Downtown Veterinary Clinic",
    openingTime: "8:00 AM",
    closingTime: "6:00 PM",
    address: "123 Main Street",
    city: "Los Angeles",
    phone: "+1 (555) 123-4567",
    email: "downtown@veticloud.com",
    type: "General",
    status: "Active",
    staffCount: 12,
  },
  {
    id: "2",
    name: "Westside Animal Hospital",
    openingTime: "7:00 AM",
    closingTime: "8:00 PM",
    address: "456 Oak Avenue",
    city: "Santa Monica",
    phone: "+1 (555) 234-5678",
    email: "westside@veticloud.com",
    type: "Specialty",
    status: "Active",
    staffCount: 18,
  },
  {
    id: "3",
    name: "Emergency Pet Care Center",
    openingTime: "24/7",
    closingTime: "",
    address: "789 Elm Street",
    city: "Beverly Hills",
    phone: "+1 (555) 345-6789",
    email: "emergency@veticloud.com",
    type: "Emergency",
    status: "Active",
    staffCount: 25,
  },
  {
    id: "4",
    name: "Northridge Vet Clinic",
    openingTime: "9:00 AM",
    closingTime: "5:00 PM",
    address: "321 Pine Road",
    city: "Northridge",
    phone: "+1 (555) 456-7890",
    email: "northridge@veticloud.com",
    type: "General",
    status: "Inactive",
    staffCount: 8,
  },
  {
    id: "5",
    name: "Coastal Animal Wellness",
    openingTime: "8:00 AM",
    closingTime: "7:00 PM",
    address: "654 Beach Blvd",
    city: "Malibu",
    phone: "+1 (555) 567-8901",
    email: "coastal@veticloud.com",
    type: "Wellness",
    status: "Active",
    staffCount: 10,
  },
  {
    id: "6",
    name: "Valley Pet Hospital",
    openingTime: "8:00 AM",
    closingTime: "6:00 PM",
    address: "987 Valley Drive",
    city: "Van Nuys",
    phone: "+1 (555) 678-9012",
    email: "valley@veticloud.com",
    type: "General",
    status: "Active",
    staffCount: 15,
  },
  {
    id: "7",
    name: "Sunset Veterinary Care",
    openingTime: "9:00 AM",
    closingTime: "6:00 PM",
    address: "246 Sunset Blvd",
    city: "West Hollywood",
    phone: "+1 (555) 789-0123",
    email: "sunset@veticloud.com",
    type: "General",
    status: "Active",
    staffCount: 9,
  },
  {
    id: "8",
    name: "Pacific Coast Animal Clinic",
    openingTime: "7:30 AM",
    closingTime: "7:30 PM",
    address: "135 Coast Highway",
    city: "Venice",
    phone: "+1 (555) 890-1234",
    email: "pacific@veticloud.com",
    type: "Specialty",
    status: "Active",
    staffCount: 14,
  },
  {
    id: "9",
    name: "Hollywood Hills Pet Center",
    openingTime: "8:30 AM",
    closingTime: "5:30 PM",
    address: "468 Hills Road",
    city: "Hollywood",
    phone: "+1 (555) 901-2345",
    email: "hills@veticloud.com",
    type: "General",
    status: "Active",
    staffCount: 11,
  },
  {
    id: "10",
    name: "South Bay Veterinary Hospital",
    openingTime: "8:00 AM",
    closingTime: "6:00 PM",
    address: "753 Bay Street",
    city: "Redondo Beach",
    phone: "+1 (555) 012-3456",
    email: "southbay@veticloud.com",
    type: "General",
    status: "Active",
    staffCount: 13,
  },
];
  let filteredData = [...mockClinicsData];

  // Apply search filter
  if (params.search) {
    const searchLower = params.search.toLowerCase();
    filteredData = filteredData.filter(
      (clinic) =>
        clinic.name.toLowerCase().includes(searchLower) ||
        clinic.city.toLowerCase().includes(searchLower) ||
        clinic.type.toLowerCase().includes(searchLower) ||
        clinic.email.toLowerCase().includes(searchLower)
    );
  }

  // Apply sorting
  if (params.sortColumn && params.sortOrder) {
    filteredData.sort((a, b) => {
      const aValue = a[params.sortColumn as keyof Clinic];
      const bValue = b[params.sortColumn as keyof Clinic];

      if (typeof aValue === "string" && typeof bValue === "string") {
        return params.sortOrder === "asc"
          ? aValue.localeCompare(bValue)
          : bValue.localeCompare(aValue);
      }

      if (typeof aValue === "number" && typeof bValue === "number") {
        return params.sortOrder === "asc" ? aValue - bValue : bValue - aValue;
      }

      return 0;
    });
  }

  // Calculate pagination
  const totalCount = filteredData.length;
  const pageSize = params.pageSize || 10;
  const page = params.page || 1;
  const totalPages = Math.ceil(totalCount / pageSize);
  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const items = filteredData.slice(startIndex, endIndex);

  return {
    item: items,
    page,
    pageSize,
    totalCount,
    hasPreviousPage: page > 1,
    hasNextPage: page < totalPages,
  };
}
// Column definitions for clinics table
const clinicColumns: DataTableColumn<Clinic>[] = [
  {
    accessorKey: "name",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Clinic Name" />
    ),
    cell: ({ row }) => (
      <div className="flex items-start gap-3">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10">
          <Building2 className="h-5 w-5 text-primary" />
        </div>
        <div>
          <div className="font-medium text-primary">{row.original.name}</div>
          <TimeRangeCell
            start={row.original.openingTime}
            end={row.original.closingTime}
          />
        </div>
      </div>
    ),
  },
  {
    accessorKey: "address",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Location" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <LocationCell address={row.original.address} city={row.original.city} />
    ),
  },
  {
    accessorKey: "phone",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Contact" enableSorting={false} />
    ),
    cell: ({ row }) => (
      <ContactCell
        name={row.original.phone}
        email={row.original.email}
      />
    ),
  },
  {
    accessorKey: "type",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Type" enableSorting={false} />
    ),
    cell: ({ row }) => <TypeBadge type={row.original.type} />,
  },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" enableSorting={false} />
    ),
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: "staffCount",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Staff" enableSorting={false} />
    ),
    cell: ({ row }) => <StaffCountCell count={row.original.staffCount} />,
  },
  {
    id: "actions",
    cell: ({ row }) => {
      const actions: RowAction<Clinic>[] = [
        { label: "View details", onClick: () => console.log("View", row.original), icon: Eye },
        { label: "Edit", onClick: () => console.log("Edit", row.original), icon: Edit },
        { label: "Delete", onClick: () => console.log("Delete", row.original), icon: Trash2, variant: "destructive", separator: true },
      ];
      return <DataTableRowActions row={row.original} actions={actions} />;
    },
  },
];

// Usage in component:
export function ClinicsTable() {
  return (
    <DataTable
      columns={clinicColumns}
      queryFn={fetchClinics}
      queryKey="clinics"
      searchPlaceholder="Search..."
      defaultPageSize={10}
    />
  );
}
