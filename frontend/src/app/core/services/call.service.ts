import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CallDto } from '../models/call.model';

export interface SimulateInboundCallRequest {
  fromNumber: string;
  toNumber: string;
  queueName: string;
}

export interface EndCallRequest {
  dispositionCode: string;
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class CallService {
  private readonly base = `${environment.apiBaseUrl}/calls`;

  constructor(private http: HttpClient) {}

  /** Stands in for the telephony provider's inbound webhook - used by the demo "Simulate call" button. */
  simulateInbound(request: SimulateInboundCallRequest) {
    return this.http.post<CallDto>(`${this.base}/simulate-inbound`, request);
  }

  accept(callId: number) {
    return this.http.post<CallDto>(`${this.base}/${callId}/accept`, {});
  }

  placeOutbound(toNumber: string) {
    return this.http.post<CallDto>(`${this.base}/outbound`, { toNumber });
  }

  end(callId: number, request: EndCallRequest) {
    return this.http.post<CallDto>(`${this.base}/${callId}/end`, request);
  }

  getMine() {
    return this.http.get<CallDto[]>(`${this.base}/mine`);
  }
}
