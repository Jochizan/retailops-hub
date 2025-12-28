import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <nav class="bg-gray-800 text-white p-4">
      <div class="container mx-auto flex gap-6">
        <h1 class="font-bold text-xl">RetailOps</h1>
        <a routerLink="/orders/create" class="hover:text-gray-300">Crear Orden</a>
        <a routerLink="/orders/list" class="hover:text-gray-300">Listado</a>
      </div>
    </nav>
    <main class="bg-gray-50 min-h-screen">
      <router-outlet></router-outlet>
    </main>
  `
})
export class App {
}
