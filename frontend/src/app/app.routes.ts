import { Routes } from '@angular/router';
import { OrderCreateComponent } from './features/orders/order-create.component';
import { OrderListComponent } from './features/orders/order-list.component';

export const routes: Routes = [
    { path: 'orders/create', component: OrderCreateComponent },
    { path: 'orders/list', component: OrderListComponent },
    { path: '', redirectTo: 'orders/create', pathMatch: 'full' }
];
