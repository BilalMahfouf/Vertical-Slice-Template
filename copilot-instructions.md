# Project Map (AI + Dev Guide)

> **VetiCloud** — Veterinary clinic management SaaS. Multi-tenant, multi-language (en/fr/ar+RTL).

## 1. What This Repo Is

A full-stack veterinary practice management system: clients, animals, appointments, visits, notifications, settings. Supports multi-clinic tenancy with role-based access.

**Non-goals:**
- No billing/payments module
- No inventory/pharmacy management
- No public-facing client portal
- No mobile apps (web only)

---

## 2. Tech Stack

| Layer | Technology | Version | Notes |
|-------|------------|---------|-------|
| **Frontend** | React | 19.2 | Feature-based architecture |
| | TypeScript | 5.9 | Strict mode |
| | Vite | 7.2 | Build + dev server |
| | react-router-dom | 7.11 | Client routing |
| | TanStack Query | 5.90 | Server state + caching |
| | TanStack Table | 8.21 | Headless tables |
| | react-hook-form + Zod | 7.69 / 4.3 | Forms + validation |
| | Axios | 1.13 | HTTP client |
| | Tailwind CSS | 4.1 | Utility-first styling |
| | shadcn/ui (Radix) | — | Component primitives |
| | Lucide React | 0.562 | Icons |
| | i18next | 25.7 | i18n (en/fr/ar) |
| | Sonner | 2.0 | Toast notifications |
| **Backend** | .NET | 10.0 | Minimal APIs |
| | Carter | 10.0 | Endpoint routing |
| | EF Core + Npgsql | 10.0 | PostgreSQL ORM |
| | FluentValidation | 12.1 | Request validation |
| | JWT Bearer | 10.0 | Auth tokens |
| | Argon2 | 2.0 | Password hashing |
| | MailKit | 4.14 | Email sending |
| **Testing** | xUnit + Moq | 2.9 / 4.20 | Backend unit tests |
| **Tooling** | pnpm | — | Package manager (required) |
| | ESLint + Prettier | 9.39 / 3.7 | Lint + format |
| | Docker Compose | — | Local dev environment |

---

## 3. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         BROWSER (React SPA)                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────────┐ │
│  │ Features │──│ API Layer│──│ TanStack │──│ Components (shadcn) │ │
│  │ (pages)  │  │ (axios)  │  │  Query   │  │ + Layouts            │ │
│  └──────────┘  └────┬─────┘  └──────────┘  └──────────────────────┘ │
└─────────────────────┼───────────────────────────────────────────────┘
                      │ HTTPS (JWT Bearer)
┌─────────────────────▼───────────────────────────────────────────────┐
│                      BACKEND (.NET 10 Minimal API)                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────────┐ │
│  │ Endpoints│──│ CQRS     │──│ Domain   │──│ Infrastructure       │ │
│  │ (Carter) │  │ Handlers │  │ Entities │  │ (EF Core, Auth, Mail)│ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┬───────────┘ │
└─────────────────────────────────────────────────────────┼───────────┘
                                                          │
                                              ┌───────────▼───────────┐
                                              │   PostgreSQL (npgsql) │
                                              └───────────────────────┘
