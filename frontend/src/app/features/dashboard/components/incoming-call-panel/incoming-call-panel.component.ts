import { Component, input, output } from '@angular/core';
import { IncomingCallNotification } from '../../../../core/models/call.model';

/** The "screen-pop" panel: shows CRM context the instant a call is routed to the agent. */
@Component({
  selector: 'app-incoming-call-panel',
  standalone: true,
  templateUrl: './incoming-call-panel.component.html',
  styleUrl: './incoming-call-panel.component.css'
})
export class IncomingCallPanelComponent {
  notification = input.required<IncomingCallNotification>();
  accept = output<number>();

  onAccept(): void {
    this.accept.emit(this.notification().callId);
  }
}
