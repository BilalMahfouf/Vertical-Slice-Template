# Copilot Instructions - Veterinary Application

> **Purpose**: This document defines the development guidelines, architecture principles, and UI standards for the Veterinary Application. Follow these rules to ensure all features are simple, production-ready, and avoid over-engineering.

---

## Table of Contents

1. [General Development Rules](#general-development-rules)
2. [Architecture Rules](#architecture-rules)
3. [Feature Implementation Rules](#feature-implementation-rules)
4. [Error Handling](#error-handling)
5. [UI Rules](#ui-rules)
6. [RTL Support](#rtl-support)
7. [i18n Rules](#i18n-rules)
8. [Reusable Components](#reusable-components)
9. [Tooling Rules](#tooling-rules)
10. [System Flow](#system-flow)
11. [Communication Rules](#communication-rules)

---

## General Development Rules

- **Keep implementations simple and clean** - Do NOT over-engineer solutions.
- **Always prefer reusable components** - Check existing components before creating new ones.
- **Follow existing project structure and patterns** - Consistency is key.
- **Before writing code, scan the codebase** - Understand the structure and conventions first.
- **User prefers flat response DTOs** - Use flat fields at root level (e.g., `AnimalId, AnimalName, ...`) instead of nested records (e.g., `AnimalInfo Animal, ...`). This applies to all `GetById` and `GetAll` endpoint responses.

---

## Architecture Rules

### The "Dumb Frontend" Pattern

The frontend must stay **"dumb"**. All business logic is handled in the backend.

**Frontend responsibilities (ONLY):**

- Send HTTP requests via Axios
- Receive responses
- Display data
- Handle UI state (modals, loading states, etc.)

**Backend responsibilities:**

- All business logic
- Validation
- Data transformation
- Error generation

### Backend Structure

The backend uses **CQRS pattern** with:

- **Commands**: For create/update/delete operations (e.g., `CreateClient.cs`)
- **Queries**: For read operations (e.g., `GetAllClient.cs`, `GetClientById.cs`)
- **Result pattern**: All handlers return `Result<T>` or `Result`
- **Error types**: `Error.NotFound()`, `Error.Validation()`, `Error.Conflict()`, `Error.Unauthorized()`

```
backend/Veterinary/src/VeterinaryApi/
├── Features/           # Feature-based organization
│   ├── Clients/        # Each feature has its own folder
│   │   ├── CreateClient.cs
│   │   ├── GetAllClient.cs
│   │   ├── GetClientById.cs
│   │   ├── UpdateClient.cs
│   │   └── DeleteClient.cs
│   ├── Animals/
│   ├── Appointments/
│   └── Visits/
├── Domain/             # Domain entities and errors
│   └── Clients/
│       ├── Client.cs
│       └── ClientErrors.cs
└── Common/             # Shared infrastructure
    ├── Errors/
    ├── Results/
    └── CQRS/
```

### Frontend Structure

```
frontend/src/
├── features/           # Feature-based organization
│   ├── clients/
│   │   ├── client-api.ts          # API calls
│   │   ├── ClientPage.tsx         # Main page
│   │   ├── client-table.tsx       # Table component
│   │   ├── add-client.tsx         # Add/Edit form
│   │   ├── view-client.tsx        # View details
│   │   ├── use-client-toast.ts    # Toast notifications
│   │   └── use-delete-client.ts   # Delete hook
│   ├── animals/
│   ├── appointments/
│   └── visits/
├── components/         # Shared UI components
│   ├── ui/             # Base UI components (Button, Dialog, etc.)
│   └── tables/         # Table components
├── lib/
│   ├── api/            # Axios instance and error handling
│   └── i18n/           # Internationalization
└── common/
    └── layouts/        # Layout components
```

---

## Feature Implementation Rules

### 1. API File Pattern

Every feature **MUST** have its own API file containing all related API calls.

**Naming convention**: `{feature}-api.ts` (e.g., `client-api.ts`, `visit-api.ts`, `animal-api.ts`)

**Example structure**:

```typescript
// client-api.ts
import type { PagedList, TableRequest } from "@/components/tables";
import api from "@/lib/api/api";
import { getTableRequsestParams } from "@/lib/utils";

// Types
export type Client = {
  id: string;
  clinicId: string;
  clinicName: string;
  fullName: string;
  phone: string;
  notes?: string;
  numberOfAnimals: number;
  createdOnUtc: string;
};

export type CreateClientRequest = {
  firstName: string;
  lastName: string;
  phone: string;
  notes?: string;
};

// API object with all feature calls
const clientApi = {
  getAllClients: async (request: TableRequest): Promise<PagedList<Client>> => {
    const params = getTableRequsestParams(request);
    const result = await api.get<PagedList<Client>>("/clients", { params });
    if (result.status !== 200) {
      throw new Error("Failed to fetch clients");
    }
    return result.data;
  },

  addClient: async (client: CreateClientRequest): Promise<string> => {
    const result = await api.post<string>("/clients", client);
    if (result.status !== 201) {
      throw new Error("Failed to add client");
    }
    return result.data;
  },

  getClientById: async (id: string): Promise<Client> => {
    const result = await api.get<Client>(`/clients/${id}`);
    if (result.status !== 200) {
      throw new Error("Failed to fetch client");
    }
    return result.data;
  },

  updateClient: async (
    id: string,
    request: CreateClientRequest,
  ): Promise<void> => {
    const result = await api.put<void>(`/clients/${id}`, request);
    if (result.status !== 204) {
      throw new Error("Failed to update client");
    }
  },

  deleteClientById: async (id: string): Promise<void> => {
    const result = await api.delete<void>(`/clients/${id}`);
    if (result.status !== 204) {
      throw new Error("Failed to delete client");
    }
  },
};

export default clientApi;
```

### 2. Toast Notifications

Every feature **MUST** show toast notifications for:

- ✅ **Success** - When an operation completes successfully
- ❌ **Errors** - When an operation fails

**Create a feature-specific toast hook**: `use-{feature}-toast.ts`

**Example**:

```typescript
// use-client-toast.ts
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import {
  parseApiError,
  ErrorCodes,
  type ParsedApiError,
} from "@/lib/api/error-types";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

export function useClientToast() {
  const { t } = useTranslation();

  const success = (operation: "added" | "updated" | "deleted") => {
    const titleKey = i18nKeyContainer.toast.client[operation];
    const descKey = i18nKeyContainer.toast.client[`${operation}Desc`];

    toast.success(t(titleKey), {
      description: t(descKey),
    });
  };

  const error = (apiError: unknown): ParsedApiError => {
    const parsedError = parseApiError(apiError);

    // Map feature-specific error codes to i18n keys
    switch (parsedError.code) {
      case ErrorCodes.CLIENT_NOT_FOUND:
        toast.error(t(i18nKeyContainer.errors.client.notFound), {
          description: t(i18nKeyContainer.errors.client.notFoundDesc),
        });
        break;
      case ErrorCodes.CLIENT_DUPLICATE:
        toast.warning(t(i18nKeyContainer.errors.client.duplicate), {
          description: t(i18nKeyContainer.errors.client.duplicateDesc),
        });
        break;
      default:
        toast.error(t(i18nKeyContainer.errors.generic.title), {
          description: t(i18nKeyContainer.errors.generic.description),
        });
    }

    return parsedError;
  };

  return { success, error };
}
```

---

## Error Handling

### Backend Error Structure

The backend returns errors using **RFC 7807 ProblemDetails** format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Client.ClientNotFound",
  "status": 404,
  "errors": ["Client.ClientNotFound", "Client with id 'xxx' was not found"]
}
```

**Error types and their HTTP status codes**:
| ErrorType | Status Code |
|---------------|-------------|
| Validation | 400 |
| Unauthorized | 401 |
| NotFound | 404 |
| Conflict | 409 |
| Failure | 500 |

### Backend Error Definition

Errors are defined in `Domain/{Feature}/{Feature}Errors.cs`:

```csharp
// ClientErrors.cs
public static class ClientErrors
{
    public static Error ClientNotFound(Guid clientId)
        => Error.NotFound("Client.ClientNotFound",
            $"Client with id '{clientId}' was not found");

    public static Error ClientsNotFound
        => Error.NotFound("Client.ClientsNotFound", "Clients not found");

    public static Error DuplicateClient(string fullName, string phone)
        => Error.Conflict("Client.DuplicateClient",
            $"Client with name '{fullName}' and phone '{phone}' already exists");
}
```

### Frontend Error Mapping

Map backend error codes in `lib/api/error-types.ts`:

```typescript
export const ErrorCodes = {
  // Client errors
  CLIENT_NOT_FOUND: "Client.ClientNotFound",
  CLIENTS_NOT_FOUND: "Client.ClientsNotFound",
  CLIENT_DUPLICATE: "Client.DuplicateClient",

  // Animal errors
  ANIMAL_NOT_FOUND: "Animal.AnimalNotFound",
  ANIMALS_NOT_FOUND: "Animal.AnimalsNotFound",

  // Add new error codes here...
} as const;
```

---

## UI Rules

- **Follow existing UI patterns** - Check similar features before implementing new UI.
- **Keep layouts clean, readable, and consistent**.
- **Avoid complex UI patterns** unless absolutely necessary.
- **Use existing UI components** from `components/ui/` (Button, Dialog, Card, etc.).
- **Use Tailwind CSS** for styling.
- **Use Lucide React** for icons.

### Component Usage

```typescript
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Plus, Edit, Trash } from "lucide-react";
```

---

## RTL Support

The app **MUST** fully support Arabic (RTL layout).

### RTL Detection

```typescript
const { i18n } = useTranslation();
const isRtl = i18n.language === "ar";
```

### Direction Attribute

Always set the `dir` attribute on container elements:

```tsx
<div dir={isRtl ? "rtl" : "ltr"}>{/* Content */}</div>
```

### Logical CSS Properties (REQUIRED)

Use **logical properties** instead of physical (left/right) properties:

| ❌ Physical (Don't use)   | ✅ Logical (Use these)   |
| ------------------------- | ------------------------ |
| `ml-4`, `mr-4`            | `ms-4`, `me-4`           |
| `pl-4`, `pr-4`            | `ps-4`, `pe-4`           |
| `left-0`, `right-0`       | `start-0`, `end-0`       |
| `text-left`, `text-right` | `text-start`, `text-end` |
| `border-l`, `border-r`    | `border-s`, `border-e`   |
| `rounded-l`, `rounded-r`  | `rounded-s`, `rounded-e` |

### RTL-Aware Examples

```tsx
// Button alignment
<Button className="ms-auto gap-2">  {/* Not ml-auto */}
  <Plus className="h-4 w-4" />
  {t(i18nKeyContainer.client.addNewClient)}
</Button>

// Sidebar positioning
<div className={cn(
  "fixed top-0 bottom-0 w-80 border-e",  {/* Not border-r */}
  isRtl ? "end-0" : "start-0"  {/* Not left-0/right-0 */}
)}>

// Spacing
<div className="ps-4 pe-2">  {/* Not pl-4 pr-2 */}
  {/* Content */}
</div>
```

---

## i18n Rules

### Key Container

**ALWAYS** use `i18nKeyContainer` for all text. **NEVER** hardcode strings.

```typescript
import { useTranslation } from "react-i18next";
import i18nKeyContainer from "@/lib/i18n/keyContainer";

const { t } = useTranslation();

// ✅ Correct
<h1>{t(i18nKeyContainer.client.title)}</h1>

// ❌ Wrong - Never hardcode
<h1>Clients</h1>
<h1>{t("client.title")}</h1>  // Don't use string literals
```

### Adding New Translation Keys

When adding new text, you **MUST**:

1. **Add key to `keyContainer.ts`**:

```typescript
const i18nKeyContainer = {
  // ...existing keys
  newFeature: {
    title: "newFeature.title",
    description: "newFeature.description",
  },
};
```

2. **Add translations to ALL locale files**:

`locales/en/en.json`:

```json
{
  "newFeature": {
    "title": "New Feature",
    "description": "This is a new feature"
  }
}
```

`locales/fr/fr.json`:

```json
{
  "newFeature": {
    "title": "Nouvelle fonctionnalité",
    "description": "C'est une nouvelle fonctionnalité"
  }
}
```

`locales/ar/ar.json`:

```json
{
  "newFeature": {
    "title": "ميزة جديدة",
    "description": "هذه ميزة جديدة"
  }
}
```

### Key Naming Convention

Use dot notation for nested keys:

- `feature.action` (e.g., `client.addClient`)
- `feature.field.label` (e.g., `client.firstName`)
- `common.action` (e.g., `common.cancel`, `common.save`)
- `errors.feature.errorType` (e.g., `errors.client.notFound`)
- `toast.feature.action` (e.g., `toast.client.added`)

---

## Reusable Components

### Table Components (`components/tables/`)

Use the existing `DataTable` component for all table views:

```typescript
import { DataTable, type DataTableColumn } from "@/components/tables";

const columns: DataTableColumn<Client>[] = [
  {
    accessorKey: "fullName",
    header: t(i18nKeyContainer.client.fullName),
    enableSorting: true,
  },
  {
    accessorKey: "phone",
    header: t(i18nKeyContainer.client.phoneNumber),
  },
  // Actions column is automatically added when onView/onEdit/onDelete are provided
];

<DataTable
  columns={columns}
  queryFn={clientApi.getAllClients}
  queryKey="clients"
  onView={handleView}
  onEdit={handleEdit}
  onDelete={handleDelete}
  enableSearch={true}
/>
```

**Available table features**:

- Server-side pagination
- Server-side sorting
- Search with debounce
- Loading skeletons
- Row actions (view, edit, delete)

### UI Components (`components/ui/`)

| Component             | Usage                             |
| --------------------- | --------------------------------- |
| `Button`              | All buttons and clickable actions |
| `Dialog`              | Modals and popups                 |
| `Card`                | Content containers                |
| `Input`               | Text inputs                       |
| `Select`              | Dropdown selections               |
| `Label`               | Form labels                       |
| `Avatar`              | User/entity images                |
| `Badge`               | Status indicators                 |
| `Separator`           | Visual dividers                   |
| `ConfirmDeleteDialog` | Delete confirmation modals        |

---

## Tooling Rules

- **Package Manager**: Use **pnpm** (not npm or yarn)
- **Before installing dependencies**: Ask for permission first
- **Before running terminal commands**: Ask for permission first

```bash
# ✅ Use pnpm
pnpm install
pnpm add <package>
pnpm dev

# ❌ Don't use npm/yarn
npm install
yarn add
```

---

## System Flow

### Complete Request Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                           FRONTEND                                   │
├─────────────────────────────────────────────────────────────────────┤
│  1. User Action (click button, submit form)                          │
│                           ↓                                          │
│  2. Feature Component calls API function                             │
│     Example: clientApi.addClient(data)                               │
│                           ↓                                          │
│  3. API file sends HTTP request via Axios                            │
│     POST /api/clients { firstName, lastName, phone }                 │
└─────────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────────┐
│                           BACKEND                                    │
├─────────────────────────────────────────────────────────────────────┤
│  4. Endpoint receives request                                        │
│     CreateClient.Endpoint maps to CreateClient.Command               │
│                           ↓                                          │
│  5. Command Handler executes business logic                          │
│     - Validates data                                                 │
│     - Checks for duplicates                                          │
│     - Creates entity                                                 │
│     - Saves to database                                              │
│                           ↓                                          │
│  6. Returns Result<Response> or Result.Failure(Error)                │
│                           ↓                                          │
│  7. Endpoint converts to HTTP response                               │
│     Success: 201 Created with ID                                     │
│     Failure: ProblemDetails with error code                          │
└─────────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────────┐
│                           FRONTEND                                   │
├─────────────────────────────────────────────────────────────────────┤
│  8. API function receives response                                   │
│                           ↓                                          │
│  9. Success: Show success toast, refresh data, close modal           │
│     Error: Parse error, map to i18n key, show error toast            │
│                           ↓                                          │
│  10. UI updates to reflect new state                                 │
└─────────────────────────────────────────────────────────────────────┘
```

### TanStack Query Integration

Use TanStack Query for all data fetching:

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

// Fetching data
const { data, isLoading, error } = useQuery({
  queryKey: ["clients"],
  queryFn: () => clientApi.getAllClients(params),
});

// Mutations
const queryClient = useQueryClient();
const mutation = useMutation({
  mutationFn: clientApi.addClient,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["clients"] });
    toast.success("Client added");
  },
  onError: (error) => {
    clientToast.error(error);
  },
});
```

---

## Communication Rules

> ⚠️ **IMPORTANT**: If anything is unclear, **DO NOT guess**. Always ask for clarification instead of making assumptions.

- When requirements are ambiguous, ask questions.
- When multiple implementation approaches exist, present options.
- When unsure about UI/UX decisions, ask for guidance.
- When backend structure is unclear, ask before proceeding.

---

## Quick Reference Checklist

When implementing a new feature, ensure:

- [ ] Created `{feature}-api.ts` with all API calls
- [ ] Created `use-{feature}-toast.ts` for notifications
- [ ] Added error codes to `error-types.ts`
- [ ] Added all text to `keyContainer.ts`
- [ ] Added translations to `en.json`, `fr.json`, `ar.json`
- [ ] Used logical CSS properties for RTL support
- [ ] Used existing UI components
- [ ] Used `DataTable` for table views
- [ ] Followed flat DTO pattern for responses
- [ ] Kept frontend "dumb" - no business logic