```

**Business logic**: Backend only. Frontend is "dumb" (display + HTTP calls).

**Domains**: Auth, Clinics, Users, Clients, Animals, Appointments, Visits, Notifications, Settings.

---

## 4. Directory & File Map

### Frontend Tree
```
frontend/src/
├── features/{feature}/     # Feature modules
│   ├── {Feature}Page.tsx   # Route page
│   ├── {feature}-api.ts    # All API calls
│   ├── {feature}-table.tsx # DataTable config
│   ├── add-{feature}.tsx   # Create/Edit dialog
│   ├── view-{feature}.tsx  # View dialog
│   ├── use-{feature}-toast.ts
│   └── use-delete-{feature}.ts
├── components/
│   ├── ui/                 # shadcn/ui primitives
│   └── tables/             # DataTable system
├── lib/
│   ├── api/                # Axios instance + error types
│   ├── i18n/               # i18next + locales
│   └── utils.ts            # cn(), getTableRequestParams()
├── common/layouts/         # MainLayout, Sidebar, TopNav
└── hooks/                  # Shared hooks
```

### Backend Tree
```
backend/Veterinary/src/VeterinaryApi/
├── Features/{Feature}/
│   ├── Create{Feature}.cs  # Command + Handler + Endpoint
│   ├── GetAll{Feature}.cs  # Query + Handler + Endpoint
│   ├── Get{Feature}ById.cs
│   ├── Update{Feature}.cs
│   ├── Delete{Feature}.cs
│   └── Common.cs           # Shared DTOs
├── Domain/{Feature}/
│   ├── {Entity}.cs
│   └── {Entity}Errors.cs
├── Common/                 # CQRS, Results, Errors, Pagination
├── Infrastructure/         # Persistence, Auth, Services
└── Migrations/
```

### Task → File Map

| Task | Files to Touch |
|------|----------------|
| **Add new page/route** | `features/{feature}/{Feature}Page.tsx`, `App.tsx` (add route) |
| **Add new API call** | `features/{feature}/{feature}-api.ts` |
| **Add reusable component** | `components/ui/{component}.tsx` |
| **Add feature-specific component** | `features/{feature}/{component}.tsx` |
| **Add form with validation** | `features/{feature}/add-{feature}.tsx` (Zod schema + react-hook-form) |
| **Add translation key** | `lib/i18n/keyContainer.ts` + `locales/{en,fr,ar}/{lang}.json` |
| **Add error code** | `lib/api/error-types.ts` (frontend) + `Domain/{Feature}/{Feature}Errors.cs` (backend) |
| **Add table column** | `features/{feature}/{feature}-table.tsx` |
| **Add toast notification** | `features/{feature}/use-{feature}-toast.ts` |
| **Add backend endpoint** | `Features/{Feature}/{Action}{Feature}.cs` |
| **Add domain entity** | `Domain/{Feature}/{Entity}.cs` + migration |
| **Add theme token** | `index.css` (CSS variables) |

---

## 5. Frontend Code Structure (React + TS)

### Component Patterns
- **Feature-based**: Each feature is self-contained in `features/{name}/`
- **Page components**: PascalCase (`ClientPage.tsx`)
- **Other components**: kebab-case (`add-client.tsx`, `client-table.tsx`)
- **No barrel exports** except `components/tables/index.ts`

### Type Definitions
```typescript
// Define types in {feature}-api.ts
export type Client = {
  id: string;
  fullName: string;
  // ... flat structure, no nested records
};

export type CreateClientRequest = {
  firstName: string;
  lastName: string;
};
```

### Hook Patterns
- **Query hooks**: Use `useQuery` directly or `useTableQuery` for tables
- **Mutation hooks**: Separate file `use-delete-{feature}.ts` or inline
- **Toast hooks**: `use-{feature}-toast.ts` per feature
- **Shared hooks**: `hooks/` folder

### Utilities
```typescript
import { cn } from "@/lib/utils";           // clsx + tailwind-merge
import { getTableRequestParams } from "@/lib/utils"; // pagination params
```

### CSS / Styling
- Tailwind utility classes only
- `cn()` for conditional classes
- **Logical properties for RTL**: `ms-`, `me-`, `ps-`, `pe-`, `start-`, `end-`
- CSS variables in `index.css` for theme tokens

### States Pattern
| State | Implementation |
|-------|----------------|
| Loading | `<Skeleton />` or `isLoading` from useQuery |
| Error | Toast via `use-{feature}-toast.ts` |
| Empty | Inline empty state in component |
| Success | Toast + cache invalidation |

### Accessibility
- All interactive elements keyboard-accessible (Radix handles this)
- Use semantic HTML
- ARIA labels for icon-only buttons
- Focus visible states via Tailwind `focus-visible:`

---

## 6. Data & Request Flow

### Lifecycle
```
User Action → Component → API call (axios) → TanStack Query cache
     ↑                                              ↓
     └──────────── Re-render with new data ─────────┘
