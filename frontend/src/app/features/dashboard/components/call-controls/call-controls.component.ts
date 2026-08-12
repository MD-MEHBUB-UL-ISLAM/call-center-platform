import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CallDto } from '../../../../core/models/call.model';

const DISPOSITIONS = ['Resolved', 'Follow-up Required', 'Escalated', 'No Action Needed'];

/** Controls for the currently active call: end it with a required disposition + notes (FR5). */
@Component({
  selector: 'app-call-controls',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './call-controls.component.html',
  styleUrl: './call-controls.component.css'
})
export class CallControlsComponent {
  activeCall = input.required<CallDto>();
  endCall = output<{ dispositionCode: string; notes: string }>();

  dispositions = DISPOSITIONS;
  selectedDisposition = signal(DISPOSITIONS[0]);
  notes = signal('');

  submitEndCall(): void {
    this.endCall.emit({ dispositionCode: this.selectedDisposition(), notes: this.notes() });
    this.notes.set('');
  }
}
