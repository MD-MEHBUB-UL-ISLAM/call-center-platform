import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Agent, LoginResponse } from '../models/agent.model';

const TOKEN_KEY = 'ccp_token';
const AGENT_KEY = 'ccp_agent';

/**
 * Handles login and holds the current agent's identity as a signal so any component
 * (dashboard, header, guards) can react to it without prop-drilling.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentAgent = signal<Agent | null>(this.readStoredAgent());
  readonly currentAgent = this._currentAgent.asReadonly();
  readonly isLoggedIn = computed(() => this._currentAgent() !== null);

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, { email, password })
      .pipe(
        tap((response) => {
          localStorage.setItem(TOKEN_KEY, response.token);
          localStorage.setItem(AGENT_KEY, JSON.stringify(response.agent));
          this._currentAgent.set(response.agent);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(AGENT_KEY);
    this._currentAgent.set(null);
    this.router.navigateByUrl('/login');
  }

  updateCurrentAgentStatus(status: Agent['status']): void {
    const agent = this._currentAgent();
    if (!agent) return;
    const updated = { ...agent, status };
    localStorage.setItem(AGENT_KEY, JSON.stringify(updated));
    this._currentAgent.set(updated);
  }

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private readStoredAgent(): Agent | null {
    const raw = localStorage.getItem(AGENT_KEY);
    return raw ? (JSON.parse(raw) as Agent) : null;
  }
}
