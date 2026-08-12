export type AgentRole = 'Agent' | 'Supervisor' | 'Admin';
export type AgentStatus = 'Offline' | 'Available' | 'OnCall' | 'Busy' | 'OnBreak';

export interface Agent {
  id: number;
  name: string;
  email: string;
  role: AgentRole;
  status: AgentStatus;
  queueName: string | null;
  skills: string[];
}

export interface LoginResponse {
  token: string;
  agent: Agent;
}
