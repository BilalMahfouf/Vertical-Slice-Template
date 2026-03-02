# VetiCloud Landing Page Specification

## 1. Project Overview
- **Project Name**: VetiCloud Landing Page
- **Type**: Static Website (HTML + CSS)
- **Summary**: A production-ready landing page for VetiCloud, a veterinary clinic management SaaS.
- **Target Users**: Veterinary clinic owners and administrators.

## 2. UI/UX Specification

### Layout Structure
- **Header**: Logo (left), Navigation Links (center), CTA Button (right). Sticky on scroll.
- **Hero Section**: Headline, Subheadline, Primary CTA, Secondary CTA, Hero Image/Illustration.
- **Features Section**: 2x2 or 3x2 Grid of feature cards.
- **How It Works**: 3-4 Step process horizontal flow.
- **Pricing**: 2 Cards (Basic, Pro). Toggle for Monthly/Yearly (visual only, prices fixed).
- **Testimonials**: Optional grid of 3 reviews.
- **FAQ**: Accordion list of 4-5 common questions.
- **CTA Section**: High contrast background, strong headline, "Get Started" button.
- **Footer**: Logo, Links columns, Copyright, Social icons.

### Visual Design

#### Color Palette
- **Primary**: Blue `#1E88E5` (Material Blue 600)
- **Primary Dark**: `#1565C0`
- **Secondary**: Slate `#64748B`
- **Background**: White `#FFFFFF`
- **Surface**: Light Gray `#F8FAFC`
- **Text Main**: Slate 900 `#0F172A`
- **Text Muted**: Slate 500 `#64748B`
- **Success**: Green `#16A34A`
- **Border**: Slate 200 `#E2E8F0`

#### Typography
- **Font Family**: System UI stack (Inter, -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif).
- **Headings**: Bold, large (H1: 3.5rem, H2: 2.5rem, H3: 1.5rem).
- **Body**: Regular, 1rem (16px), relaxed line-height (1.6).

#### Spacing & Sizing
- **Border Radius**: `rounded-md` (8px).
- **Shadows**: Soft, subtle (`0 4px 6px -1px rgb(0 0 0 / 0.1)`).
- **Container**: Max-width 1200px, centered.
- **Grid**: CSS Grid and Flexbox.

#### Visual Effects
- **Buttons**: Hover lift effect, transition duration 200ms.
- **Cards**: Hover shadow increase.
- **Smooth Scroll**: `scroll-behavior: smooth`.

### Components
1.  **Button**: Primary (blue bg, white text), Secondary (white bg, blue border), Ghost (text only).
2.  **Feature Card**: Icon (top), Title, Description. Hover: lift + shadow.
3.  **Pricing Card**: Header (Plan Name), Price, Features List, CTA Button. Highlight "1 Month Free".
4.  **Accordion (FAQ)**: Click to expand/collapse.

## 3. Functionality Specification

### Core Features
- **Responsive Design**: Mobile-first, scales to Desktop.
- **RTL Readiness**: Use logical properties (`margin-inline-start`, `padding-inline-end`) to support Arabic in future.
- **Pricing**:
    - Basic: 6000 DZD/mo
    - Pro: 9000 DZD/mo
    - Both: 1 month free trial badge.

### Content & Copy
- **Hero Headline**: "Modern Veterinary Practice Management"
- **Hero Subhead**: "Streamline appointments, visits, and client communication with VetiCloud. Multi-tenant, multi-language, and built for modern clinics."
- **Features to Highlight**:
    - Appointments & Scheduling
    - Visit Management
    - Multi-language Support (EN/FR/AR)
    - Notifications
    - Multi-tenant Architecture
    - Settings & Configuration
- **How It Works**:
    1. Sign Up
    2. Configure Clinic
    3. Start Managing
- **FAQ Items**:
    - "Is there a free trial?"
    - "What languages are supported?"
    - "Can I manage multiple clinics?"
    - "Is my data secure?"

## 4. Acceptance Criteria
- [ ] Single HTML file with embedded CSS.
- [ ] All sections present: Hero, Features, How it works, Pricing, FAQ, CTA, Footer.
- [ ] Pricing displays 6000 DZD and 9000 DZD correctly.
- [ ] "1 Month Free" visible on pricing.
- [ ] Responsive (looks good on mobile and desktop).
- [ ] Semantic HTML (header, main, section, footer).
- [ ] Accessible (contrast, readable fonts).
- [ ] RTL-ready CSS (logical properties).
