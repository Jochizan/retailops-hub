import { Component, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from './services/api.service';
import { StoreService } from './services/store.service';

@Component({
  selector: 'app-root',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="bg-indigo-900 text-white shadow-xl">
      <div class="container mx-auto px-4">
        <div class="flex justify-between items-center h-16">
          <!-- Logo & Links -->
          <div class="flex items-center gap-8">
            <h1 class="font-black text-2xl tracking-tighter text-indigo-100">RetailOps <span class="text-indigo-400">Hub</span></h1>

            <div class="flex gap-1">
              <a routerLink="/dashboard" routerLinkActive="bg-indigo-800 text-white"
                 class="px-4 py-2 rounded-lg text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white transition-all">
                Dashboard
              </a>
              <a routerLink="/orders/create" routerLinkActive="bg-indigo-800 text-white"
                 class="px-4 py-2 rounded-lg text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white transition-all">
                Ventas
              </a>
              <a routerLink="/alerts" routerLinkActive="bg-indigo-800 text-white"
                 class="px-4 py-2 rounded-lg text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white transition-all">
                Alertas
              </a>
              <a routerLink="/audit" routerLinkActive="bg-indigo-800 text-white"
                 class="px-4 py-2 rounded-lg text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white transition-all">
                Auditoría
              </a>
              <a routerLink="/outbox" routerLinkActive="bg-indigo-800 text-white"
                 class="px-4 py-2 rounded-lg text-sm font-medium text-indigo-200 hover:bg-indigo-800 hover:text-white transition-all">
                Outbox
              </a>
            </div>
          </div>

          <!-- Controls -->
          <div class="flex items-center gap-4">
            <!-- Role Picker (Demo Mode) -->
            <div class="flex items-center bg-white/10 border border-white/20 rounded-lg px-3 py-1.5 gap-2">
              <span class="text-[10px] uppercase font-black text-indigo-300">Rol:</span>
              <select [value]="storeService.userRole()" 
                      (change)="onRoleChange($event)"
                      class="bg-transparent text-sm font-bold focus:outline-none cursor-pointer">
                <option value="admin" class="text-gray-900">Admin</option>
                <option value="manager" class="text-gray-900">Manager</option>
                <option value="clerk" class="text-gray-900">Clerk</option>
              </select>
            </div>

            <!-- Store Picker -->
            <div class="flex items-center bg-indigo-800/50 border border-indigo-700 rounded-lg px-3 py-1.5 gap-2">
              <span class="text-xs uppercase font-bold text-indigo-300">Tienda:</span>
              <select [value]="storeService.selectedStoreId()" 
                      (change)="onStoreChange($event)"
                      class="bg-transparent text-sm font-bold focus:outline-none cursor-pointer">
                <option *ngFor="let s of stores()" [value]="s.id" class="text-gray-900">{{ s.name }}</option>
              </select>
            </div>
            <div class="bg-emerald-500/20 text-emerald-400 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border border-emerald-500/30">
              Live
            </div>
          </div>
        </div>
      </div>
    </nav>

    <main class="bg-slate-50 min-h-[calc(100vh-4rem)]">
      <router-outlet></router-outlet>
    </main>
  `
})
export class App implements OnInit {
  stores = signal<any[]>([]);

  constructor(
    private api: ApiService,
    public storeService: StoreService
  ) {}

  ngOnInit() {
    this.api.getStores().subscribe(data => {
      this.stores.set(data);
      this.storeService.stores.set(data); // Sync with service
    });
  }

  onStoreChange(event: any) {
    const id = parseInt(event.target.value, 10);
    this.storeService.setStore(id);
  }

  onRoleChange(event: any) {
    this.storeService.setRole(event.target.value);
  }
}
