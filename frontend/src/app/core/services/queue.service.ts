import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface QueueDto {
  id: number;
  name: string;
  requiredSkill: string;
}

@Injectable({ providedIn: 'root' })
export class QueueService {
  private readonly base = `${environment.apiBaseUrl}/queues`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<QueueDto[]>(this.base);
  }
}
