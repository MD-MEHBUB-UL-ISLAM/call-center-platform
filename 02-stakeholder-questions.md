# Stakeholder Questions — Before Starting Build

The brief is deliberately incomplete. Below are the questions I'd ask before committing to a
design, grouped by theme, along with *why* each one matters.

## 1. Telephony & Carrier
1. Do we already have a relationship with a SIP trunk provider or CPaaS (e.g., Twilio, Vonage,
   a local BD carrier), or is that an open decision? — *This drives cost, call quality, and
   how much telephony infrastructure we build vs. buy.*
2. Do agents need local (Bangladesh) numbers, international numbers, or both?
3. Is there an existing PBX/IVR system we're migrating from, or config we should replicate 1:1?
4. What call volume are we handling today with the third-party tool (peak concurrent calls,
   daily volume)? — *needed to size the MVP correctly, not just "50 agents."*

## 2. CRM Integration
5. Which CRM is in use, and does it expose a REST/GraphQL API and webhooks, or only a UI?
6. Is there an existing integration (even partial) with the current third-party call tool we can
   look at as a reference for what data needs to flow?
7. What's the source of truth for customer phone numbers — is matching guaranteed to be unique
   per customer, or can multiple customers share/reuse a number (e.g. shared household lines)?

## 3. Scale & Growth
8. Is "500+ agents" a hard 12-month target, or a longer-term vision? — *affects how much
   scalability work belongs in MVP vs. later phases.*
9. Will agents be in one office/one shift, or multiple locations/shifts (affects network design,
   redundancy, and whether we need multi-region routing)?

## 4. Compliance & Recording
10. Is call recording a legal/compliance requirement, and if so, what's the required retention
    period?
11. Do we need consent messaging (e.g., "this call may be recorded") baked into the IVR flow?
12. Are there data residency requirements (must call data stay in-country)?

## 5. Budget, Team & Timeline
13. Is there a preferred cloud provider (AWS/Azure/GCP) or on-prem requirement, and an existing
    DevOps setup we should build on top of?
14. What's the realistic team size and timeline for v1 — is this a small team building over
    months, or a larger effort?
15. Is there budget for a commercial telephony/CPaaS layer, or is the expectation to self-host
    open-source telephony (e.g., Asterisk/FreeSWITCH/Kamailio)? — *This is probably the single
    biggest architecture decision, so I'd want it confirmed rather than assumed.*

## 6. AI Roadmap
16. For the future AI features (summaries, transcription, smart routing) — is there a rough
    timeline, or is "AI-ready" purely about not blocking future work?
17. Any preference for in-house AI vs. third-party AI APIs (e.g., speech-to-text providers) once
    that phase starts?

## 7. Success Criteria
18. What does "done" look like for leadership to consider this a successful replacement of the
    third-party tool — feature parity, cost savings, both?
