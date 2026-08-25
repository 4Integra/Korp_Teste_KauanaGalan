import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateProductRequest,
  IncreaseStockRequest,
  IncreaseStockResponse,
  Product
} from '../../shared/models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private readonly productsUrl =
    'http://localhost:5001/api/products';

  private readonly stockUrl =
    'http://localhost:5001/api/stock';

  constructor(
    private readonly http: HttpClient
  ) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(
      this.productsUrl
    );
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(
      `${this.productsUrl}/${id}`
    );
  }

  create(
    request: CreateProductRequest
  ): Observable<Product> {
    return this.http.post<Product>(
      this.productsUrl,
      request
    );
  }

  increaseStock(
    request: IncreaseStockRequest
  ): Observable<IncreaseStockResponse> {
    return this.http.post<IncreaseStockResponse>(
      `${this.stockUrl}/increase`,
      request
    );
  }
}