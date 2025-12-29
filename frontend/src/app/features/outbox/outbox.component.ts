import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-outbox',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 space-y-6">
      <header>
        <h2 class="text-3xl font-black text-slate-800">Buzón de Salida</h2>
        <p class="text-slate-500">Monitor de eventos de integración asíncrona.</p>
      </header>

      <div class="bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm">
        <table class="w-full text-left">
          <thead class="bg-slate-50 text-slate-400 text-[10px] uppercase font-black tracking-widest border-b border-slate-100">
            <tr>
              <th class="px-6 py-4">ID</th>
              <th class="px-6 py-4">Tipo de Evento</th>
              <th class="px-6 py-4">Ocurrido</th>
              <th class="px-6 py-4">Procesado</th>
              <th class="px-6 py-4">Estado</th>
              <th class="px-6 py-4">Payload</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr *ngFor="let event of events()" class="hover:bg-slate-50/50 transition-colors">
              <td class="px-6 py-4 font-mono text-xs text-slate-500">
                {{ event.id | slice:0:8 }}...
              </td>
              <td class="px-6 py-4">
                <span class="font-bold text-slate-700 bg-slate-100 px-2 py-1 rounded text-xs">{{ event.type }}</span>
              </td>
              <td class="px-6 py-4 text-xs text-slate-500">
                {{ event.occurredOn | date:'medium' }}
              </td>
              <td class="px-6 py-4 text-xs text-slate-500">
                {{ event.processedOn ? (event.processedOn | date:'medium') : '-' }}
              </td>
              <td class="px-6 py-4">
                <span *ngIf="event.processedOn" class="text-emerald-600 bg-emerald-50 px-2 py-1 rounded text-xs font-black uppercase tracking-tighter">Enviado</span>
                <span *ngIf="!event.processedOn && !event.error" class="text-amber-600 bg-amber-50 px-2 py-1 rounded text-xs font-black uppercase tracking-tighter">Pendiente</span>
                <span *ngIf="event.error" class="text-rose-600 bg-rose-50 px-2 py-1 rounded text-xs font-black uppercase tracking-tighter" [title]="event.error">Fallido</span>
              </td>
              <td class="px-6 py-4">
                <div class="max-w-xs group relative">
                  <pre class="text-[10px] bg-slate-50 text-slate-500 p-2 rounded border border-slate-200 overflow-hidden text-ellipsis whitespace-nowrap group-hover:whitespace-pre-wrap group-hover:overflow-visible group-hover:absolute group-hover:z-10 group-hover:bg-white group-hover:shadow-lg group-hover:w-96 transition-all cursor-pointer">
{{ event.payloadJson }}
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
export class OutboxComponent implements OnInit {
  events = signal<any[]>([]);

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.getOutboxEvents().subscribe(data => {
      this.events.set(data);
    });
  }
}
