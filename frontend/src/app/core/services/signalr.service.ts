import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { CallDto, IncomingCallNotification } from '../models/call.model';

/**
 * Thin wrapper over the SignalR client - the real-time channel to the backend's
 * Notification Hub (CallHub). Exposes incoming events as signals so components can
 * bind to them directly, matching the signal-based reactivity used throughout this app.
 */
@Injectable({ providedIn: 'root' })
export class SignalrService {
  private connection?: signalR.HubConnection;

  readonly incomingCall = signal<IncomingCallNotification | null>(null);
  readonly callEnded = signal<CallDto | null>(null);
  readonly queueDepth = signal<{ queueName: string; queueDepth: number } | null>(null);
  readonly connectionState = signal<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);

  async connect(token: string): Promise<void> {
    if (this.connection) {
      await this.disconnect();
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('IncomingCall', (payload: IncomingCallNotification) => {
      this.incomingCall.set(payload);
    });

    this.connection.on('CallEnded', (payload: CallDto) => {
      this.callEnded.set(payload);
    });

    this.connection.on('QueueUpdated', (payload: { queueName: string; queueDepth: number }) => {
      this.queueDepth.set(payload);
    });

    this.connection.onreconnecting(() => this.connectionState.set(signalR.HubConnectionState.Reconnecting));
    this.connection.onreconnected(() => this.connectionState.set(signalR.HubConnectionState.Connected));
    this.connection.onclose(() => this.connectionState.set(signalR.HubConnectionState.Disconnected));

    await this.connection.start();
    this.connectionState.set(signalR.HubConnectionState.Connected);
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
    this.connectionState.set(signalR.HubConnectionState.Disconnected);
  }

  /** Clears the incoming-call banner once the agent has acted on it (accept/decline). */
  clearIncomingCall(): void {
    this.incomingCall.set(null);
  }
}
