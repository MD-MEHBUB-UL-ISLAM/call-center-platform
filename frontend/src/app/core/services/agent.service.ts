import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Agent, AgentStatus } from '../models/agent.model';

@Injectable({ providedIn: 'root' })
export class AgentService {
  private readonly base = `${environment.apiBaseUrl}/agents`;

  constructor(private http: HttpClient) {}

  updateStatus(agentId: number, status: AgentStatus) {
    return this.http.put<Agent>(`${this.base}/${agentId}/status`, { status });
  }
}
