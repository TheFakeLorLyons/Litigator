import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Deadline } from '../models/deadline.model';

@Injectable({
  providedIn: 'root'
})
export class DeadlineService {
  private apiUrl = `${environment.apiUrl}/deadlines`;

  constructor(private http: HttpClient) { }

  getUpcomingDeadlines(days: number = 30): Observable<Deadline[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<Deadline[]>(`${this.apiUrl}/upcoming`, { params });
  }

  getDeadlinesByCaseId(caseId: number): Observable<Deadline[]> {
    return this.http.get<Deadline[]>(`${this.apiUrl}/case/${caseId}`);
  }

  createDeadline(deadline: Partial<Deadline>): Observable<Deadline> {
    return this.http.post<Deadline>(this.apiUrl, deadline);
  }

  updateDeadline(id: number, deadline: Partial<Deadline>): Observable<Deadline> {
    return this.http.put<Deadline>(`${this.apiUrl}/${id}`, deadline);
  }

  markDeadlineComplete(id: number): Observable<Deadline> {
    return this.http.put<Deadline>(`${this.apiUrl}/${id}/complete`, {});
  }

  deleteDeadline(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
