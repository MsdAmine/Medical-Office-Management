# Medical Office Management

## Project Overview
- Internal web application for médecins, secrétaires, administrators, and patients to manage day-to-day operations in a medical practice.
- Centralizes patient information with controlled access that follows defined actor roles.

## Core Features
- **Dashboard & KPIs**: Operational cards and summary metrics for quick status checks.
- **Patients**: Patient list views with filtering, detail pages, and action shortcuts for related tasks.
- **Role-Based Access**: Authorization aligned to Admin, Secrétaire, Médecin, and Patient roles for all patient workflows.

## Technical Stack
- **Backend**: ASP.NET Core MVC.
- **Frontend**: Razor views styled with Tailwind CSS (non-Bootstrap) for a clean, calm medical aesthetic.
- **Architecture**: Clear separation via Controllers, ViewModels, and Views to keep UI, logic, and data concerns isolated and maintainable.

## Design & UX Approach
- Built for internal productivity with data-rich layouts and direct action pathways.
- Card-based, table-driven UI with reusable components to keep pages consistent across modules.
- Calm, medical-oriented styling without marketing embellishment; focuses on clarity and readability.

## Current State
- Implemented UI for dashboard and patient management with role-based protections.
- Mock and static data used where appropriate; ready to attach to live data sources focused on patient workflows.

## Future Improvements
- Integrate Entity Framework Core for persistent storage.
- Expand patient workflow depth while maintaining strict role controls.
