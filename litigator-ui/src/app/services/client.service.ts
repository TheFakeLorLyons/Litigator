import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ClientDTO, ClientDetailDTO } from '../models/client.model';
import { BaseService } from './base.service';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ClientService extends BaseService {
  private apiUrl = `${environment.apiUrl}/clients`;

  constructor(private http: HttpClient) {
    super();
  }

  // Read operations - return DTOs
  getAllClients(): Observable<ClientDTO[]> {
    return this.http.get<ClientDTO[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getClientById(id: number): Observable<ClientDetailDTO> {
    return this.http.get<ClientDetailDTO>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  searchClients(searchTerm: string): Observable<ClientDTO[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<ClientDTO[]>(`${this.apiUrl}/search`, { params }).pipe(
      catchError(this.handleError)
    );
  }

  getActiveClients(): Observable<ClientDTO[]> {
    console.log('Making request to:', this.apiUrl + '/active');
    return this.http.get<ClientDTO[]>(`${this.apiUrl}/active`).pipe(
      tap(data => console.log('Received client data:', data)),
      catchError(error => {
        console.error('Client service error:', error);
        return this.handleError(error);
      })
    );
  }

  // Write operations
  createClient(clientDto: ClientDetailDTO): Observable<ClientDetailDTO> {
    return this.http.post<ClientDetailDTO>(this.apiUrl, clientDto).pipe(
      catchError(this.handleError)
    );
  }

  updateClient(clientDto: ClientDetailDTO): Observable<ClientDetailDTO> {
    return this.http.put<ClientDetailDTO>(`${this.apiUrl}/${clientDto.clientId}`, clientDto).pipe(
      catchError(this.handleError)
    );
  }

  deleteClient(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }
}
