import { Component, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { StoreService } from '../../services/store.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 space-y-8">
      <header>
        <h2 class="text-3xl font-black text-slate-800">Panel de Control</h2>
        <p class="text-slate-500">Métricas clave y estado de inventario en tiempo real.</p>
      </header>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
          <p class="text-sm font-bold text-slate-400 uppercase tracking-wider">Ventas Totales</p>
          <h3 class="text-4xl font-black text-slate-800 mt-1">
            \${{ summary()?.totalAmount | number:'1.2-2' }}
          </h3>
          <div class="mt-2 flex items-center gap-2 font-bold text-sm"
               [ngClass]="(summary()?.growthPercentage || 0) >= 0 ? 'text-emerald-600' : 'text-rose-600'">
            <span>
              {{ (summary()?.growthPercentage || 0) >= 0 ? '↑' : '↓' }} 
              {{ summary()?.growthPercentage | number:'1.0-1' }}% vs ayer
            </span>
          </div>
        </div>

        <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
          <p class="text-sm font-bold text-slate-400 uppercase tracking-wider">Órdenes Procesadas</p>
          <h3 class="text-4xl font-black text-slate-800 mt-1">
            {{ summary()?.totalOrders || 0 }}
          </h3>
          <p class="text-slate-500 text-sm mt-2">{{ summary()?.totalItems || 0 }} items en total</p>
        </div>

        <div class="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
          <p class="text-sm font-bold text-slate-400 uppercase tracking-wider">Estado del Stock</p>
          <div class="mt-2 space-y-1">
            <div class="flex justify-between items-center">
              <span class="text-sm font-medium text-slate-600">Reserved:</span>
              <span class="bg-amber-100 text-yellow-700 px-2 py-0.5 rounded-full text-xs font-bold">{{ summary()?.countsByStatus['Reserved'] || 0 }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-sm font-medium text-slate-600">Draft:</span>
              <span class="bg-slate-100 text-slate-500 px-2 py-0.5 rounded-full text-xs font-bold">{{ summary()?.countsByStatus['Draft'] || 0 }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Critical Stock Table -->
      <section class="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div class="p-6 border-b border-slate-100 flex justify-between items-center">
          <h3 class="font-black text-slate-800 text-xl italic underline decoration-rose-500 underline-offset-4">Stock Crítico (Re-abastecer)</h3>
          <span class="bg-rose-500 text-white px-3 py-1 rounded-full text-xs font-bold animate-pulse">Atención Inmediata</span>
        </div>
        
        <table class="w-full text-left">
          <thead class="bg-slate-50 text-slate-400 text-[10px] uppercase font-black tracking-widest">
            <tr>
              <th class="px-6 py-4">Producto / Sku</th>
              <th class="px-6 py-4">Marca</th>
              <th class="px-6 py-4 text-center">Disponible</th>
              <th class="px-6 py-4 text-center">Punto Reorden</th>
              <th class="px-6 py-4 text-center">Salud</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr *ngFor="let item of criticalStock()" class="hover:bg-slate-50 transition-colors">
              <td class="px-6 py-4">
                <p class="font-bold text-slate-800">{{ item.productName }}</p>
                <p class="text-xs font-mono text-slate-400">{{ item.skuCode }}</p>
              </td>
              <td class="px-6 py-4 text-sm text-slate-600">{{ item.brand }}</td>
              <td class="px-6 py-4 text-center">
                <span class="text-lg font-black" [class.text-rose-600]="item.available <= 0">{{ item.available }}</span>
              </td>
              <td class="px-6 py-4 text-center text-slate-400 font-medium">{{ item.reorderPoint }}</td>
              <td class="px-6 py-4">
                <div class="w-full bg-slate-100 rounded-full h-2">
                  <div class="bg-rose-500 h-2 rounded-full" [style.width.%]="(item.available / item.reorderPoint) * 50"></div>
                </div>
              </td>
            </tr>
            <tr *ngIf="criticalStock().length === 0">
              <td colspan="5" class="px-6 py-12 text-center text-slate-400 italic">
                No se detectaron problemas de stock en esta tienda.
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Top SKUs Table -->
      <section class="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div class="p-6 border-b border-slate-100">
          <h3 class="font-black text-slate-800 text-xl">Top SKUs Más Vendidos</h3>
        </div>
        
        <table class="w-full text-left">
          <thead class="bg-slate-50 text-slate-400 text-[10px] uppercase font-black tracking-widest">
            <tr>
              <th class="px-6 py-4">Producto</th>
              <th class="px-6 py-4 text-right">Cant. Vendida</th>
              <th class="px-6 py-4 text-right">Ingresos</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr *ngFor="let item of topSkus()" class="hover:bg-slate-50 transition-colors">
              <td class="px-6 py-4">
                <p class="font-bold text-slate-800">{{ item.productName }}</p>
                <p class="text-xs font-mono text-slate-400">{{ item.skuCode }}</p>
              </td>
              <td class="px-6 py-4 text-right font-black text-indigo-600">{{ item.totalQuantity }}</td>
              <td class="px-6 py-4 text-right font-medium text-slate-600">\${{ item.totalRevenue | number:'1.2-2' }}</td>
            </tr>
            <tr *ngIf="topSkus().length === 0">
              <td colspan="3" class="px-6 py-12 text-center text-slate-400 italic">
                Aún no hay ventas registradas para generar este reporte.
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>
  `
})
export class DashboardComponent {
  summary = signal<any>(null);
  criticalStock = signal<any[]>([]);
  topSkus = signal<any[]>([]);

  constructor(
    private api: ApiService,
    private storeService: StoreService
  ) {
    // React to store changes automatically
    effect(() => {
      const storeId = this.storeService.selectedStoreId();
      this.refreshData(storeId);
    });
  }

  private refreshData(storeId: number) {
    this.api.getOrdersSummary(storeId).subscribe(data => this.summary.set(data));
    this.api.getCriticalStock(storeId).subscribe(data => this.criticalStock.set(data));
    this.api.getTopSkus(storeId).subscribe(data => this.topSkus.set(data));
  }
}
