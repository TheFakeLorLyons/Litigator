import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AttorneyDTO, AttorneyDetailDTO } from '../models/attorney.model';
import { LegalSpecialization } from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class AttorneyService {
  private apiUrl = `${environment.apiUrl}/attorneys`;

  constructor(private http: HttpClient) { }

  // Get all attorneys
  getAllAttorneys(): Observable<AttorneyDTO[]> {
    return this.http.get<AttorneyDTO[]>(this.apiUrl);
  }

  // Get active attorneys only
  getActiveAttorneys(): Observable<AttorneyDTO[]> {
    return this.http.get<AttorneyDTO[]>(`${this.apiUrl}/active`);
  }

  // Get attorney by ID
  getAttorneyById(id: number): Observable<AttorneyDetailDTO> {
    return this.http.get<AttorneyDetailDTO>(`${this.apiUrl}/${id}`);
  }

  // Get attorney by bar number
  getAttorneyByBarNumber(barNumber: string): Observable<AttorneyDetailDTO> {
    return this.http.get<AttorneyDetailDTO>(`${this.apiUrl}/bar/${barNumber}`);
  }

  // Create a new attorney
  createAttorney(attorney: AttorneyDetailDTO): Observable<AttorneyDetailDTO> {
    return this.http.post<AttorneyDetailDTO>(this.apiUrl, attorney);
  }

  // Update an existing attorney
  updateAttorney(id: number, attorney: AttorneyDetailDTO): Observable<AttorneyDetailDTO> {
    return this.http.put<AttorneyDetailDTO>(`${this.apiUrl}/${id}`, attorney);
  }

  // Delete an attorney
  deleteAttorney(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Get attorneys by specialization
  getAttorneysBySpecialization(specialization: LegalSpecialization): Observable<AttorneyDTO[]> {
    const params = new HttpParams().set('specialization', specialization.toString());
    return this.http.get<AttorneyDTO[]>(`${this.apiUrl}/specialization`, { params });
  }

  // Search attorneys
  searchAttorneys(query: string): Observable<AttorneyDTO[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<AttorneyDTO[]>(`${this.apiUrl}/search`, { params });
  }

  // Toggle attorney active status
  toggleAttorneyStatus(id: number): Observable<AttorneyDetailDTO> {
    return this.http.patch<AttorneyDetailDTO>(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  // Get attorney statistics
  getAttorneyStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/statistics`);
  }
}
