import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateInvoiceRequest,
  Invoice
} from '../../shared/models/invoice';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {

  private readonly apiUrl =
    'http://localhost:5002/api/invoices';

  constructor(
    private readonly http: HttpClient
  ) {}

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(
      this.apiUrl
    );
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(
      `${this.apiUrl}/${id}`
    );
  }

  create(
    request: CreateInvoiceRequest
  ): Observable<Invoice> {
    return this.http.post<Invoice>(
      this.apiUrl,
      request
    );
  }

  print(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(
      `${this.apiUrl}/${id}/print`,
      {}
    );
  }
}