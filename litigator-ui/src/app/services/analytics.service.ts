import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AttorneyPerformanceDTO,
  CaseOutcomePredictionDTO,
  CriticalCaseDTO,
  MonthlyTrendDTO,
  DeadlinePerformanceDTO
} from '../models/analytics.model';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = `${environment.apiUrl}/analytics`;

  constructor(private http: HttpClient) { }

  getAttorneyPerformance(): Observable<AttorneyPerformanceDTO[]> {
    return this.http.get<AttorneyPerformanceDTO[]>(`${this.apiUrl}/attorney-performance`);
  }

  getCaseOutcomePredictions(): Observable<CaseOutcomePredictionDTO[]> {
    return this.http.get<CaseOutcomePredictionDTO[]>(`${this.apiUrl}/case-predictions`);
  }

  getCriticalCases(): Observable<CriticalCaseDTO[]> {
    return this.http.get<CriticalCaseDTO[]>(`${this.apiUrl}/critical-cases`);
  }

  getMonthlyTrends(): Observable<MonthlyTrendDTO[]> {
    return this.http.get<MonthlyTrendDTO[]>(`${this.apiUrl}/monthly-trends`);
  }

  getDeadlinePerformance(): Observable<DeadlinePerformanceDTO[]> {
    return this.http.get<DeadlinePerformanceDTO[]>(`${this.apiUrl}/deadline-performance`);
  }
}
