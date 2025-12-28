import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto p-4">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold">Listado de Órdenes</h2>
        <button (click)="refresh()" class="text-blue-600 underline">Refrescar</button>
      </div>

      <div *ngIf="loading" class="text-gray-500">Cargando órdenes...</div>

      <div *ngIf="!loading && orders.length === 0" class="text-gray-500">No hay órdenes registradas.</div>

      <table *ngIf="orders.length > 0" class="min-w-full bg-white border shadow-sm">
        <thead>
          <tr class="bg-gray-50 border-b">
            <th class="p-3 text-left">ID</th>
            <th class="p-3 text-left">Tienda</th>
            <th class="p-3 text-left">Estado</th>
            <th class="p-3 text-right">Total</th>
            <th class="p-3 text-left">Fecha</th>
            <th class="p-3 text-left">Items</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let order of orders" class="border-b hover:bg-gray-50">
            <td class="p-3 font-mono text-sm">#{{ order.id }}</td>
            <td class="p-3">{{ order.storeName }}</td>
            <td class="p-3">
              <span class="px-2 py-1 rounded text-xs font-bold"
                    [ngClass]="{
                      'bg-green-100 text-green-800': order.status === 'Confirmed',
                      'bg-yellow-100 text-yellow-800': order.status === 'Reserved',
                      'bg-gray-100 text-gray-800': order.status === 'Draft'
                    }">
                {{ order.status }}
              </span>
            </td>
            <td class="p-3 text-right font-bold">\${{ order.totalAmount | number:'1.2-2' }}</td>
            <td class="p-3 text-sm text-gray-600">{{ order.createdAt | date:'short' }}</td>
            <td class="p-3 text-sm">
              <ul class="list-disc list-inside">
                <li *ngFor="let item of order.items">
                  {{ item.quantity }}x {{ item.skuCode }}
                </li>
              </ul>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  loading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    this.loading = true;
    this.api.getOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching orders', err);
        this.loading = false;
      }
    });
  }
}
