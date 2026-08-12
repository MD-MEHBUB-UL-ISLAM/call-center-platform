export type CallDirection = 'Inbound' | 'Outbound';
export type CallStatus = 'Queued' | 'Ringing' | 'Connected' | 'OnHold' | 'Completed' | 'Abandoned';

export interface CallDto {
  id: number;
  correlationId: string;
  direction: CallDirection;
  status: CallStatus;
  fromNumber: string;
  toNumber: string;
  agentId: number | null;
  agentName: string | null;
  queueName: string | null;
  crmContactId: string | null;
  crmContactName: string | null;
  queuedAtUtc: string;
  connectedAtUtc: string | null;
  endedAtUtc: string | null;
  dispositionCode: string | null;
  notes: string | null;
}

/** Pushed over SignalR when a call is routed to this agent - the CRM screen-pop payload. */
export interface IncomingCallNotification {
  callId: number;
  correlationId: string;
  fromNumber: string;
  crmContactId: string | null;
  crmContactName: string | null;
  crmCompany: string | null;
  crmTier: string | null;
  crmLastInteractionSummary: string | null;
  queueName: string;
}
