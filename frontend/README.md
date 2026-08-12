# Call Center Platform — Agent Portal Prototype (Angular)

Local-only frontend prototype pairing with `../backend`. Standalone components, signal-based
state, and a typed SignalR client — consistent with the Angular conventions used elsewhere
(signals, standalone APIs).

## Prerequisites

- Node.js 20+
- The backend running at `https://localhost:7181` (see `../backend/README.md`)

## Run it

```bash
cd frontend
npm install
npm start
```

Opens at `http://localhost:4200`. Log in with one of the seeded demo accounts (shown on the
login screen).

## What's here

- `core/services/auth.service.ts` — login, JWT storage, current-agent signal
- `core/services/signalr.service.ts` — wraps `@microsoft/signalr`, exposes incoming events as signals
- `core/interceptors/auth.interceptor.ts` — attaches the JWT to every API call
- `core/guards/auth.guard.ts` — protects the dashboard route
- `features/login` — sign-in screen
- `features/dashboard` — the agent workspace:
  - **Simulate inbound call** panel — stands in for the telephony provider's webhook
  - **Incoming call panel** — the CRM screen-pop, populated the instant SignalR delivers `IncomingCall`
  - **Call controls** — accept/end with required disposition + notes (written back to the mocked CRM)
  - **Call history** — the agent's own call log

## Note on scope

This prototype implements the *agent* side of the MVP end-to-end (login → status → inbound call
→ CRM screen-pop → accept → disposition → history), which is enough to demonstrate the core
architecture live. The Admin/Supervisor portal, live queue dashboard, and reporting UI described
in the System Design and MVP docs are backed by working API endpoints
(`/api/reports/*`, `/api/agents`, `/api/queues`) but don't yet have a dedicated UI — a natural
next slice of the same architecture, called out here rather than silently left out.
