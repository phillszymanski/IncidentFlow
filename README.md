# IncidentFlow

## Deployment (Quick Start)

1. Start required services (PostgreSQL) and ensure the API connection string is configured in `IncidentFlowAPI/src/IncidentFlow.API/appsettings.Development.json` (or environment variables for your target environment).
2. Run the backend API:
   - `cd IncidentFlowAPI/src/IncidentFlow.API`
   - `dotnet restore`
   - `dotnet run`
3. Run the frontend app:
   - `cd IncidentFlow.Client`
   - `npm install`
   - `npm run dev`
4. Open the client in your browser at `http://localhost:5173` and verify API connectivity.

## Brief Description

IncidentFlow is a full-stack incident management application for tracking operational issues from creation to resolution. It supports role-based permissions, incident timelines/audit logs, comments, dashboard metrics, filtering, assignment workflows, and soft-delete/restore operations.

## Project Structure

- `IncidentFlow.Client/` — React + TypeScript frontend (Vite, Tailwind)
- `IncidentFlowAPI/` — ASP.NET Core backend (CQRS/MediatR, EF Core, PostgreSQL)

## Core Features

- Incident lifecycle management (create, update, status/severity changes, assignment)
- Role/permission-based access controls (User/Manager/Admin)
- Incident activity timeline (audit trail)
- Comments and collaboration on incidents
- Dashboard metrics + charts and server-side filtering/pagination

## IncidentSeed Configuration

Configure startup data seeding under `IncidentFlowAPI/src/IncidentFlow.API/appsettings.Development.json` in the `IncidentSeed` section.

Supported keys:

- `Enabled`: enables/disables startup seeding.
- `SeedAdminUser`: when true, seeds a configured admin account (if missing).
- `AdminEmail`, `AdminUsername`, `AdminPassword`: required when `SeedAdminUser` is true.
- `Users`: list of non-admin/admin users to seed idempotently (matched by email or username).
- `SeedUserId`: preferred owner id used for seeded incidents/logs/comments.
- `NumberOfIncidents`: target non-deleted incident count.
- `IncidentLogsPerIncident`: target log count per incident.
- `Items`: incident templates used to generate seeded incidents.

Notes:

- `Users` entries require `Email`, `Username`, and `Password`; `Role` defaults to `User`.
- Allowed roles in seed config: `Admin`, `Manager`, `Responder`, `User`.
- Existing seeded users are updated for role/full name and are not duplicated.

Example:

```json
{
   "IncidentSeed": {
      "Enabled": true,
      "SeedAdminUser": true,
      "AdminEmail": "admin@test.com",
      "AdminUsername": "admin",
      "AdminPassword": "Admin@12345",
      "Users": [
         {
            "Email": "user1@test.com",
            "Username": "user1",
            "Password": "User@12345",
            "Role": "User",
            "FullName": "User One"
         }
      ],
      "SeedUserId": "11111111-1111-1111-1111-111111111111",
      "NumberOfIncidents": 100,
      "IncidentLogsPerIncident": 8,
      "Items": [
         {
            "Title": "Authentication failures spiking",
            "Description": "Multiple users reported repeated login failures starting around 08:30 UTC.",
            "Severity": "High"
         }
      ]
   }
}
```
