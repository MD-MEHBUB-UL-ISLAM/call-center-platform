import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { CallDto } from '../../../../core/models/call.model';

/** Prototype of the reporting/history view described in the MVP's "basic historical reporting" scope. */
@Component({
  selector: 'app-call-history',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './call-history.component.html',
  styleUrl: './call-history.component.css'
})
export class CallHistoryComponent {
  calls = input.required<CallDto[]>();
}