```

### API Configuration
```typescript
// lib/api/api.ts
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL + "/api",
  timeout: 10000,
  withCredentials: true, // for refresh token cookie
});
```

### Auth Token Flow
1. Login → Access token stored in memory (`tokenManager.ts`)
2. Refresh token in httpOnly cookie
3. Request interceptor attaches `Authorization: Bearer {token}`
4. 401 response → Auto-refresh via `/auth/refresh` → Retry original request
5. Refresh fails → Redirect to `/login`

### Query Pattern
```typescript
const { data, isLoading } = useQuery({
  queryKey: ["clients", params],
  queryFn: () => clientApi.getAllClients(params),
});
```

### Mutation Pattern
```typescript
const queryClient = useQueryClient();
const mutation = useMutation({
  mutationFn: clientApi.addClient,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["clients"] });
    clientToast.success("added");
    onClose();
  },
  onError: (err) => clientToast.error(err),
});
```

### Error Handling
1. Backend returns RFC 7807 ProblemDetails: `{ title: "Client.NotFound", status: 404, errors: [...] }`
2. Frontend `parseApiError()` extracts error code
3. Feature toast hook maps code to i18n key
4. Display localized toast message

---

## 7. UI Identity (Design System)

### Visual Principles
| Property | Value |
|----------|-------|
| Border radius | `rounded-md` (6px default) |
| Spacing scale | Tailwind default (4px base) |
| Typography | System font stack via shadcn |
| Shadows | Minimal, `shadow-sm` for elevation |

### Color Tokens (CSS Variables)
```css
--primary: 217 91% 51%;        /* #1E88E5 Material Blue 600 */
--primary-foreground: 0 0% 100%;
--secondary: 215 16% 47%;      /* Slate */
--destructive: 0 84% 60%;      /* Red */
--success: 142 76% 36%;        /* Green */
--warning: 45 93% 47%;         /* Amber */
--background: 0 0% 100%;
--foreground: 222 47% 11%;
--muted: 210 40% 96%;
--border: 214 32% 91%;
```

### Component Rules
| Do | Don't |
|----|-------|
| Use shadcn/ui components | Create custom primitives |
| Use `Button`, `Dialog`, `Card` | Use raw HTML buttons/modals |
| Use `Lucide` icons | Use other icon libraries |
| Use `Sonner` for toasts | Use alert() or custom toasts |

### Layout Rules
- Sidebar: Fixed, 256px wide, collapsible on mobile
- Main content: Scrollable, max-width container
- Breakpoints: Tailwind defaults (`sm:640px`, `md:768px`, `lg:1024px`)

### UX Patterns
| Pattern | Implementation |
|---------|----------------|
| Modals | `<Dialog>` for forms and views |
| Confirmations | `<ConfirmDeleteDialog>` |
| Loading | Skeleton loaders in tables |
| Notifications | Sonner toasts (top-right, RTL: top-left) |
| Empty states | Inline with icon + message |

### RTL Support (Required)
```typescript
const { i18n } = useTranslation();
const isRtl = i18n.language === "ar";
// Set dir="rtl" on containers
// Use logical properties: ms-, me-, ps-, pe-, start-, end-
```

### UI PR Checklist
- [ ] Uses existing shadcn/ui components
- [ ] RTL-compatible (logical CSS properties)
- [ ] All text via `t(i18nKeyContainer.x.y)`
- [ ] Loading/error/empty states handled
- [ ] Keyboard accessible
- [ ] Mobile responsive

---

## 8. Common Practices & Standards

### TypeScript
- Strict mode enabled
- No `any` except rare cases
- Prefer `type` over `interface` for DTOs
- Export types from API files

### Lint/Format
```bash
pnpm lint          # ESLint check
pnpm lint:fix      # ESLint fix
pnpm format        # Prettier
```

### Testing
| Layer | Framework | Location |
|-------|-----------|----------|
| Backend unit | xUnit + Moq | `Tests/Application.Tests/` |
| Frontend | — | Not configured |

### Logging
- Backend: Use `ILogger<T>`
- Frontend: `console.error` for caught errors only

### Security
- Never log tokens or passwords
- Sanitize user input on backend
- Access token in memory only (not localStorage)
- Refresh token httpOnly cookie
- HTTPS required

### Performance
- Use `React.memo` sparingly (only for expensive renders)
- TanStack Query handles caching
- Lazy load routes if needed
- Optimize images (not currently applicable)

### Read-Only Files (Do Not Modify)
- `*.config.ts` / `*.config.js` — Build/tool configs
- `tsconfig*.json` — TypeScript configs
- `eslint.config.*` — Linter configs
- `vite.config.ts` — Vite config
- `docker-compose*.yml` — Docker configs
- `*.csproj` — .NET project files
- `appsettings*.json` — Backend configs

---

## 9. Feature Development Playbook

### New Frontend Feature
1. Create folder: `features/{feature}/`
2. Create API file: `{feature}-api.ts` with types + API object
3. Create page: `{Feature}Page.tsx`
4. Add route in `App.tsx`
5. Create table: `{feature}-table.tsx` using `DataTable`
6. Create forms: `add-{feature}.tsx` with Zod + react-hook-form
7. Create toast hook: `use-{feature}-toast.ts`
8. Add translations: `keyContainer.ts` + all locale files
9. Add error codes: `lib/api/error-types.ts`

### New Backend Feature
1. Create entity: `Domain/{Feature}/{Entity}.cs`
2. Create errors: `Domain/{Feature}/{Entity}Errors.cs`
3. Add DbSet to `AppDbContext`
4. Create migration: `dotnet ef migrations add Add{Entity}`
5. Create CQRS files in `Features/{Feature}/`:
   - `Create{Entity}.cs`
   - `GetAll{Entity}.cs`
   - `Get{Entity}ById.cs`
   - `Update{Entity}.cs`
   - `Delete{Entity}.cs`
6. Register endpoints (auto via Carter)

---

## 10. Quick Commands

```bash
# Frontend
cd frontend
pnpm install          # Install dependencies
pnpm dev              # Start dev server (Vite)
pnpm build            # Production build
pnpm lint             # Run ESLint
pnpm format           # Run Prettier

