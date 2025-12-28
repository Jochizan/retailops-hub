import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrderRequest, Order, InventoryItem } from '../models/order.model';
import { v4 as uuidv4 } from 'uuid'; // Need to install uuid or write simple generator

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5000/api'; // Ajustar puerto según configuración de .NET

  constructor(private http: HttpClient) { }

  getOrders(storeId?: number, status?: string): Observable<Order[]> {
    let params = new HttpParams();
    if (storeId) params = params.set('storeId', storeId);
    if (status) params = params.set('status', status);

    return this.http.get<Order[]>(`${this.baseUrl}/orders`, { params });
  }

  createOrder(request: CreateOrderRequest): Observable<any> {
    const idempotencyKey = crypto.randomUUID(); // Native browser UUID
    const headers = new HttpHeaders().set('Idempotency-Key', idempotencyKey);

    return this.http.post(`${this.baseUrl}/orders`, request, { headers });
  }

  getInventory(storeId: number): Observable<InventoryItem[]> {
    const params = new HttpParams().set('storeId', storeId);
    return this.http.get<InventoryItem[]>(`${this.baseUrl}/inventory`, { params });
  }
}
