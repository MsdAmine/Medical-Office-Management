# Medical Office Management

## Project Overview
- Internal web application for clinic staff, physicians, and administrators to manage day-to-day operations in a medical practice.
- Centralizes patient, appointment, billing, and reporting workflows to reduce manual coordination and provide at-a-glance operational insight.

## Core Features
- **Dashboard & KPIs**: Operational cards and summary metrics for quick status checks.
- **Patients**: Patient list views with filtering, detail pages, and action shortcuts for related tasks.
- **Appointments**: Scheduling flows with availability management and status tracking.
- **Doctors**: Provider roster with profile details, specialties, and scheduling context.
- **Medical Records**: Access to visit summaries and medical records in a consistent, readable layout.
- **Billing & Invoicing**: Billing overview, invoice visibility, and payment status monitoring.
- **Reports & Analytics**: Data-driven reports with configurable filters for operational insights.
- **Settings**: Administrative preferences for clinic configuration and user-facing defaults.

## Technical Stack
- **Backend**: ASP.NET Core MVC.
- **Frontend**: Razor views styled with Tailwind CSS (non-Bootstrap) for a clean, calm medical aesthetic.
- **Architecture**: Clear separation via Controllers, ViewModels, and Views to keep UI, logic, and data concerns isolated and maintainable.

## Design & UX Approach
- Built for internal productivity with data-rich layouts and direct action pathways.
- Card-based, table-driven UI with reusable components to keep pages consistent across modules.
- Calm, medical-oriented styling without marketing embellishment; focuses on clarity and readability.

## Current State
- Fully implemented UI for dashboard, patients, appointments, doctors, medical records, billing, reports, and settings.
- Mock and static data used where appropriate; ready to attach to live data sources.

## Future Improvements
- Integrate Entity Framework Core for persistent storage.
- Add role-based access controls and full CRUD workflows.
- Enable real-time updates for schedules, billing changes, and alerts.
- Expand reporting depth and export options.