# Backend
cd backend/Veterinary/src/VeterinaryApi
dotnet restore        # Restore packages
dotnet run            # Run API
dotnet watch run      # Run with hot reload
dotnet ef migrations add <Name>   # Add migration
dotnet ef database update         # Apply migrations

# Docker (from backend/Veterinary)
docker-compose up -d  # Start all services

# Tests
cd backend/Veterinary/Tests/Application.Tests
dotnet test           # Run unit tests
```

---

## 11. "When You Are Unsure" Rules

### AI Behavior Contract

1. **Prefer this document** over scanning the repo
2. **Only scan if**: info not in this doc or `copilot-instructions.md`
3. **If scanning**, state exactly which files and why
4. **Never invent**:
   - API endpoints (check `{feature}-api.ts`)
   - Types (check API files)
   - CSS tokens (check `index.css`)
   - i18n keys (check `keyContainer.ts`)
   - Error codes (check `error-types.ts`)
5. **Ask one focused question** when blocked
6. **Follow patterns exactly**: copy structure from existing features
7. **Check existing components first** before creating new ones
8. **Use pnpm**, never npm or yarn
9. **Use logical CSS properties** for RTL support
10. **All user-facing text** must use `t(i18nKeyContainer.x.y)`

### Files to Inspect by Task

| Unsure About | Inspect |
|--------------|---------|
| API structure | `features/clients/client-api.ts` |
| Table setup | `features/clients/client-table.tsx` |
| Form pattern | `features/clients/add-client.tsx` |
| Toast pattern | `features/clients/use-client-toast.ts` |
| Error handling | `lib/api/error-types.ts` |
| i18n keys | `lib/i18n/keyContainer.ts` |
| Theme tokens | `index.css` |
| Layout | `common/layouts/MainLayout.tsx` |
| Backend CQRS | `Features/Clients/CreateClient.cs` |
| Domain errors | `Domain/Clients/ClientErrors.cs` |

### Do Not Touch
- Config files (`*.config.*`, `tsconfig.*`, `docker-compose.*`)
- Project files (`*.csproj`, `package.json` dependencies)
- Environment files (`.env*`, `appsettings.*`)
