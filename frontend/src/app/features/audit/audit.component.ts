import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 space-y-6">
      <header>
        <h2 class="text-3xl font-black text-slate-800">Trazabilidad Total</h2>
        <p class="text-slate-500">Historial completo de modificaciones en el sistema.</p>
      </header>

      <div class="bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm">
        <table class="w-full text-left">
          <thead class="bg-slate-50 text-slate-400 text-[10px] uppercase font-black tracking-widest border-b border-slate-100">
            <tr>
              <th class="px-6 py-4">Fecha</th>
              <th class="px-6 py-4">Entidad / Acción</th>
              <th class="px-6 py-4">ID</th>
              <th class="px-6 py-4">Usuario</th>
              <th class="px-6 py-4">Detalle de Cambios</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr *ngFor="let log of logs()" class="hover:bg-slate-50/50 transition-colors">
              <td class="px-6 py-4 text-xs font-medium text-slate-400">
                {{ log.timestamp | date:'medium' }}
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <span class="font-bold text-slate-700">{{ log.entityName }}</span>
                  <span [ngClass]="{
                    'text-emerald-600 bg-emerald-50': log.action === 'Added',
                    'text-indigo-600 bg-indigo-50': log.action === 'Modified',
                    'text-rose-600 bg-rose-50': log.action === 'Deleted'
                  }" class="text-[10px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter">
                    {{ log.action }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 font-mono text-sm text-slate-500">
                #{{ log.entityId }}
              </td>
              <td class="px-6 py-4 text-sm font-semibold text-slate-600">
                {{ log.userId }}
              </td>
              <td class="px-6 py-4">
                <div class="max-w-md">
                  <pre class="text-[10px] bg-slate-900 text-slate-300 p-3 rounded-lg overflow-x-auto font-mono leading-relaxed">
{{ formatChanges(log.changesJson) }}
                  </pre>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class AuditComponent implements OnInit {
  logs = signal<any[]>([]);

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.getAuditLogs().subscribe(data => {
      this.logs.set(data);
    });
  }

  formatChanges(json: string): string {
    try {
      const obj = JSON.parse(json);
      return JSON.stringify(obj, null, 2);
    } catch {
      return json;
    }
  }
}
