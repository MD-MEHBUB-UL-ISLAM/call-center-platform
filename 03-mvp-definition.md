# MVP Definition — v1 Scope

## Guiding Principle

The MVP has to prove the platform can **replace the third-party tool for core voice operations**
at 50 agents, on solid architectural footing that doesn't need to be thrown away at 500 agents.
Everything that's about *convenience* or *future capability* (AI, omni-channel, advanced analytics)
is cut from v1.

## In Scope for v1

| Feature | Why it's in v1 |
|---|---|
| Agent login + status (Available/Busy/Break/Offline) | Core to any call routing decision |
| Inbound call handling with basic queueing | This is the primary use case being replaced |
| Outbound click-to-call from CRM + manual dial | Second primary use case |
| Basic IVR (menu → route to queue) | Needed for any real inbound flow, even a simple one |
| Skill/queue-based routing (not just round robin) | Realistic minimum for a call center, not a toy |
| Hold, transfer (warm/cold) | Table-stakes agent functionality |
| Call recording (audio + metadata stored) | Compliance/QA almost always required from day one |
| CRM screen-pop on inbound call | This is a key reason to go in-house — must be in v1 |
| CRM write-back (call outcome, duration, notes) | Same as above — proves the CRM integration works |
| Supervisor live dashboard (queue depth, agent status) | Minimum supervisors need to run a shift |
| Basic historical reporting (volume, wait time, agent talk time) | Needed to prove parity with old tool |
| RBAC (Agent / Supervisor / Admin roles) | Security baseline, not optional |
| Centralized logging/monitoring for the call path | Can't operate a call center blind |

## Explicitly Cut from v1 (and why)

| Feature | Why it's deferred |
|---|---|
| AI call summaries / transcription / smart routing | Explicitly a "later" feature per the brief; only architectural readiness is in scope now |
| Listen-in / whisper / barge for supervisors | High value but not required to *replace* the current tool functionally; adds real-time complexity |
| Omni-channel (chat/email/social) | Brief is voice-only; scope creep risk |
| Native mobile agent app | Browser-based agent portal covers the office-based agent use case |
| Multi-region / multi-tenant support | Only one company, one region today |
| Advanced outbound campaign management (predictive dialers, dialing lists automation) | Manual + click-to-call outbound is enough to prove the core loop; campaign tooling is a v2 differentiator |
| Self-serve IVR config UI (drag-and-drop builder) | v1 can use structured config (JSON/DB-driven) edited by admins/devs; a visual builder is a UX investment for later |
| Full DR/multi-region failover | v1 needs backups and a documented recovery plan, not automatic multi-region failover |

## Scoping Rationale (Value / Effort / Risk)

- **Inbound + outbound + CRM sync** are the three things that directly replace the third-party
  tool's job — cutting any of them means the MVP doesn't actually prove the platform works.
- **Recording and basic reporting** are cheap relative to their value (compliance risk if missing,
  and leadership will ask for these numbers on day one).
- **Supervisor barge/whisper and AI features** are high effort, real-time-heavy, and not required
  to prove the core hypothesis — they're natural v1.1/v2 additions once the base platform is stable.
- **Telephony build-vs-buy**: MVP assumes buying into a proven SIP trunk/CPaaS layer rather than
  building carrier-grade telephony in-house. This is the highest-risk, highest-effort item in the
  whole project, and building it ourselves for v1 would blow the timeline for no MVP-stage benefit.

## MVP Success Criteria

1. An agent can take an inbound call, see the customer's CRM record pop up, and log an outcome
   that saves back to the CRM.
2. An agent can place an outbound call from a CRM record.
3. A supervisor can see live queue and agent status.
4. Calls are recorded and retrievable.
5. The system holds up under a simulated 50-agent load without falling over.
