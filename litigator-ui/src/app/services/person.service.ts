// src/app/services/person.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Person, PersonType, LegalSpecialization } from '../models/person.model';
import { PagedResult } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  private apiUrl = 'https://localhost:7000/api/people';

  constructor(private http: HttpClient) { }

  // Basic CRUD operations
  getPeople(pageNumber?: number, pageSize?: number): Observable<PagedResult<Person>> {
    let params = new HttpParams();
    if (pageNumber) params = params.set('pageNumber', pageNumber.toString());
    if (pageSize) params = params.set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<Person>>(this.apiUrl, { params });
  }

  getPerson(id: number): Observable<Person> {
    return this.http.get<Person>(`${this.apiUrl}/${id}`);
  }

  createPerson(person: Partial<Person>): Observable<Person> {
    return this.http.post<Person>(this.apiUrl, person);
  }

  updatePerson(id: number, person: Partial<Person>): Observable<Person> {
    return this.http.put<Person>(`${this.apiUrl}/${id}`, person);
  }

  deletePerson(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Person type specific methods
  getAttorneys(active?: boolean): Observable<Person[]> {
    let params = new HttpParams();
    if (active !== undefined) params = params.set('active', active.toString());
    return this.http.get<Person[]>(`${this.apiUrl}/attorneys`, { params });
  }

  getClients(active?: boolean): Observable<Person[]> {
    let params = new HttpParams();
    if (active !== undefined) params = params.set('active', active.toString());
    return this.http.get<Person[]>(`${this.apiUrl}/clients`, { params });
  }

  getJudges(active?: boolean): Observable<Person[]> {
    let params = new HttpParams();
    if (active !== undefined) params = params.set('active', active.toString());
    return this.http.get<Person[]>(`${this.apiUrl}/judges`, { params });
  }

  getAttorneysBySpecialization(specialization: LegalSpecialization): Observable<Person[]> {
    const params = new HttpParams().set('specialization', specialization.toString());
    return this.http.get<Person[]>(`${this.apiUrl}/attorneys/specialization`, { params });
  }

  // Search and filter methods
  searchPeople(query: string, personType?: PersonType): Observable<Person[]> {
    let params = new HttpParams().set('query', query);
    if (personType !== undefined) params = params.set('personType', personType.toString());
    return this.http.get<Person[]>(`${this.apiUrl}/search`, { params });
  }

  togglePersonStatus(id: number): Observable<Person> {
    return this.http.patch<Person>(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  getPersonStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/statistics`);
  }
}
