import { Component, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { StoreService } from '../../services/store.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 max-w-5xl mx-auto space-y-6">
      <header class="flex justify-between items-end">
        <div>
          <h2 class="text-3xl font-black text-slate-800">Centro de Alertas</h2>
          <p class="text-slate-500">Gestiona las anomalías de inventario y sistema.</p>
        </div>
        
        <div class="flex bg-slate-200 p-1 rounded-xl">
          <button (click)="setStatusFilter('open')" 
                  [class.bg-white]="statusFilter() === 'open'" [class.shadow-sm]="statusFilter() === 'open'"
                  class="px-4 py-1.5 rounded-lg text-sm font-bold transition-all">Abiertas</button>
          <button (click)="setStatusFilter('acknowledged')" 
                  [class.bg-white]="statusFilter() === 'acknowledged'" [class.shadow-sm]="statusFilter() === 'acknowledged'"
                  class="px-4 py-1.5 rounded-lg text-sm font-bold transition-all">En Proceso</button>
          <button (click)="setStatusFilter('resolved')" 
                  [class.bg-white]="statusFilter() === 'resolved'" [class.shadow-sm]="statusFilter() === 'resolved'"
                  class="px-4 py-1.5 rounded-lg text-sm font-bold transition-all">Resueltas</button>
        </div>
      </header>

      <div class="space-y-4">
        <div *ngFor="let alert of alerts()" 
             class="bg-white border border-slate-200 rounded-2xl p-5 flex justify-between items-center hover:border-indigo-300 transition-colors shadow-sm">
          <div class="flex gap-4 items-center">
            <div [ngClass]="{
              'bg-rose-100 text-rose-600': alert.status === 'open',
              'bg-amber-100 text-amber-600': alert.status === 'acknowledged',
              'bg-emerald-100 text-emerald-600': alert.status === 'resolved'
            }" class="w-12 h-12 rounded-full flex items-center justify-center text-xl">
              <span *ngIf="alert.status === 'open'">⚠️</span>
              <span *ngIf="alert.status === 'acknowledged'">⏳</span>
              <span *ngIf="alert.status === 'resolved'">✅</span>
            </div>
            
            <div>
              <div class="flex items-center gap-2">
                <span class="text-[10px] font-black uppercase tracking-tighter px-2 py-0.5 rounded bg-slate-100 text-slate-500 border border-slate-200">
                  {{ alert.skuCode }}
                </span>
                <span class="text-xs text-slate-400 font-medium">{{ alert.createdAt | date:'short' }}</span>
              </div>
              <h4 class="font-bold text-slate-800 text-lg">{{ alert.productName }}</h4>
              <p class="text-slate-600 text-sm">{{ alert.message }}</p>
            </div>
          </div>

          <div class="flex gap-2">
            <button *ngIf="alert.status === 'open'" 
                    (click)="ack(alert.id)"
                    class="bg-white border border-slate-200 text-slate-700 px-4 py-2 rounded-xl text-sm font-bold hover:bg-slate-50">
              Atender
            </button>
            <button *ngIf="alert.status !== 'resolved'" 
                    (click)="resolve(alert.id)"
                    class="bg-indigo-600 text-white px-4 py-2 rounded-xl text-sm font-bold hover:bg-indigo-700">
              Resolver
            </button>
          </div>
        </div>

        <div *ngIf="alerts().length === 0" class="py-20 text-center bg-white border border-dashed border-slate-300 rounded-3xl">
          <p class="text-slate-400 italic font-medium">No hay alertas para mostrar con el filtro seleccionado.</p>
        </div>
      </div>
    </div>
  `
})
export class AlertsComponent {
  alerts = signal<any[]>([]);
  statusFilter = signal<string>('open');

  constructor(
    private api: ApiService,
    private storeService: StoreService
  ) {
    effect(() => {
      this.loadAlerts();
    });
  }

  setStatusFilter(status: string) {
    this.statusFilter.set(status);
  }

  private loadAlerts() {
    this.api.getAlerts(this.storeService.selectedStoreId(), this.statusFilter()).subscribe(data => {
      this.alerts.set(data);
    });
  }

  ack(id: number) {
    this.api.ackAlert(id).subscribe(() => this.loadAlerts());
  }

  resolve(id: number) {
    this.api.resolveAlert(id).subscribe(() => this.loadAlerts());
  }
}
