import { Routes } from '@angular/router';
import { OrderCreateComponent } from './features/orders/order-create.component';
import { OrderListComponent } from './features/orders/order-list.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
    { path: 'dashboard', component: DashboardComponent },
    { path: 'orders/create', component: OrderCreateComponent },
    { path: 'orders/list', component: OrderListComponent },
    { path: 'alerts', loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent) },
    { path: 'audit', loadComponent: () => import('./features/audit/audit.component').then(m => m.AuditComponent) },
    { path: 'outbox', loadComponent: () => import('./features/outbox/outbox.component').then(m => m.OutboxComponent) },
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
