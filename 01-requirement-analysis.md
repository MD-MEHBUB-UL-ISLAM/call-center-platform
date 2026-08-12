# Requirement Analysis — In-House Call Center Platform

**Prepared by:** Md Mehbub Ul Islam
**Role:** System Analyst / Solution Architect (candidate exercise)
**Date:** August 2026

## 1. Business Context

The company currently depends on a third-party call center tool to handle customer-facing voice
communication. The goal is to replace this with an in-house platform that:

- Handles **inbound** calls (customers calling in) and **outbound** calls (agents calling customers).
- Integrates with the **existing CRM** so agents work with full customer context.
- Supports **50+ concurrent agents today**, scaling to **500+ agents** in the future.
- Leaves room for **AI features** later: call summarization, transcription, and smart routing.
- Is built on **.NET Core (backend)** and **Angular (frontend)**, unless a stronger alternative is justified.

## 2. Business Goals

1. Remove recurring license/seat costs and vendor lock-in from the third-party tool.
2. Own the data — call recordings, transcripts, and customer interaction history — instead of it
   living in a vendor's system.
3. Get tighter CRM integration than the current tool allows (real-time screen-pop, logging,
   click-to-call from CRM records).
4. Build a platform that can absorb AI capabilities as they mature, without a re-architecture.
5. Support the company's growth path (10x agent headcount) without a full platform rewrite.

## 3. Stakeholders

| Stakeholder | Interest |
|---|---|
| Call center agents | Fast, reliable UI to answer/place calls, see customer context, log outcomes |
| Team leads / supervisors | Real-time queue visibility, agent monitoring, call barge/whisper |
| Admin / Ops team | User management, routing rules, IVR configuration, reporting |
| CRM team / data owners | Clean, reliable sync between calls and CRM records |
| IT / Infrastructure | Uptime, security, compliance, on-call burden |
| Compliance / Legal | Call recording consent, data retention, regional telephony regulations |
| Leadership / Finance | Cost vs. the third-party tool, ROI, timeline |
| Future AI/Data team | Access to clean call data (audio + metadata) for future AI features |

## 4. Assumptions



- The CRM exposes a REST or GraphQL API (not a legacy/on-prem-only system) that can be integrated with.
- Telephony connectivity will go through a carrier via **SIP trunking**, using a telephony platform
  (e.g., a SIP-based PBX/softswitch or a CPaaS provider) rather than building a softswitch from scratch.
- Agents work from desktop browsers (Chrome/Edge) on a stable network, not primarily mobile.
- "500+ agents" means 500+ **concurrent** seats, not just user accounts — this materially affects
  telephony and media capacity planning.
- Call recording is required (for QA/compliance), with configurable retention.
- The company operates in a single primary region/timezone initially (Bangladesh), with room to
  add more later.
- Budget favors a **hybrid build**: build the app layer in-house, but use a proven telephony/media
  provider rather than writing a custom SIP stack — reinventing carrier-grade telephony is high-risk
  and out of scope for a small team.

## 5. Functional Requirements

### 5.1 Agent Portal
- FR1: Agent can log in and set status (Available, Busy, Break, Offline).
- FR2: Agent receives inbound calls with a "screen pop" showing matched CRM customer data.
- FR3: Agent can place outbound calls (click-to-call from CRM record or manual dial).
- FR4: Agent can hold, transfer (warm/cold), and conference calls.
- FR5: Agent can log call disposition/notes, which sync back to the CRM.
- FR6: Agent can see their own call history and basic personal stats.

### 5.2 Admin / Supervisor Portal
- FR7: Admin manages agents, teams, skills, and roles (RBAC).
- FR8: Admin configures IVR menus and routing rules (skill-based, round-robin, priority).
- FR9: Supervisor sees a real-time dashboard: queue length, wait times, agent statuses.
- FR10: Supervisor can listen-in / whisper / barge on live calls (QA use case).
- FR11: Admin configures outbound calling rules (caller ID, allowed hours, dialing lists).

### 5.3 Telephony Core
- FR12: System handles inbound call routing to the right queue/agent.
- FR13: System places outbound calls on behalf of an agent through the carrier.
- FR14: System records calls (audio) and stores them with metadata (caller, agent, duration, disposition).
- FR15: System supports IVR (menu-based call routing before reaching an agent).

### 5.4 CRM Integration
- FR16: Incoming call number is matched against CRM contacts in real time.
- FR17: Call outcome/notes/duration are written back to the corresponding CRM record.
- FR18: Click-to-call is available directly from CRM contact records.

### 5.5 Reporting
- FR19: Historical reports on call volume, wait time, abandonment rate, agent productivity.
- FR20: Exportable reports (CSV/Excel) for management.
- FR21: Recordings are retrievable/searchable by agent, date, customer, or call ID.

## 6. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Scalability | Support 50 concurrent agents at launch, scale to 500+ without re-architecture |
| Availability | Target 99.9% uptime for call-handling path; degrade gracefully, not fail completely |
| Latency | Call setup time and CRM screen-pop under ~1–2 seconds |
| Security | TLS everywhere, encrypted call recordings at rest, RBAC, audit logging |
| Compliance | Call recording consent handling, configurable data retention, PII protection |
| Portability | Cloud-agnostic where reasonable; avoid hard vendor lock-in beyond the telephony provider |
| Observability | Centralized logging, metrics, and alerting for the telephony and app layers |
| Maintainability | Modular architecture so AI features can be added as separate services later |
| Disaster Recovery | Defined RPO/RTO for call data and configuration; automated backups |

## 7. Out of Scope (v1)

- Building a custom SIP softswitch/media server from scratch (use a proven telephony provider/PBX instead).
- AI features themselves (summarization, transcription, smart routing) — only the *readiness* for them.
- Omni-channel support (chat, email, social) — voice only for v1.
- Mobile native agent apps — browser-based agent portal only for v1.
- Multi-region/multi-tenant support — single company, single primary region for v1.

## 8. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Underestimating telephony complexity (carrier integration, call quality, failover) | High | Use a mature SIP trunk/CPaaS provider instead of building from scratch |
| CRM API limitations (rate limits, missing webhooks) | Medium | Confirm CRM API capabilities early (see Stakeholder Questions) |
| Scaling from 50 → 500 agents breaks assumptions made at MVP stage | High | Design stateless services and externalize session/queue state from day one |
| Call recording storage costs grow fast | Medium | Tiered storage (hot/cold), configurable retention policy |
| Compliance gaps (consent, data residency) | High | Involve Legal/Compliance before go-live, not after |
| Team's first large real-time system | Medium | Time-box MVP tightly, lean on managed telephony infra to reduce custom real-time code |
