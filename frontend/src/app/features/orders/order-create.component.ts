import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { InventoryItem, CreateOrderItem } from '../../models/order.model';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mx-auto p-4">
      <h2 class="text-2xl font-bold mb-4">Crear Nueva Orden</h2>

      <!-- Store Selection -->
      <div class="mb-6 flex gap-4 items-end">
        <div>
          <label class="block text-sm font-medium text-gray-700">Tienda ID</label>
          <input type="number" [(ngModel)]="storeId" class="mt-1 block w-32 border-gray-300 rounded-md shadow-sm border p-2">
        </div>
        <button (click)="loadInventory()" 
                class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
                [disabled]="loading">
          {{ loading ? 'Cargando...' : 'Cargar Inventario' }}
        </button>
      </div>

      <!-- Inventory Table -->
      <div *ngIf="inventory.length > 0" class="mb-6">
        <h3 class="text-lg font-semibold mb-2">Inventario Disponible</h3>
        <table class="min-w-full bg-white border">
          <thead>
            <tr class="bg-gray-100">
              <th class="p-2 border text-left">Producto</th>
              <th class="p-2 border text-left">SKU</th>
              <th class="p-2 border text-right">Stock Físico</th>
              <th class="p-2 border text-right">Reservado</th>
              <th class="p-2 border text-right">Disponible</th>
              <th class="p-2 border text-center">Pedir</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of inventory">
              <td class="p-2 border">{{ item.productName }}</td>
              <td class="p-2 border">{{ item.skuCode }}</td>
              <td class="p-2 border text-right">{{ item.onHand }}</td>
              <td class="p-2 border text-right text-yellow-600">{{ item.reserved }}</td>
              <td class="p-2 border text-right font-bold" [class.text-red-500]="(item.onHand - item.reserved) <= 0">
                {{ item.onHand - item.reserved }}
              </td>
              <td class="p-2 border text-center">
                <input type="number" [(ngModel)]="cart[item.skuId]" min="0" [max]="item.onHand - item.reserved"
                       class="w-20 border rounded p-1 text-center">
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Action -->
      <div *ngIf="inventory.length > 0" class="flex justify-end">
        <button (click)="submitOrder()" 
                class="bg-green-600 text-white px-6 py-2 rounded text-lg font-bold hover:bg-green-700 disabled:opacity-50"
                [disabled]="isCartEmpty() || processing">
          {{ processing ? 'Procesando...' : 'Confirmar Orden' }}
        </button>
      </div>

      <!-- Feedback -->
      <div *ngIf="message" class="mt-4 p-4 rounded" 
           [ngClass]="success ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
        {{ message }}
      </div>
    </div>
  `
})
export class OrderCreateComponent {
  storeId: number = 1;
  inventory: InventoryItem[] = [];
  cart: { [skuId: number]: number } = {};
  
  loading = false;
  processing = false;
  message = '';
  success = false;

  constructor(private api: ApiService) {}

  loadInventory() {
    this.loading = true;
    this.message = '';
    this.api.getInventory(this.storeId).subscribe({
      next: (data) => {
        this.inventory = data;
        this.cart = {}; // Reset cart
        this.loading = false;
      },
      error: (err) => {
        this.message = 'Error cargando inventario. Asegúrate que la API corre.';
        this.success = false;
        this.loading = false;
      }
    });
  }

  isCartEmpty(): boolean {
    return Object.values(this.cart).every(q => !q || q <= 0);
  }

  submitOrder() {
    const items: CreateOrderItem[] = Object.keys(this.cart)
      .map(skuId => ({
        skuId: Number(skuId),
        quantity: this.cart[Number(skuId)]
      }))
      .filter(i => i.quantity > 0);

    if (items.length === 0) return;

    this.processing = true;
    this.message = '';

    this.api.createOrder({ storeId: this.storeId, items }).subscribe({
      next: (res) => {
        this.success = true;
        this.message = `Orden #${res.orderId} creada exitosamente! Estado: ${res.status}`;
        this.processing = false;
        this.loadInventory(); // Refresh stock
      },
      error: (err) => {
        this.success = false;
        this.processing = false;
        if (err.status === 409) {
          if (err.error.error === 'INSUFFICIENT_STOCK') {
            this.message = 'Error: Stock insuficiente. Alguien más compró los items antes que tú.';
          } else {
            this.message = 'Error de conflicto (Idempotencia o Concurrencia). Intenta de nuevo.';
          }
        } else {
          this.message = 'Error creando la orden. Revisa la consola.';
        }
        console.error(err);
      }
    });
  }
}
