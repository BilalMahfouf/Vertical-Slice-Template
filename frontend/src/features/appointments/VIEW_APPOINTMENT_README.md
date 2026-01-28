# ViewAppointment Component

A read-only dialog component for displaying comprehensive appointment details in the Veterinary Application.

## Features

✅ **Read-only display** - No editing, only viewing appointment information  
✅ **Fully responsive** - Works seamlessly on phones, tablets, and desktops  
✅ **RTL support** - Proper layout and alignment for Arabic (RTL) languages  
✅ **Scrollable content** - Handles long forms with smooth scrolling  
✅ **ShadCN UI** - Uses consistent UI components from the design system  
✅ **Multilingual** - Full i18n support with translation keys  
✅ **Loading states** - Skeleton loaders for better UX  
✅ **Status badges** - Color-coded appointment status indicators

## Component API

### Props

```typescript
interface ViewAppointmentProps {
  open: boolean;          // Controls dialog visibility
  onClose: () => void;    // Callback when dialog is closed
  appointmentId: string;  // ID of the appointment to display
}
```

## Usage Example

### Basic Usage

```tsx
import { useState } from "react";
import ViewAppointment from "./view-appointment";

function MyComponent() {
  const [viewOpen, setViewOpen] = useState(false);
  const [appointmentId, setAppointmentId] = useState<string>("");

  const handleViewAppointment = (id: string) => {
    setAppointmentId(id);
    setViewOpen(true);
  };

  return (
    <>
      <button onClick={() => handleViewAppointment("some-id")}>
        View Appointment
      </button>

      <ViewAppointment
        open={viewOpen}
        onClose={() => setViewOpen(false)}
        appointmentId={appointmentId}
      />
    </>
  );
}
```

### Integration with AppointmentPage

Add a "View" action to the appointment data table:

```tsx
// In AppointmentPage.tsx

import ViewAppointment from "./view-appointment";
import { Eye } from "lucide-react";

export default function AppointmentPage() {
  // ... existing state
  const [viewAppointmentOpen, setViewAppointmentOpen] = useState(false);
  const [appointmentToView, setAppointmentToView] = useState<Appointment | null>(null);

  const handleView = (appointment: Appointment) => {
    setAppointmentToView(appointment);
    setViewAppointmentOpen(true);
  };

  return (
    <div>
      {/* Pass handleView to the data table */}
      <AppointmentDataTable
        onView={handleView}  // Add this prop
        onReschedule={handleReschedule}
        onCancel={handleCancel}
        onDelete={handleDelete}
      />

      {/* Add ViewAppointment dialog */}
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
    </div>
  );
}
```

### Update AppointmentDataTable to Support View Action

```tsx
// In appointment-data-table.tsx

interface AppointmentDataTableProps {
  onView?: (appointment: Appointment) => void;  // Add this
  onReschedule?: (appointment: Appointment) => void;
  onCancel?: (appointment: Appointment) => void;
  onDelete?: (appointment: Appointment) => void;
}

export default function AppointmentDataTable({
  onView,  // Add this
  onReschedule,
  onCancel,
  onDelete,
}: AppointmentDataTableProps) {
  // ... in the actions column cell:
  
  cell: ({ row }) => {
    const actions: RowAction<Appointment>[] = [];

    // Add View action first
    if (onView) {
      actions.push({
        label: t(i18nKeyContainer.table.viewDetails),
        onClick: () => onView(row.original),
        icon: Eye,
      });
    }

    // ... rest of the actions (reschedule, cancel, delete)
  }
}
```

## Displayed Information

The component displays the following appointment details:

1. **Status** - Color-coded badge showing appointment state:
   - ✅ Confirmed (green)
   - ❌ Cancelled (red)
   - ✔️ Completed (blue)
   - 🔄 Rescheduled (amber)

2. **Date** - Localized appointment date
3. **Time** - Localized appointment time
4. **Client Name** - Name of the pet owner
5. **Animal Name** - Name of the patient
6. **Created On** - When the appointment was created

## Responsive Behavior

### Mobile (< 640px)
- Single column layout
- Full-width elements
- Stacked date/time fields
- Scrollable content area

### Tablet (640px - 1024px)
- Two-column grid for date/time
- Optimized spacing
- Adaptive dialog width

### Desktop (> 1024px)
- Maximum width: 672px (2xl)
- Two-column grid layout
- Full feature set

## RTL Support

The component automatically adapts to RTL languages (Arabic):

- Text direction changes to right-to-left
- Icons and spacing flip appropriately
- Dialog alignment adjusts
- All UI elements maintain proper visual hierarchy

## Styling

The component follows the app's design system:

- **Colors**: Slate palette for backgrounds and text
- **Spacing**: Consistent padding and gaps (Tailwind spacing scale)
- **Typography**: Proper font weights and sizes
- **Borders**: Subtle borders using slate-200
- **Shadows**: Minimal, following ShadCN defaults

## Accessibility

- Keyboard navigation supported
- Proper ARIA labels
- Focus management
- Screen reader friendly

## Translation Keys Used

All labels use i18n keys from `keyContainer`:

```typescript
// In keyContainer.ts
appointment: {
  viewTitle: 'appointment.viewTitle',
  viewDescription: 'appointment.viewDescription',
  status: 'appointment.status',
  statusConfirmed: 'appointment.statusConfirmed',
  statusCancelled: 'appointment.statusCancelled',
  statusCompleted: 'appointment.statusCompleted',
  statusRescheduled: 'appointment.statusRescheduled',
  date: 'appointment.date',
  time: 'appointment.time',
  client: 'appointment.client',
  animal: 'appointment.animal',
  createdOn: 'appointment.createdOn',
}
```

## Dependencies

All required dependencies are already installed in the project:

- `@tanstack/react-query` - Data fetching
- `react-i18next` - Internationalization
- `lucide-react` - Icons
- ShadCN UI components (Dialog, Button)

## Notes

- The component fetches appointment data automatically when opened
- Data is cached by React Query for performance
- The dialog prevents closing while loading
- All dates/times are formatted according to the current locale

## Common Issues

**Issue**: Dialog doesn't open  
**Solution**: Ensure `appointmentId` is valid and `open` prop is `true`

**Issue**: Translation keys not found  
**Solution**: Verify all keys exist in `en.json`, `ar.json`, and `fr.json`

**Issue**: Data not loading  
**Solution**: Check API endpoint and ensure `appointmentApi.getAppointmentById` works

## Example: Complete Integration

See [AppointmentPage.tsx](./AppointmentPage.tsx) for a complete example of how to integrate this component with the appointment management page.
