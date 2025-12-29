import { Component, effect, inject, signal, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { StoreService } from '../../services/store.service';
import { InventoryItem, CreateOrderItem } from '../../models/order.model';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mx-auto p-6 max-w-5xl">
      <header class="mb-8 border-b border-slate-200 pb-6">
        <h2 class="text-3xl font-black text-slate-800 tracking-tight">Nueva Venta</h2>
        <p class="text-slate-500 mt-1">
          Creando orden para <span class="font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded">
            {{ storeService.selectedStore()?.name || 'Cargando...' }}
          </span>
        </p>
      </header>

      <!-- Loading State -->
      <div *ngIf="loading()" class="flex justify-center py-12">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div>
      </div>

      <!-- Empty State -->
      <div *ngIf="!loading() && inventory().length === 0" class="text-center py-12 bg-white rounded-2xl border border-dashed border-slate-300">
        <p class="text-slate-400 italic">No hay inventario disponible en esta tienda.</p>
        <button (click)="loadInventory()" class="mt-4 text-indigo-600 font-bold text-sm hover:underline">Reintentar</button>
      </div>

      <!-- Inventory Grid -->
      <div *ngIf="!loading() && inventory().length > 0" class="grid grid-cols-1 lg:grid-cols-3 gap-8">

        <!-- Product List -->
        <div class="lg:col-span-2 space-y-4">
          <div *ngFor="let item of inventory()"
               class="bg-white p-4 rounded-2xl border border-slate-200 shadow-sm hover:shadow-md transition-all flex justify-between items-center group">
            <div>
              <p class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">{{ item.skuCode }}</p>
              <h4 class="font-bold text-slate-800 text-lg">{{ item.productName }}</h4>
              <div class="flex gap-3 mt-2 text-sm">
                <span class="text-slate-500">Físico: <strong>{{ item.onHand }}</strong></span>
                <span class="text-amber-600">Reservado: <strong>{{ item.reserved }}</strong></span>
              </div>
            </div>

            <div class="flex flex-col items-end gap-2">
              <span class="text-xl font-black" [class.text-rose-500]="(item.onHand - item.reserved) <= 0" [class.text-emerald-600]="(item.onHand - item.reserved) > 0">
                {{ item.onHand - item.reserved }} <span class="text-xs font-medium text-slate-400">disp.</span>
              </span>

              <div class="flex items-center bg-slate-50 rounded-lg border border-slate-200">
                <button (click)="decrement(item.skuId)" class="px-3 py-1 text-slate-500 hover:text-indigo-600 font-bold">-</button>
                <input type="number" [ngModel]="cart[item.skuId]" (ngModelChange)="updateCart(item.skuId, $event)" min="0" [max]="item.onHand - item.reserved"
                       class="w-12 text-center bg-transparent border-none focus:ring-0 font-bold text-slate-800 p-0">
                <button (click)="increment(item.skuId, item.onHand - item.reserved)" class="px-3 py-1 text-slate-500 hover:text-indigo-600 font-bold">+</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Cart Summary -->
        <div class="lg:col-span-1">
          <div class="bg-slate-900 text-white p-6 rounded-3xl sticky top-6 shadow-xl shadow-slate-200">
            <h3 class="text-xl font-black mb-4">Resumen</h3>

            <div class="space-y-3 mb-6">
              <div *ngFor="let item of getCartItems()" class="flex justify-between text-sm">
                <span class="text-slate-300">{{ item.name }} (x{{ item.qty }})</span>
                <span class="font-bold text-white">Pending</span>
              </div>
              <p *ngIf="getCartItems().length === 0" class="text-slate-500 italic text-sm">El carrito está vacío.</p>
            </div>

            <div class="border-t border-slate-700 pt-4 mt-4">
              <div class="flex justify-between items-center mb-6">
                <span class="text-slate-400">Total Items</span>
                <span class="text-2xl font-black">{{ getTotalItems() }}</span>
              </div>

              <button (click)="submitOrder()"
                      [disabled]="getTotalItems() === 0 || processing()"
                      class="w-full bg-indigo-500 hover:bg-indigo-400 text-white font-bold py-4 rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed flex justify-center">
                <span *ngIf="!processing()">Confirmar Orden ➔</span>
                <div *ngIf="processing()" class="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
              </button>
            </div>

            <!-- Feedback -->
            <div *ngIf="message()" class="mt-4 p-3 rounded-lg text-xs font-medium"
                 [ngClass]="success() ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/30' : 'bg-rose-500/20 text-rose-300 border border-rose-500/30'">
              {{ message() }}
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OrderCreateComponent {
  inventory = signal<InventoryItem[]>([]);
  loading = signal<boolean>(false);
  processing = signal<boolean>(false);
  message = signal<string>('');
  success = signal<boolean>(false);

  cart: { [skuId: number]: number } = {};

  storeService = inject(StoreService);
  private api = inject(ApiService);

  constructor() {
    // Auto-load when store changes
    effect(() => {
      const storeId = this.storeService.selectedStoreId();
      untracked(() => {
        this.loadInventory(storeId);
      });
    });
  }

  loadInventory(storeId?: number) {
    const id = storeId || this.storeService.selectedStoreId();
    this.loading.set(true);
    this.message.set('');
    this.cart = {}; // Reset cart

    this.api.getInventory(id).subscribe({
      next: (data) => {
        this.inventory.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.message.set('Error cargando inventario.');
        this.loading.set(false);
      }
    });
  }

  increment(skuId: number, max: number) {
    const current = this.cart[skuId] || 0;
    if (current < max) this.cart[skuId] = current + 1;
  }

  decrement(skuId: number) {
    const current = this.cart[skuId] || 0;
    if (current > 0) this.cart[skuId] = current - 1;
  }

  updateCart(skuId: number, value: number) {
    this.cart[skuId] = value;
  }

  getCartItems() {
    return this.inventory()
      .filter(i => this.cart[i.skuId] > 0)
      .map(i => ({ name: i.productName, qty: this.cart[i.skuId] }));
  }

  getTotalItems() {
    return Object.values(this.cart).reduce((a, b) => a + (b || 0), 0);
  }

  submitOrder() {
    const items: CreateOrderItem[] = Object.keys(this.cart)
      .map(skuId => ({
        skuId: Number(skuId),
        quantity: this.cart[Number(skuId)]
      }))
      .filter(i => i.quantity > 0);

    if (items.length === 0) return;

    this.processing.set(true);
    this.message.set('');

    this.api.createOrder({ storeId: this.storeService.selectedStoreId(), items }).subscribe({
      next: (res) => {
        this.success.set(true);
        this.message.set(`Orden #${res.orderId} creada!`);
        this.processing.set(false);
        this.loadInventory();
      },
      error: (err) => {
        this.success.set(false);
        this.processing.set(false);
        this.message.set(err.error?.message || 'Error procesando la orden.');
      }
    });
  }
}
