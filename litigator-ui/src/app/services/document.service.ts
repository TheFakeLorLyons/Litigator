import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Document } from '../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private apiUrl = 'https://localhost:7000/api/documents';

  constructor(private http: HttpClient) { }

  // Basic CRUD operations
  getDocuments(pageNumber?: number, pageSize?: number): Observable<any> {
    let params = new HttpParams();
    if (pageNumber) params = params.set('pageNumber', pageNumber.toString());
    if (pageSize) params = params.set('pageSize', pageSize.toString());
    return this.http.get<any>(this.apiUrl, { params });
  }

  getDocument(id: number): Observable<Document> {
    return this.http.get<Document>(`${this.apiUrl}/${id}`);
  }

  updateDocument(id: number, document: Partial<Document>): Observable<Document> {
    return this.http.put<Document>(`${this.apiUrl}/${id}`, document);
  }

  deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Document-specific methods
  getDocumentsByCase(caseId: number): Observable<Document[]> {
    return this.http.get<Document[]>(`${this.apiUrl}/case/${caseId}`);
  }

  getDocumentsByType(documentType: string): Observable<Document[]> {
    const params = new HttpParams().set('type', documentType);
    return this.http.get<Document[]>(`${this.apiUrl}/type`, { params });
  }

  uploadDocument(
    file: File,
    caseId: number,
    documentType: string,
    description?: string
  ): Observable<HttpEvent<Document>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('caseId', caseId.toString());
    formData.append('documentType', documentType);
    if (description) {
      formData.append('description', description);
    }

    const req = new HttpRequest('POST', `${this.apiUrl}/upload`, formData, {
      reportProgress: true,
      responseType: 'json'
    });

    return this.http.request<Document>(req);
  }

  uploadMultipleDocuments(
    files: File[],
    caseId: number,
    documentType: string
  ): Observable<HttpEvent<Document[]>> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    formData.append('caseId', caseId.toString());
    formData.append('documentType', documentType);

    const req = new HttpRequest('POST', `${this.apiUrl}/upload-multiple`, formData, {
      reportProgress: true,
      responseType: 'json'
    });

    return this.http.request<Document[]>(req);
  }

  downloadDocument(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, {
      responseType: 'blob'
    });
  }

  previewDocument(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/preview`, {
      responseType: 'blob'
    });
  }

  searchDocuments(query: string, caseId?: number): Observable<Document[]> {
    let params = new HttpParams().set('query', query);
    if (caseId) params = params.set('caseId', caseId.toString());
    return this.http.get<Document[]>(`${this.apiUrl}/search`, { params });
  }

  getDocumentStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/statistics`);
  }
}
