import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { StoreService } from '../../services/store.service';
import { Order } from '../../models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mx-auto p-6 space-y-6">
      <div class="flex justify-between items-center">
        <h2 class="text-3xl font-black text-slate-800 tracking-tight">Gestión de Órdenes</h2>
        <button (click)="refresh()" class="bg-indigo-50 text-indigo-600 px-4 py-2 rounded-xl text-sm font-bold hover:bg-indigo-100 transition-colors">
          Refrescar Datos
        </button>
      </div>

      <div *ngIf="loading" class="flex justify-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>

      <div *ngIf="!loading && orders.length === 0" class="text-center py-20 bg-white border rounded-3xl border-dashed border-slate-300">
        <p class="text-slate-400 italic">No hay órdenes registradas en esta tienda.</p>
      </div>

      <div *ngIf="!loading && orders.length > 0" class="grid gap-4">
        <div *ngFor="let order of orders" 
             class="bg-white border border-slate-200 rounded-3xl p-6 shadow-sm hover:shadow-md transition-shadow flex justify-between items-center">
          
          <div class="flex gap-6 items-center">
            <div class="text-center bg-slate-50 px-4 py-2 rounded-2xl border border-slate-100">
              <p class="text-[10px] uppercase font-black text-slate-400">Orden</p>
              <p class="font-mono font-bold text-slate-700">#{{ order.id }}</p>
            </div>

            <div>
              <div class="flex items-center gap-3 mb-1">
                <span class="px-2 py-0.5 rounded-full text-[10px] font-black uppercase tracking-widest"
                      [ngClass]="{
                        'bg-emerald-100 text-emerald-700': order.status === 'Confirmed',
                        'bg-amber-100 text-amber-700': order.status === 'Reserved',
                        'bg-rose-100 text-rose-700': order.status === 'Cancelled',
                        'bg-slate-100 text-slate-700': order.status === 'Draft'
                      }">
                  {{ order.status }}
                </span>
                <span class="text-xs text-slate-400 font-medium">{{ order.createdAt | date:'medium' }}</span>
              </div>
              <p class="text-slate-500 text-sm">Tienda: <span class="text-slate-800 font-bold">{{ order.storeName }}</span></p>
              <div class="flex gap-2 mt-2">
                <span *ngFor="let item of order.items" class="text-[10px] bg-indigo-50 text-indigo-600 px-2 py-0.5 rounded font-bold">
                  {{ item.quantity }}x {{ item.skuCode }}
                </span>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-8">
            <div class="text-right">
              <p class="text-[10px] uppercase font-black text-slate-400">Total</p>
              <p class="text-2xl font-black text-slate-800">\${{ order.totalAmount | number:'1.2-2' }}</p>
            </div>

            <!-- Lifecycle Actions -->
            <div *ngIf="order.status === 'Reserved' && canManage()" class="flex gap-2 border-l border-slate-100 pl-6">
              <button (click)="cancel(order.id)" 
                      class="p-2 text-rose-600 hover:bg-rose-50 rounded-xl transition-colors font-bold text-sm">
                Cancelar
              </button>
              <button (click)="confirm(order.id)" 
                      class="bg-indigo-600 text-white px-6 py-2 rounded-xl text-sm font-bold hover:bg-indigo-700 shadow-lg shadow-indigo-200 transition-all">
                Confirmar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  loading = false;
  private api = inject(ApiService);
  private storeService = inject(StoreService);

  ngOnInit() {
    this.refresh();
  }

  canManage(): boolean {
    const role = this.storeService.userRole();
    return role === 'admin' || role === 'manager';
  }

  refresh() {
    this.loading = true;
    this.api.getOrders(this.storeService.selectedStoreId()).subscribe({
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

  confirm(id: number) {
    if (confirm('¿Confirmar esta orden? El stock físico será descontado.')) {
      this.api.confirmOrder(id).subscribe(() => this.refresh());
    }
  }

  cancel(id: number) {
    if (confirm('¿Cancelar esta orden? El stock reservado será liberado.')) {
      this.api.cancelOrder(id).subscribe(() => this.refresh());
    }
  }
}
