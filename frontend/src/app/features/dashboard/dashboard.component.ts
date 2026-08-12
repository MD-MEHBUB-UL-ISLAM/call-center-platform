import { Component, OnDestroy, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { SignalrService } from '../../core/services/signalr.service';
import { CallService } from '../../core/services/call.service';
import { AgentService } from '../../core/services/agent.service';
import { QueueService, QueueDto } from '../../core/services/queue.service';
import { CallDto } from '../../core/models/call.model';
import { AgentStatus } from '../../core/models/agent.model';
import { IncomingCallPanelComponent } from './components/incoming-call-panel/incoming-call-panel.component';
import { CallControlsComponent } from './components/call-controls/call-controls.component';
import { CallHistoryComponent } from './components/call-history/call-history.component';

const STATUS_OPTIONS: AgentStatus[] = ['Available', 'Busy', 'OnBreak', 'Offline'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, IncomingCallPanelComponent, CallControlsComponent, CallHistoryComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly signalr = inject(SignalrService);
  private readonly callService = inject(CallService);
  private readonly agentService = inject(AgentService);
  private readonly queueService = inject(QueueService);

  readonly agent = this.auth.currentAgent;
  readonly incomingCall = this.signalr.incomingCall;
  readonly statusOptions = STATUS_OPTIONS;

  activeCall = signal<CallDto | null>(null);
  callHistory = signal<CallDto[]>([]);
  queues = signal<QueueDto[]>([]);

  // "Simulate inbound call" form state - this stands in for the SIP/CPaaS provider's webhook.
  simFromNumber = signal('+8801700000001');
  simToNumber = signal('+8801900000000');
  simQueueName = signal('Support');
  simulateError = signal<string | null>(null);

  constructor() {
    // Whenever a CallEnded event arrives over SignalR, refresh history and clear the active call.
    effect(() => {
      const ended = this.signalr.callEnded();
      if (ended) {
        this.activeCall.set(null);
        this.refreshHistory();
      }
    });
  }

  ngOnInit(): void {
    const token = this.auth.token;
    if (token) {
      this.signalr.connect(token);
    }

    this.queueService.getAll().subscribe((qs) => this.queues.set(qs));
    this.refreshHistory();
  }

  ngOnDestroy(): void {
    this.signalr.disconnect();
  }

  changeStatus(status: AgentStatus): void {
    const agent = this.agent();
    if (!agent) return;

    this.agentService.updateStatus(agent.id, status).subscribe(() => {
      this.auth.updateCurrentAgentStatus(status);
    });
  }

  simulateInboundCall(): void {
    this.simulateError.set(null);
    this.callService
      .simulateInbound({
        fromNumber: this.simFromNumber(),
        toNumber: this.simToNumber(),
        queueName: this.simQueueName()
      })
      .subscribe({
        error: () => this.simulateError.set('Could not simulate call — check the queue name matches a seeded queue.')
      });
  }

  acceptCall(callId: number): void {
    this.callService.accept(callId).subscribe((call) => {
      this.activeCall.set(call);
      this.signalr.clearIncomingCall();
    });
  }

  endCall(payload: { dispositionCode: string; notes: string }): void {
    const call = this.activeCall();
    if (!call) return;

    this.callService.end(call.id, payload).subscribe((updated) => {
      this.activeCall.set(null);
      this.callHistory.update((history) => [updated, ...history.filter((c) => c.id !== updated.id)]);
    });
  }

  logout(): void {
    this.signalr.disconnect();
    this.auth.logout();
  }

  private refreshHistory(): void {
    this.callService.getMine().subscribe((calls) => this.callHistory.set(calls));
  }
}
