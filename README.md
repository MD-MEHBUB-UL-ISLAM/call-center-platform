# Call Center Platform

A prototype in-house call center voice platform — built to replace a third-party call center
tool with inbound/outbound calling, real-time agent notifications, and CRM screen-pop, backed by
a clean, layered .NET architecture.

**Live demo:** https://call-center-platform-xi.vercel.app/login
**Design docs:** [`docs/`](./docs) — requirement analysis, MVP scope, system design, scalability
plan, AI-readiness notes, and deployment strategy

---

## What this is

This started as a take-home system design exercise: design an in-house call center platform on
paper, then bring a working prototype to prove the design holds up. This repo is that prototype —
a real, running system demonstrating the core loop end to end:

**Agent logs in → sets status → an inbound call is routed to them → a CRM screen-pop appears in
real time → they accept, handle, and log the outcome → it's written back and shows up in call
history.**

The seven design documents in [`docs/`](./docs) cover the reasoning (requirements, MVP scope,
architecture, scaling from 50 to 500+ agents, AI-readiness, and deployment); this codebase is
the proof of execution behind them.

## Architecture

```
┌─────────────────────┐        ┌──────────────────────────────────────────┐
│   Agent Portal       │  HTTPS │              .NET 10 API                  │
│   (Angular, Vercel)  │◄──────►│  Controllers → Application → Infra layer  │
│                       │  WSS   │  JWT auth · SignalR hub · EF Core         │
└─────────────────────┘        └──────────────────┬─────────────────────────┘
                                                    │
                                          ┌─────────┴─────────┐
                                          │  SQLite (dev) /    │
                                          │  Postgres (prod)   │
                                          └────────────────────┘
```

| Layer | Responsibility |
|---|---|
| `CallCenter.Domain` | Entities and enums — no framework dependencies |
| `CallCenter.Application` | Business logic: routing engine, call orchestration, reporting — depends only on interfaces |
| `CallCenter.Infrastructure` | EF Core persistence, JWT issuing, mocked CRM client |
| `CallCenter.Api` | Controllers, SignalR hub, composition root (`Program.cs`) |

Real telephony (SIP/PSTN) and a live CRM connection are intentionally **out of scope** for this
prototype — see [`docs/04-system-design.md`](./docs/04-system-design.md) for why, and
[`ICrmService`](./backend/src/CallCenter.Application/Interfaces/ICrmService.cs) /
[`MockCrmService`](./backend/src/CallCenter.Infrastructure/ExternalServices/MockCrmService.cs)
for where a real integration would plug in without touching the rest of the system.

## Repo structure

```
call-center-platform-monorepo/
├── docs/           System design write-ups (Markdown, no code)
├── backend/        .NET 10 Web API — see backend/README.md
├── frontend/        Angular 18 Agent Portal — see frontend/README.md
└── DEPLOY.md       Step-by-step Vercel + Railway deployment guide
```

## Tech stack

**Backend** — .NET 10 · ASP.NET Core Web API · Entity Framework Core (SQLite / Postgres) ·
SignalR · JWT Bearer auth · Swagger

**Frontend** — Angular 18 (standalone components, signals) · `@microsoft/signalr` · RxJS

## Features (MVP scope)

- Agent login with JWT auth and role-based access (Agent / Supervisor / Admin)
- Agent status management (Available / Busy / On Break / Offline)
- Inbound call simulation → skill/queue-based routing → real-time delivery to an available agent
- CRM screen-pop on incoming calls (mocked CRM, matched by phone number)
- Outbound click-to-call
- Call disposition + notes, written back to the (mocked) CRM
- Per-agent call history
- Reporting endpoints for call volume and agent productivity

Full in/out-of-scope reasoning: [`docs/03-mvp-definition.md`](./docs/03-mvp-definition.md).

## Getting started locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), Node.js 20+

```bash
# Terminal 1 — backend
cd backend
dotnet restore
dotnet run --project src/CallCenter.Api
# → https://localhost:7181 (Swagger UI in Development)

# Terminal 2 — frontend
cd frontend
npm install
npm start
# → http://localhost:4200
```

### Seeded demo accounts

| Email | Password | Role |
|---|---|---|
| rafi.agent@example.com | Password123! | Agent (Support queue) |
| nusrat.agent@example.com | Password123! | Agent (Sales queue) |
| imran.supervisor@example.com | Password123! | Supervisor |

Try simulating an inbound call from `+8801700000001` for a live CRM screen-pop match.

## Deployment

Deployed as two independently-hosted services from this one repo:

- **Frontend** → [Vercel](https://vercel.com) (root directory: `frontend`)
- **Backend** → [Railway](https://railway.app) (root directory: `backend`, deployed via Docker)

Full step-by-step instructions, required environment variables, and CORS setup: **[DEPLOY.md](./DEPLOY.md)**.

## Known limitations

- No real SIP/PSTN telephony or live CRM connection — both are mocked, by design (see system design docs)
- SQLite on Railway's free tier is ephemeral; data resets on redeploy unless a volume or Postgres is configured
- Only the Agent Portal has a UI; Admin/Supervisor screens are backed by working API endpoints (`/api/reports/*`, `/api/agents`, `/api/queues`) without a dedicated frontend yet

## License

Private prototype — not licensed for reuse.
