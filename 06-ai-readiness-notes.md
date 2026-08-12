# AI-Readiness Notes

No AI features are implemented in v1. This document is about **not painting ourselves into a
corner** — making sure the architecture doesn't need to be reworked when AI features are added.

## 1. Future AI Features Anticipated

- **Call transcription** — speech-to-text on recorded (or live) calls.
- **Call summarization** — LLM-generated summary of the call, likely written to the CRM record.
- **Smart routing** — using call/customer context (or even sentiment) to route more intelligently
  than static skill-based rules.
- (Likely later) sentiment analysis, agent coaching suggestions, auto-QA scoring.

## 2. Why the Current Design Already Leaves Room for This

- **Event-driven backbone**: A `CallEnded` event already exists on the message broker. Adding
  transcription/summarization later means adding a new consumer service (e.g., `AI Insights
  Service`) that listens to that same event — no changes needed to the telephony or routing core.
- **Recordings are stored with structured metadata**, not just raw files — an AI service can find
  and process a specific call's audio without needing new plumbing to identify calls.
- **CRM write-back is centralized** in the CRM Integration Service — an AI summary can reuse the
  exact same write-back pathway as today's manual agent notes, just with a different payload.
- **Routing Engine is a distinct service** with its own decision logic — "smart routing" later
  means swapping/extending the routing rules (e.g., calling a scoring model) inside that one
  service, not touching telephony orchestration or the client apps.

## 3. What We'd Add (Not Now, But Without Rework)

| Future addition | How it plugs in |
|---|---|
| AI Insights Service (transcription + summary) | New consumer of `CallEnded` / recording-ready events; writes results via existing CRM Integration Service |
| Speech-to-text | Either a call to a third-party STT API, or a self-hosted model — isolated behind the AI Insights Service so the choice can change without affecting the rest of the system |
| Smart routing model | Called from within the Routing Engine as an additional signal alongside skill/queue rules; can be introduced as an A/B'd enhancement, not a replacement |
| Live transcription (real-time) | Would require tapping the media stream at the Telephony Orchestration layer — flagged as a bigger change, not assumed to be free; noted here explicitly rather than glossed over |
| Agent-facing AI suggestions | Delivered through the existing SignalR notification channel, same path as screen-pop today |

## 4. Data Considerations for AI (Flagged Now, Decided Later)

- Recordings and transcripts are exactly the kind of data that needs **clear retention and access
  policy** once used for AI processing — this ties back to the compliance questions raised in the
  Stakeholder Questions doc, and should be revisited before any AI feature goes live, not after.
- If a third-party AI API is used for transcription/summarization, customer call data would leave
  our infrastructure — worth a deliberate compliance/legal sign-off, not an engineering-only decision.

## 5. What This Doc Deliberately Does Not Do

Per the exercise brief, no AI implementation is proposed here. The goal was to check that today's
architectural choices (event-driven design, isolated services, centralized recording/CRM handling)
don't have to be undone to bolt AI on later — and to flag the one place (live transcription) where
a real design decision will be needed when that day comes.
