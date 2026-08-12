# Call Center Platform — Backend Prototype (.NET 8)

Local-only prototype proving out the architecture described in `04-system-design.md`
(Requirement Analysis, MVP Definition, and other docs are in the parent submission — this
folder is the "What Stays on His PC" prototype, not part of the submitted write-ups).

## What this proves

- A layered .NET Core solution (Domain / Application / Infrastructure / Api) with clean
  seams (`IRoutingEngine`, `ICrmService`, `ICallNotifier`) matching the System Design doc's
  component list, so each seam could be swapped for a real implementation without touching
  the others.
- A working inbound-call → routing → agent screen-pop → CRM write-back loop, using
  **SignalR** for the real-time push the design calls out as the Notification Hub.
- JWT auth + role-based authorization (Agent / Supervisor / Admin).
- EF Core + SQLite persistence with a seeded demo dataset (no manual setup required).
- Swagger UI for exercising the API directly if you want to demo without the Angular app.

## What this intentionally does NOT do

Per the assignment brief and `03-mvp-definition.md`, this prototype does not implement real
telephony (SIP/PSTN), does not call a real CRM, and does not record real audio. Those
integration points are represented by clear interfaces (`ICrmService`, and the "simulate
inbound call" endpoint standing in for a telephony webhook) so the architecture is provably
correct without needing carrier/CRM credentials to run.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run it

```bash
cd backend
dotnet restore
dotnet run --project src/CallCenter.Api
```

The API starts at `https://localhost:7181` (Swagger UI opens automatically). A SQLite file
(`callcenter.db`) is created and seeded automatically on first run.

## Seeded demo accounts

| Email | Password | Role | Queue |
|---|---|---|---|
| rafi.agent@example.com | Password123! | Agent | Support |
| nusrat.agent@example.com | Password123! | Agent | Sales |
| imran.supervisor@example.com | Password123! | Supervisor | — |

## Demo script (matches the Angular frontend)

1. Log in as `rafi.agent@example.com`, set status to **Available**.
2. From another tool (Swagger, curl, or a second browser tab hitting the "Simulate inbound
   call" button in the Angular app), trigger:
   ```
   POST /api/calls/simulate-inbound
   { "fromNumber": "+8801700000001", "toNumber": "+8801900000000", "queueName": "Support" }
   ```
3. Watch the Angular agent dashboard receive the call in real time via SignalR, with the CRM
   screen-pop for "Tanvir Rahman" already populated.
4. Accept the call, then end it with a disposition — the (mocked) CRM write-back is logged
   server-side, and the call appears in call history / reports.

## Solution layout

```
backend/
  src/
    CallCenter.Domain/          # Entities + enums, no dependencies
    CallCenter.Application/     # DTOs, interfaces, business logic (RoutingEngine, CallOrchestrationService, ReportingService)
    CallCenter.Infrastructure/  # EF Core, repositories, mocked CRM client, JWT token service
    CallCenter.Api/             # Controllers, SignalR hub, Program.cs composition root
```
